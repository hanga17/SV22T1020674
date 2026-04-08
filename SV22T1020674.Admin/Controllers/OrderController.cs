using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SV22T1020674.BusinessLayers;
using SV22T1020674.Models.Catalog;
using SV22T1020674.Models.Partner;
using SV22T1020674.Models.Sales;
using SV22T1020674.Session;


namespace SV22T1020674.Admin.Controllers
{
    
    [Authorize]
    public class OrderController : Controller
    {
        

        public IActionResult Index()
        {
            ViewBag.Title = "Quản lý đơn hàng";
            return View();
        }

        

        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            var data = await SalesDataService.ListOrdersAsync(input);
            return View(data);
        }

        

        public async Task<IActionResult> Details(int id)
        {
            var order = await SalesDataService.GetOrderAsync(id);
            if (order == null)
                return RedirectToAction("Index");

            var details = await SalesDataService.ListDetailsAsync(id);
            

            order.Details = details; 

            return View(order);
        }

       
        public async Task<IActionResult> Accept(int id = 0)
        {
            await SalesDataService.AcceptOrderAsync(id, 1);
            return RedirectToAction("Index");
        }

        
        public async Task<IActionResult> Reject(int id = 0)
        {
            await SalesDataService.RejectOrderAsync(id, 1);
            return RedirectToAction("Index");
        }

        
        public async Task<IActionResult> Cancel(int id = 0)
        {
            await SalesDataService.CancelOrderAsync(id);
            return RedirectToAction("Index");
        }

        

        [HttpGet]
        public IActionResult Shipping(int id = 0)
        {
            ViewBag.Title = "Chọn shipper";
            ViewBag.OrderID = id;

           
            ViewBag.Shippers = new List<Shipper>()
    {
        new Shipper() { ShipperID = 1, ShipperName = "Shipper A" },
        new Shipper() { ShipperID = 2, ShipperName = "Shipper B" }
    };

            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            
            if (shipperID == 0)
            {
                TempData["Error"] = "Vui lòng chọn shipper";
                return RedirectToAction("Shipping", new { id = id });
            }

            
            await SalesDataService.ShipOrderAsync(id, shipperID);

            TempData["Success"] = "Đã chuyển đơn cho shipper";

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Finish(int id = 0)
        {
            await SalesDataService.CompleteOrderAsync(id);
            return RedirectToAction("Index");
        }

        

        [HttpGet]
        public async Task<IActionResult> Delete(int id = 0)
        {
            ViewBag.Title = "Xóa đơn hàng";
            var order = await SalesDataService.GetOrderAsync(id);
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id, string dummy)
        {
            await SalesDataService.DeleteOrderAsync(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Create(string searchValue = "")
        {
            var model = new CreateOrderViewModel()
            {
                SearchValue = searchValue
            };

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 10,
                    SearchValue = searchValue
                };

                var result = await CatalogDataService.ListProductsAsync(input);
                model.Products = result.DataItems;
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productID, int quantity, decimal salePrice)
        {
            var product = await CatalogDataService.GetProductAsync(productID);
            if (product == null)
                return RedirectToAction("Create");

            var item = new CartItem()
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                Unit = product.Unit,
                Quantity = quantity,
                SalePrice = salePrice
            };

            CartSession.AddItem(HttpContext.Session, item);

            return RedirectToAction("Create");
        }
        [HttpGet]
        public IActionResult DeleteCartItem(int productID)
        {
            return View(productID);
        }
        [HttpPost]
        public IActionResult DeleteCartItem(int productID, string dummy)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart");

            if (cart != null)
            {
                var item = cart.FirstOrDefault(x => x.ProductID == productID);
                if (item != null)
                    cart.Remove(item);

                HttpContext.Session.SetObject("cart", cart);
            }

            return RedirectToAction("Create");
        }
        [HttpGet]
        public IActionResult ClearCart()
        {
            return View();
        }

        [HttpPost]
        [ActionName("ClearCart")]   
        public IActionResult ClearCartPost()
        {
            HttpContext.Session.Remove("cart");
            return RedirectToAction("Create");
        }
        [HttpPost]
        public async Task<IActionResult> SaveOrder(int CustomerID, string DeliveryProvince, string DeliveryAddress)
        {
            var cart = CartSession.GetCart(HttpContext.Session);

            
            if (cart == null || cart.Count == 0)
            {
                ModelState.AddModelError("", "Giỏ hàng đang trống");
                return RedirectToAction("Create");
            }

            if (CustomerID == 0)
            {
                ModelState.AddModelError("", "Chưa chọn khách hàng");
                return RedirectToAction("Create");
            }

            var order = new Order()
            {
                CustomerID = CustomerID,
                OrderTime = DateTime.Now,
                EmployeeID = 1, // tạm hardcode
                Status = OrderStatusEnum.New,
                DeliveryProvince = DeliveryProvince,
                DeliveryAddress = DeliveryAddress
            };

            int orderID = await SalesDataService.CreateOrderAsync(order);

            
            foreach (var item in cart)
            {
                var detail = new OrderDetail()
                {
                    OrderID = orderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                };

                await SalesDataService.AddOrderDetailAsync(detail);
            }

            
            CartSession.ClearCart(HttpContext.Session);

            
            return RedirectToAction("Index");
        }
    }
}
