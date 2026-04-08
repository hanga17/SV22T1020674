using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.DataLayers.SQLServer;
using SV22T1020674.Models.Partner;

namespace SV22T1020674.BusinessLayers
{
    public static class CustomerDataService
    {
        private static ICustomerRepository customerDB;

        static CustomerDataService()
        {
            customerDB = new CustomerRepository(Configuration.ConnectionString);
        }

        public static Customer? GetCustomerByEmail(string email)
        {
            return customerDB.GetByEmail(email);
        }
    }
}