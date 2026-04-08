using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.DataLayers.SQLServer;
using SV22T1020674.BusinessLayers;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.DataLayers.SQLServer;
using System.Collections.Generic;
using System.Threading.Tasks;
// FIX: alias Province
using ProvinceModel = SV22T1020674.Models.DataDictionary.Province;

namespace SV22T1020674.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến từ điển dữ liệu
    /// </summary>
    public static class DictionaryDataService
    {
        private static readonly IDataDictionaryRepository<ProvinceModel> provinceDB;

        /// <summary>
        /// Ctor
        /// </summary>
        static DictionaryDataService()
        {
            provinceDB = new ProvinceRepository(Configuration.ConnectionString);
        }

        /// <summary>
        /// Lấy danh sách tỉnh thành
        /// </summary>
        public static async Task<List<ProvinceModel>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }
    }
}