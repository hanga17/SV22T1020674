using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SV22T1020674.Admin.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id = 0)
        {
            return View();
        }

        public IActionResult Delete(int id = 0)
        {
            return View();
        }
    }
}