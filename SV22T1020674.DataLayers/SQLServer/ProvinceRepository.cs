using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.DataDictionary;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020674.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các phép xử lý dữ liệu cho bảng Province (Tỉnh/Thành)
    /// </summary>
    public class ProvinceRepository : IDataDictionaryRepository<Province>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Hàm khởi tạo nhận vào chuỗi kết nối CSDL
        /// </summary>
        /// <param name="connectionString"></param>
        public ProvinceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Hàm hỗ trợ mở kết nối đến SQL Server
        /// </summary>
        private IDbConnection OpenConnection()
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Lấy danh sách tất cả các Tỉnh/Thành
        /// </summary>
        public async Task<List<Province>> ListAsync()
        {
            List<Province> data = new List<Province>();

            using (var connection = OpenConnection())
            {
                var sql = "SELECT ProvinceName FROM Provinces ORDER BY ProvinceName";
                data = (await connection.QueryAsync<Province>(sql: sql, commandType: CommandType.Text)).ToList();
            }

            return data;
        }
    }
}