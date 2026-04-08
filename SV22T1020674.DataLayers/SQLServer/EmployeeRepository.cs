using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Common;
using SV22T1020674.Models.HR; // Gọi đúng namespace chứa lớp Employee

namespace SV22T1020674.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thao tác dữ liệu với bảng Employees trong CSDL SQL Server
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public EmployeeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Bổ sung một nhân viên mới vào CSDL
        /// </summary>
        public async Task<int> AddAsync(Employee data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Employees (FullName, BirthDate, Address, Phone, Email, Photo, IsWorking)
                VALUES (@FullName, @BirthDate, @Address, @Phone, @Email, @Photo, @IsWorking);
                
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Xóa bản ghi nhân viên theo mã ID
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM Employees WHERE EmployeeID = @EmployeeID";

            int rowsAffected = await connection.ExecuteAsync(sql, new { EmployeeID = id });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một nhân viên theo mã
        /// </summary>
        public async Task<Employee?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Employees WHERE EmployeeID = @EmployeeID";

            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { EmployeeID = id });
        }

        /// <summary>
        /// Kiểm tra xem nhân viên có đang được sử dụng (có đơn hàng) hay không
        /// </summary>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS(SELECT * FROM Orders WHERE EmployeeID = @EmployeeID)
                    SELECT 1
                ELSE
                    SELECT 0";

            return await connection.ExecuteScalarAsync<bool>(sql, new { EmployeeID = id });
        }

        /// <summary>
        /// Truy vấn, tìm kiếm nhân viên dưới dạng phân trang
        /// </summary>
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Employee>
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string searchValue = string.IsNullOrWhiteSpace(input.SearchValue) ? "" : $"%{input.SearchValue}%";

            using var connection = new SqlConnection(_connectionString);

            string sqlCount = @"
                SELECT COUNT(*) 
                FROM Employees 
                WHERE (@SearchValue = N'') 
                   OR (FullName LIKE @SearchValue) 
                   OR (Email LIKE @SearchValue)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new { SearchValue = searchValue });

            if (result.RowCount > 0)
            {
                string sqlList = @"
                    SELECT * FROM Employees 
                    WHERE (@SearchValue = N'') 
                       OR (FullName LIKE @SearchValue) 
                       OR (Email LIKE @SearchValue)
                    ORDER BY FullName";

                if (input.PageSize > 0)
                {
                    sqlList += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                }

                var data = await connection.QueryAsync<Employee>(sqlList, new
                {
                    SearchValue = searchValue,
                    Offset = input.Offset,
                    PageSize = input.PageSize
                });

                result.DataItems = data.ToList();
            }

            return result;
        }

        /// <summary>
        /// Cập nhật thông tin của nhân viên
        /// </summary>
        public async Task<bool> UpdateAsync(Employee data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Employees
                SET FullName = @FullName,
                    BirthDate = @BirthDate,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    Photo = @Photo,
                    IsWorking = @IsWorking
                WHERE EmployeeID = @EmployeeID";

            int rowsAffected = await connection.ExecuteAsync(sql, data);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Kiểm tra Email của nhân viên có bị trùng lặp không
        /// </summary>
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT COUNT(*) 
                FROM Employees 
                WHERE Email = @Email AND EmployeeID <> @EmployeeID";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email, EmployeeID = id });
            return count == 0;
        }
        public async Task<Employee?> AuthorizeAsync(string username, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT *
                       FROM Employees
                       WHERE Email = @username AND Password = @password";

                return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new
                {
                    username,
                    password
                });
            }
        }
        public async Task<bool> UpdatePasswordAsync(int employeeId, string newPassword)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
        UPDATE Employees
        SET Password = @Password
        WHERE EmployeeID = @EmployeeID";

            int rows = await connection.ExecuteAsync(sql, new
            {
                Password = newPassword,
                EmployeeID = employeeId
            });

            return rows > 0;
        }
    }
}