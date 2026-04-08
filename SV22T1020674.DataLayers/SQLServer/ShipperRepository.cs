using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Common;
using SV22T1020674.Models.Partner;

namespace SV22T1020674.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp thao tác dữ liệu với bảng Shippers trong CSDL SQL Server
    /// </summary>
    public class ShipperRepository : IGenericRepository<Shipper>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Khởi tạo repository với chuỗi kết nối
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến SQL Server</param>
        public ShipperRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Bổ sung một người giao hàng mới vào CSDL
        /// </summary>
        /// <param name="data">Dữ liệu người giao hàng cần thêm</param>
        /// <returns>Mã ShipperID của người giao hàng vừa được thêm</returns>
        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Shippers (ShipperName, Phone)
                VALUES (@ShipperName, @Phone);
                
                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Xóa bản ghi người giao hàng theo mã ID
        /// </summary>
        /// <param name="id">Mã người giao hàng cần xóa</param>
        /// <returns>True nếu xóa thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM Shippers WHERE ShipperID = @ShipperID";

            int rowsAffected = await connection.ExecuteAsync(sql, new { ShipperID = id });
            return rowsAffected > 0;
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một người giao hàng theo mã (trả về null nếu không có)
        /// </summary>
        /// <param name="id">Mã người giao hàng cần lấy</param>
        /// <returns>Đối tượng Shipper hoặc null</returns>
        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Shippers WHERE ShipperID = @ShipperID";

            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { ShipperID = id });
        }

        /// <summary>
        /// Kiểm tra xem người giao hàng có đang được sử dụng trong bảng Orders hay không
        /// </summary>
        /// <param name="id">Mã người giao hàng cần kiểm tra</param>
        /// <returns>True nếu đang được sử dụng (có đơn hàng), False nếu chưa có</returns>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS(SELECT * FROM Orders WHERE ShipperID = @ShipperID)
                    SELECT 1
                ELSE
                    SELECT 0";

            return await connection.ExecuteScalarAsync<bool>(sql, new { ShipperID = id });
        }

        /// <summary>
        /// Truy vấn, tìm kiếm người giao hàng và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Đối tượng PagedResult chứa thông tin phân trang và danh sách dữ liệu</returns>
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Shipper>
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            // Tạo từ khóa tìm kiếm (tìm tương đối theo Tên)
            string searchValue = string.IsNullOrWhiteSpace(input.SearchValue) ? "" : $"%{input.SearchValue}%";

            using var connection = new SqlConnection(_connectionString);

            // 1. Đếm tổng số dòng thỏa điều kiện tìm kiếm
            string sqlCount = @"
                SELECT COUNT(*) 
                FROM Shippers 
                WHERE (@SearchValue = N'') 
                   OR (ShipperName LIKE @SearchValue)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new { SearchValue = searchValue });

            // 2. Lấy dữ liệu phân trang nếu có kết quả
            if (result.RowCount > 0)
            {
                string sqlList = @"
                    SELECT * FROM Shippers 
                    WHERE (@SearchValue = N'') 
                       OR (ShipperName LIKE @SearchValue)
                    ORDER BY ShipperName";

                // Thêm điều kiện phân trang nếu PageSize > 0
                if (input.PageSize > 0)
                {
                    sqlList += " OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                }

                var data = await connection.QueryAsync<Shipper>(sqlList, new
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
        /// Cập nhật thông tin của một người giao hàng
        /// </summary>
        /// <param name="data">Dữ liệu người giao hàng cần cập nhật</param>
        /// <returns>True nếu cập nhật thành công, False nếu không tìm thấy bản ghi</returns>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Shippers
                SET ShipperName = @ShipperName,
                    Phone = @Phone
                WHERE ShipperID = @ShipperID";

            int rowsAffected = await connection.ExecuteAsync(sql, data);
            return rowsAffected > 0;
        }
    }
}