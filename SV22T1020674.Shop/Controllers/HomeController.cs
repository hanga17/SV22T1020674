using Microsoft.AspNetCore.Mvc;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Catalog;
using SV22T1020674.Models.Common;

namespace SV22T1020674.Shop.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;

        public HomeController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index(string searchValue = "")
        {
            var input = new ProductSearchInput()
            {
                Page = 1,
                PageSize = 20,
                SearchValue = searchValue,
                CategoryID = 0,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };

            var result = await _productRepository.ListAsync(input);

            return View(result.DataItems);
        }
    }
}
