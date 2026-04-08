using SV22T1020674.Models.Common;
using SV22T1020674.Models.Sales;

namespace SV22T1020674.DataLayers.Interfaces
{
    public interface IOrderRepository
    {
        Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input);
        Task<OrderViewInfo?> GetAsync(int orderID);
        Task<int> AddAsync(Order data);
        Task<bool> UpdateAsync(Order data);

        Task<bool> DeleteAsync(int orderID);

        Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID);
        Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID);
        Task<bool> AddDetailAsync(OrderDetail data);
        Task<bool> UpdateDetailAsync(OrderDetail data);
        Task<bool> DeleteDetailAsync(int orderID, int productID);

        Task<int> CreateOrderAsync(Order order);
        Task AddOrderDetailAsync(OrderDetail detail);

        // ===== CART =====
        Task<int> CreateOrderAsync(int customerID);
        Task<List<dynamic>> GetOrderHistoryAsync(int customerID);

        // ===== ADD (CHO CONTROLLER) =====
        Task<IEnumerable<dynamic>> GetOrdersByCustomerAsync(int customerID);
        Task<dynamic?> GetOrderByIdAsync(int orderID);
        Task<IEnumerable<dynamic>> GetOrderDetailsAsync(int orderID);
    }
}
