using SV22T1020674.Models.Partner;

namespace SV22T1020674.DataLayers.Interfaces
{
    public interface IUserAccountRepository
    {
        Task<Customer?> LoginAsync(string email, string password);
        Task<int> CheckEmailExistsAsync(string email);
        Task<bool> RegisterAsync(string fullName, string address, string phone, string email, string password);
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer?> CheckOldPasswordAsync(int customerId, string password);
        Task<bool> UpdatePasswordAsync(int customerId, string newPassword);
        Task<Customer?> GetProfileAsync(int customerId);
        Task<bool> UpdateProfileAsync(Customer model);
    }
}
