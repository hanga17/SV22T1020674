using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Partner;

namespace SV22T1020674.DataLayers.SQLServer
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        public UserAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ===== LOGIN =====
        public async Task<Customer?> LoginAsync(string email, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Customers
                WHERE Email = @Email AND Password = @Password";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new
            {
                Email = email,
                Password = password
            });
        }

        // ===== CHECK EMAIL =====
        public async Task<int> CheckEmailExistsAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT COUNT(*) FROM Customers WHERE Email = @Email";

            return await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        }

        // ===== REGISTER =====
        public async Task<bool> RegisterAsync(string fullName, string address, string phone, string email, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                INSERT INTO Customers(CustomerName, ContactName, Address, Phone, Email, Password, IsLocked)
                VALUES(@Name, @Name, @Address, @Phone, @Email, @Password, 0)";

            int rows = await connection.ExecuteAsync(sql, new
            {
                Name = fullName,
                Address = address,
                Phone = phone,
                Email = email,
                Password = password
            });

            return rows > 0;
        }

        // ===== GET CUSTOMER =====
        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT * FROM Customers WHERE CustomerID = @CustomerID";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { CustomerID = customerId });
        }

        // ===== CHECK OLD PASSWORD =====
        public async Task<Customer?> CheckOldPasswordAsync(int customerId, string password)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT * FROM Customers
                WHERE CustomerID = @CustomerID AND Password = @Password";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new
            {
                CustomerID = customerId,
                Password = password
            });
        }

        // ===== UPDATE PASSWORD =====
        public async Task<bool> UpdatePasswordAsync(int customerId, string newPassword)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Customers
                SET Password = @Password
                WHERE CustomerID = @CustomerID";

            int rows = await connection.ExecuteAsync(sql, new
            {
                Password = newPassword,
                CustomerID = customerId
            });

            return rows > 0;
        }

        // ===== PROFILE =====
        public async Task<Customer?> GetProfileAsync(int customerId)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"SELECT * FROM Customers WHERE CustomerID = @CustomerID";

            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { CustomerID = customerId });
        }

        // ===== UPDATE PROFILE =====
        public async Task<bool> UpdateProfileAsync(Customer model)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                UPDATE Customers
                SET CustomerName = @CustomerName,
                    Phone = @Phone,
                    Address = @Address
                WHERE CustomerID = @CustomerID";

            int rows = await connection.ExecuteAsync(sql, model);

            return rows > 0;
        }
    }
}
