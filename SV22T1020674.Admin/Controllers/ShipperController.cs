using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020674.BusinessLayers;
using SV22T1020674.Models.Common;
using SV22T1020674.Models.Partner;

namespace SV22T1020674.Admin.Controllers
{
    [Authorize]
    public class ShipperController : Controller
    {
        private const string SHIPPER_SEARCH = "ShipperSearchInput";

        // ====================== INDEX ======================
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = 20,
                SearchValue = searchValue ?? ""
            };

            var result = await PartnerDataService.ListShippersAsync(input);

            ViewBag.SearchValue = searchValue;
            return View(result);
        }

        // ====================== CREATE ======================
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung shipper";
            return View(new Shipper());
        }

        [HttpPost]
        public async Task<IActionResult> Create(Shipper model)
        {
            if (string.IsNullOrWhiteSpace(model.ShipperName))
                ModelState.AddModelError(nameof(model.ShipperName), "Tên shipper không được để trống");

            if (!ModelState.IsValid)
                return View(model);

            await PartnerDataService.AddShipperAsync(model);
            return RedirectToAction("Index");
        }

        // ====================== EDIT ======================
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật shipper";

            var data = await PartnerDataService.GetShipperAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Shipper model)
        {
            if (string.IsNullOrWhiteSpace(model.ShipperName))
                ModelState.AddModelError(nameof(model.ShipperName), "Tên shipper không được để trống");

            if (!ModelState.IsValid)
                return View(model);

            await PartnerDataService.UpdateShipperAsync(model);
            return RedirectToAction("Index");
        }

        // ====================== DELETE ======================
        public async Task<IActionResult> Delete(int id = 0)
        {
            var data = await PartnerDataService.GetShipperAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await PartnerDataService.IsUsedShipperAsync(id);

            return View(data);
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await PartnerDataService.DeleteShipperAsync(id);
            return RedirectToAction("Index");
        }
    }
}
