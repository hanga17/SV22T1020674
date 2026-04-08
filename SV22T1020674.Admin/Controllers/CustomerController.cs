using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020674.Models.Common;
using SV22T1020674.Models.Partner;
using SV22T1020674.DataLayers.Interfaces;
using System.Text.Json;

namespace SV22T1020674.Admin.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        private const string CUSTOMER_SEARCH = "CustomerSearchInput";

        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            PaginationSearchInput? input = null;
            var sessionString = HttpContext.Session.GetString(CUSTOMER_SEARCH);

            if (!string.IsNullOrEmpty(sessionString))
            {
                input = JsonSerializer.Deserialize<PaginationSearchInput>(sessionString);
            }

            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            }

            if (HttpContext.Request.Query.ContainsKey("searchValue") || HttpContext.Request.Query.ContainsKey("page"))
            {
                input.Page = HttpContext.Request.Query.ContainsKey("page") ? page : 1;
                input.SearchValue = searchValue ?? "";
            }

            HttpContext.Session.SetString(CUSTOMER_SEARCH, JsonSerializer.Serialize(input));

            // 👉 GỌI REPOSITORY
            var result = await _customerRepository.ListAsync(input);

            ViewBag.SearchValue = input.SearchValue;

            return View(result);
        }

        public IActionResult Create()
        {
            var model = new Customer()
            {
                CustomerID = 0,
                IsLocked = false
            };
            ViewBag.Title = "Bổ sung khách hàng";
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";

            var model = await _customerRepository.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Customer data)
        {
            try
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật thông tin khách hàng";

                if (string.IsNullOrWhiteSpace(data.CustomerName))
                    ModelState.AddModelError(nameof(data.CustomerName), "Vui lòng nhập tên khách hàng");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
                else if (!await _customerRepository.ValidateEmailAsync(data.Email, data.CustomerID))
                    ModelState.AddModelError(nameof(data.Email), "Email này đã được sử dụng");

                if (string.IsNullOrEmpty(data.Province))
                    ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn Tỉnh/Thành");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                if (string.IsNullOrWhiteSpace(data.ContactName)) data.ContactName = data.CustomerName;
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";

                if (data.CustomerID == 0)
                {
                    await _customerRepository.AddAsync(data);
                }
                else
                {
                    await _customerRepository.UpdateAsync(data);
                }

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("Error", "Hệ thống tạm thời đang bận");
                return View("Edit", data);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await _customerRepository.DeleteAsync(id);
                return RedirectToAction("Index");
            }

            var model = await _customerRepository.GetAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await _customerRepository.IsUsedAsync(id);
            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword(int id)
        {
            ViewBag.Title = "Đặt lại mật khẩu khách hàng";
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(int id, string newPassword, string confirmPassword)
        {
            return RedirectToAction("Index");
        }
    }
}
