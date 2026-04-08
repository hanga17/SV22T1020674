using Microsoft.AspNetCore.Mvc;
using SV22T1020674.DataLayers.Interfaces;

namespace SV22T1020674.Shop.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // ===== DANH SÁCH ĐƠN =====
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetInt32("CustomerID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customerId = HttpContext.Session.GetInt32("CustomerID");

            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var orders = await _orderRepository.GetOrdersByCustomerAsync(customerId.Value);

            return View(orders);
        }

        // ===== CHI TIẾT ĐƠN =====
        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetInt32("CustomerID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderRepository.GetOrderByIdAsync(id);
            var details = await _orderRepository.GetOrderDetailsAsync(id);

            ViewBag.Details = details;

            decimal total = 0;
            foreach (var item in details)
            {
                total += item.Quantity * item.SalePrice;
            }

            ViewBag.Total = total;

            return View(order);
        }
    }
}
