using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.DataLayers.SQLServer;
using SV22T1020674.Models.Partner;
using SV22T1020674.Models.Common;

namespace SV22T1020674.BusinessLayers
{
    public static class CommonDataService
    {
        private static readonly IGenericRepository<Shipper> shipperDB;

        static CommonDataService()
        {
            shipperDB = new ShipperRepository(Configuration.ConnectionString);
        }

        public static List<Shipper> ListOfShippers()
        {
            var input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 0, // lấy tất cả
                SearchValue = ""
            };

            var result = shipperDB.ListAsync(input).Result;
            return result.DataItems;
        }
    }
}
