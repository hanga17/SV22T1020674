using Microsoft.AspNetCore.Mvc;
using SV22T1020674.BusinessLayers;
using SV22T1020674.Models.Catalog;
using SV22T1020674.DataLayers.Interfaces;

namespace SV22T1020674.Shop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index(string searchValue = "", int page = 1, string priceRange = "")
        {
            int pageSize = 21;

            decimal minPrice = 0;
            decimal maxPrice = 0;

            // Tách khoảng giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                var parts = priceRange.Split('-');

                if (parts.Length == 2)
                {
                    decimal.TryParse(parts[0], out minPrice);
                    decimal.TryParse(parts[1], out maxPrice);
                }
            }

            var input = new ProductSearchInput()
            {
                Page = page,
                PageSize = pageSize,
                SearchValue = searchValue ?? "",
                MinPrice = minPrice,
                MaxPrice = maxPrice
            };

            var result = await _productRepository.ListAsync(input);

            ViewBag.Page = page;
            ViewBag.PriceRange = priceRange;

            return View(result.DataItems);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (HttpContext.Session.GetInt32("CustomerID") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _productRepository.GetSellingProductAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
