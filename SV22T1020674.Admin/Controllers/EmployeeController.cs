using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Common;
using SV22T1020674.Models.HR;

namespace SV22T1020674.Admin.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(int page = 1, string searchValue = "")
        {
            var input = new PaginationSearchInput()
            {
                Page = page,
                PageSize = ApplicationContext.PageSize,
                SearchValue = searchValue ?? ""
            };

            var result = await _employeeRepository.ListAsync(input);

            ViewBag.SearchValue = searchValue;

            return View(result);
        }

        // =========================
        // CREATE - GET
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Employee());
        }

        // =========================
        // CREATE - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee model)
        {
            if (string.IsNullOrWhiteSpace(model.FullName))
                ModelState.AddModelError(nameof(model.FullName), "Vui lòng nhập tên");

            if (string.IsNullOrWhiteSpace(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Email không được để trống");
            else if (!await _employeeRepository.ValidateEmailAsync(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Email đã tồn tại");

            if (!ModelState.IsValid)
                return View(model);

            model.Password = "123456"; // mặc định
            model.IsWorking = true;

            await _employeeRepository.AddAsync(model);

            return RedirectToAction("Index");
        }

        // =========================
        // EDIT - GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeRepository.GetAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            return View(employee);
        }

        // =========================
        // EDIT - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Employee model)
        {
            var employee = await _employeeRepository.GetAsync(model.EmployeeID);
            if (employee == null)
                return RedirectToAction("Index");

            employee.FullName = model.FullName;
            employee.BirthDate = model.BirthDate;
            employee.Phone = model.Phone;
            employee.Email = model.Email;
            employee.Address = model.Address;

            await _employeeRepository.UpdateAsync(employee);

            return RedirectToAction("Index");
        }

        // =========================
        // DELETE - GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _employeeRepository.GetAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await _employeeRepository.IsUsedAsync(id);

            return View(employee);
        }

        // =========================
        // DELETE - POST
        // =========================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int employeeID)
        {
            await _employeeRepository.DeleteAsync(employeeID);
            return RedirectToAction("Index");
        }

        // =========================
        // CHANGE PASSWORD - GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var employee = await _employeeRepository.GetAsync(id);
            if (employee == null)
                return RedirectToAction("Index");

            return View(employee);
        }

        // =========================
        // CHANGE PASSWORD - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(int employeeID, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError("Error", "Mật khẩu không được rỗng");
                return View();
            }

            string hash = CryptHelper.HashMD5(newPassword);

            await _employeeRepository.UpdatePasswordAsync(employeeID, hash);

            return RedirectToAction("Index");
        }
    }
}
