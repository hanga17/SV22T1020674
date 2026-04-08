using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Common;
using SV22T1020674.Models.Partner;

namespace SV22T1020674.DataLayers.SQLServer
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ===== Thêm khách hàng =====
        public async Task<int> AddAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, @IsLocked);
                SELECT CAST(SCOPE_IDENTITY() AS int);";
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        // ===== Xóa khách hàng =====
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM Customers WHERE CustomerID = @CustomerID";
            return await connection.ExecuteAsync(sql, new { CustomerID = id }) > 0;
        }

        // ===== Lấy khách hàng theo ID =====
        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Customers WHERE CustomerID = @CustomerID";
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { CustomerID = id });
        }

        // ===== Lấy khách hàng theo Email =====
        public Customer? GetByEmail(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Customers WHERE Email = @Email";
            return connection.QueryFirstOrDefault<Customer>(sql, new { Email = email });
        }

        // ===== Kiểm tra email hợp lệ =====
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT COUNT(*) FROM Customers WHERE Email = @Email AND CustomerID <> @CustomerID";
            int count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, CustomerID = id });
            return count == 0;
        }

        // ===== Kiểm tra khách hàng có đơn hàng =====
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS(SELECT 1 FROM Orders WHERE CustomerID = @CustomerID)
                    SELECT 1 ELSE SELECT 0";
            return await connection.ExecuteScalarAsync<bool>(sql, new { CustomerID = id });
        }

        // ===== Cập nhật khách hàng =====
        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Customers
                SET CustomerName = @CustomerName,
                    ContactName = @ContactName,
                    Province = @Province,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    IsLocked = @IsLocked,
                    Password = @Password
                WHERE CustomerID = @CustomerID";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        // ===== Lấy danh sách khách hàng có phân trang =====
        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Customer>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string searchValue = string.IsNullOrWhiteSpace(input.SearchValue) ? "" : $"%{input.SearchValue}%";

            using var connection = new SqlConnection(_connectionString);

            string sqlCount = @"
                SELECT COUNT(*) FROM Customers
                WHERE (@SearchValue = N'')
                   OR CustomerName LIKE @SearchValue
                   OR ContactName LIKE @SearchValue
                   OR Email LIKE @SearchValue";
            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new { SearchValue = searchValue });

            if (result.RowCount > 0)
            {
                string sqlList = @"
                    SELECT * FROM Customers
                    WHERE (@SearchValue = N'')
                       OR CustomerName LIKE @SearchValue
                       OR ContactName LIKE @SearchValue
                       OR Email LIKE @SearchValue
                    ORDER BY CustomerName
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var data = await connection.QueryAsync<Customer>(sqlList, new
                {
                    SearchValue = searchValue,
                    Offset = input.Offset,
                    PageSize = input.PageSize
                });
                result.DataItems = data.ToList();
            }

            return result;
        }
    }
}
