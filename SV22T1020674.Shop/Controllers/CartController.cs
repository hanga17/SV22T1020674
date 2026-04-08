using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SV22T1020674.BusinessLayers;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Catalog;
using SV22T1020674.Models;

using SV22T1020674.Models.Sales;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020674.Shop.Controllers
{
    public class CartController : Controller
    {
        private readonly IOrderRepository _orderRepo;

        public CartController(IOrderRepository orderRepo)
        {
            _orderRepo = orderRepo;
        }

        private async Task<List<Product>> GetData()
        {
            return await CatalogDataService.GetAllProductsAsync();
        }

        private List<CartItem> GetCart()
        {
            var session = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(session))
                return new List<CartItem>();

            return JsonConvert.DeserializeObject<List<CartItem>>(session);
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("CustomerID") == null)
                return RedirectToAction("Login", "Account");

            return View(GetCart());
        }

        public async Task<IActionResult> Add(int id)
        {
            if (HttpContext.Session.GetInt32("CustomerID") == null)
                return RedirectToAction("Login", "Account");

            var cart = GetCart();
            var data = await GetData();

            var product = data.FirstOrDefault(x => x.ProductID == id);
            if (product == null)
                return RedirectToAction("Index", "Product");

            var item = cart.FirstOrDefault(x => x.ProductID == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Quantity = 1
                });
            }
            else item.Quantity++;

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Increase(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);
            if (item != null) item.Quantity++;

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Decrease(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductID == id);

            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                    cart.Remove(item);
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(x => x.ProductID == id);

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var cart = GetCart();
            if (cart.Count == 0)
                return RedirectToAction("Index");

            int orderId = await _orderRepo.CreateOrderAsync(customerId.Value);

            foreach (var item in cart)
            {
                await _orderRepo.AddDetailAsync(new SV22T1020674.Models.Sales.OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.Price
                });

            }

            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index", "Order");
        }

        public async Task<IActionResult> History()
        {
            var customerID = HttpContext.Session.GetInt32("CustomerID");
            if (customerID == null)
                return RedirectToAction("Login", "Account");

            var data = await _orderRepo.GetOrderHistoryAsync(customerID.Value);
            return View(data);
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");
            return RedirectToAction("Index");
        }
    }
}
