using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Catalog;
using SV22T1020674.Models.Common;

namespace SV22T1020674.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thao tác dữ liệu với bảng Categories trong CSDL SQL Server
    /// </summary>
    public class CategoryRepository : IGenericRepository<Category>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public CategoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Bổ sung một loại hàng mới vào CSDL
        /// </summary>        /// <param name="data">Dữ liệu loại hàng cần thêm</param>
        /// <returns>Mã CategoryID của loại hàng vừa được thêm</returns>
        public async Task<int> AddAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Categories (CategoryName, Description)
                VALUES (@CategoryName, @Description);
                
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Xóa bản ghi loại hàng theo mã ID
        /// </summary>
        /// <param name="id">Mã loại hàng cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM Categories WHERE CategoryID = @CategoryID";

            int rowsAffected = await connection.ExecuteAsync(sql, new { CategoryID = id });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một loại hàng theo mã (trả về null nếu không có)
        /// </summary>
        /// <param name="id">Mã loại hàng cần lấy</param>
        /// <returns>Đối tượng Category hoặc null</returns>
        public async Task<Category?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Categories WHERE CategoryID = @CategoryID";

            return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { CategoryID = id });
        }

        /// <summary>
        /// Kiểm tra xem loại hàng có đang được sử dụng trong bảng Products hay không
        /// </summary>
        /// <param name="id">Mã loại hàng cần kiểm tra</param>
        /// <returns>True nếu đang được sử dụng (có mặt hàng thuộc loại này), False nếu chưa có</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS(SELECT * FROM Products WHERE CategoryID = @CategoryID)
                    SELECT 1
                ELSE
                    SELECT 0";

            return await connection.ExecuteScalarAsync<bool>(sql, new { CategoryID = id });
        }

        /// <summary>
        /// Truy vấn, tìm kiếm loại hàng và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Đối tượng PagedResult chứa thông tin phân trang và danh sách dữ liệu</returns>
        public async Task<PagedResult<Category>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Category>
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            // Tạo từ khóa tìm kiếm (tìm tương đối theo Tên loại hàng)
            string searchValue = string.IsNullOrWhiteSpace(input.SearchValue) ? "" : $"%{input.SearchValue}%";

            using var connection = new SqlConnection(_connectionString);

            // 1. Đếm tổng số dòng thỏa điều kiện tìm kiếm
            string sqlCount = @"
                SELECT COUNT(*) 
                FROM Categories 
                WHERE (@SearchValue = N'') 
                   OR (CategoryName LIKE @SearchValue)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new { SearchValue = searchValue });

            // 2. Lấy dữ liệu phân trang nếu có kết quả
            if (result.RowCount > 0)
            {
                string sqlList = @"
                    SELECT * FROM Categories 
                    WHERE (@SearchValue = N'') 
                       OR (CategoryName LIKE @SearchValue)
                    ORDER BY CategoryName";

                // Thêm điều kiện phân trang nếu PageSize > 0
                if (input.PageSize > 0)
                {
                    sqlList += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                }

                var data = await connection.QueryAsync<Category>(sqlList, new
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
        /// Cập nhật thông tin của một loại hàng
        /// </summary>
        /// <param name="data">Dữ liệu loại hàng cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> UpdateAsync(Category data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Categories
                SET CategoryName = @CategoryName,
                    Description = @Description
                WHERE CategoryID = @CategoryID";

            int rowsAffected = await connection.ExecuteAsync(sql, data);
            return rowsAffected > 0;
        }
    }
}