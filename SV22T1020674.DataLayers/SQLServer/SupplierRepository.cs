using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Common;
using SV22T1020674.Models.Partner;

namespace SV22T1020674.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thao tác dữ liệu với bảng Suppliers trong CSDL SQL Server
    /// </summary>
    public class SupplierRepository : IGenericRepository<Supplier>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public SupplierRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Bổ sung một bản ghi nhà cung cấp vào CSDL
        /// </summary>
        /// <param name="data">Dữ liệu nhà cung cấp cần thêm</param>
        /// <returns>Mã SupplierID của nhà cung cấp vừa được thêm</returns>
        public async Task<int> AddAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Suppliers (SupplierName, ContactName, Province, Address, Phone, Email)
                VALUES (@SupplierName, @ContactName, @Province, @Address, @Phone, @Email);
                
                SELECT SCOPE_IDENTITY();";

            // Dùng ExecuteScalarAsync để lấy về giá trị ID vừa được sinh tự động
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Xóa bản ghi nhà cung cấp có mã là id
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM Suppliers WHERE SupplierID = @SupplierID";

            int rowsAffected = await connection.ExecuteAsync(sql, new { SupplierID = id });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Lấy dữ liệu của một nhà cung cấp theo mã (trả về null nếu không tìm thấy)
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần lấy</param>
        /// <returns>Đối tượng Supplier hoặc null</returns>
        public async Task<Supplier?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Suppliers WHERE SupplierID = @SupplierID";

            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { SupplierID = id });
        }

        /// <summary>
        /// Kiểm tra xem nhà cung cấp có dữ liệu liên quan trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần kiểm tra</param>
        /// <returns>True nếu đang được sử dụng (có sản phẩm), False nếu chưa có</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS(SELECT * FROM Products WHERE SupplierID = @SupplierID)
                    SELECT 1
                ELSE
                    SELECT 0";

            return await connection.ExecuteScalarAsync<bool>(sql, new { SupplierID = id });
        }

        /// <summary>
        /// Truy vấn, tìm kiếm nhà cung cấp và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang (SearchValue, Page, PageSize)</param>
        /// <returns>Đối tượng PagedResult chứa thông tin phân trang và danh sách dữ liệu</returns>
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Supplier>
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            // Tạo từ khóa tìm kiếm cho phép tìm gần đúng bằng mệnh đề LIKE
            string searchValue = string.IsNullOrWhiteSpace(input.SearchValue) ? "" : $"%{input.SearchValue}%";

            using var connection = new SqlConnection(_connectionString);

            // 1. Lấy tổng số dòng dữ liệu thỏa điều kiện
            string sqlCount = @"
                SELECT COUNT(*) 
                FROM Suppliers 
                WHERE (@SearchValue = N'') 
                   OR (SupplierName LIKE @SearchValue) 
                   OR (ContactName LIKE @SearchValue)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new { SearchValue = searchValue });

            // 2. Nếu có dữ liệu thì lấy danh sách phân trang
            if (result.RowCount > 0)
            {
                string sqlList = @"
                    SELECT * FROM Suppliers 
                    WHERE (@SearchValue = N'') 
                       OR (SupplierName LIKE @SearchValue) 
                       OR (ContactName LIKE @SearchValue)
                    ORDER BY SupplierName";

                // Nếu có phân trang (PageSize > 0) thì bổ sung điều kiện bỏ qua và lấy n dòng
                if (input.PageSize > 0)
                {
                    sqlList += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                }

                var data = await connection.QueryAsync<Supplier>(sqlList, new
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
        /// Cập nhật thông tin một nhà cung cấp trong CSDL
        /// </summary>
        /// <param name="data">Dữ liệu nhà cung cấp cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy</returns>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Suppliers
                SET SupplierName = @SupplierName,
                    ContactName = @ContactName,
                    Province = @Province,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email
                WHERE SupplierID = @SupplierID";

            int rowsAffected = await connection.ExecuteAsync(sql, data);
            return rowsAffected > 0;
        }
    }
}