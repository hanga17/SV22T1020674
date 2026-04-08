using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.DataDictionary; // ✅ dùng Province đúng
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020674.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp lấy các dữ liệu từ điển, danh mục tĩnh (như Tỉnh thành)
    /// </summary>
    public class DataDictionaryRepository : IDataDictionaryRepository<Province>
    {
        private readonly string _connectionString;

        public DataDictionaryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Lấy danh sách toàn bộ Tỉnh/Thành phố
        /// </summary>
        public async Task<List<Province>> ListAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"SELECT ProvinceName FROM Provinces ORDER BY ProvinceName";

            var data = await connection.QueryAsync<Province>(sql);
            return data.ToList();
        }
    }
}
