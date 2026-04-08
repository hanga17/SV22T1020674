using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020674.Models.Security;
using SV22T1020674.DataLayers.Interfaces;

namespace SV22T1020674.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public AccountController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // ================= LOGIN =================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            User.GetUserData();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.Username = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập tên và mật khẩu");
                return View();
            }

            string hashedPassword = CryptHelper.HashMD5(password);

            // 👉 GỌI REPOSITORY (KHÔNG DÙNG HRDataService)
            var employee = await _employeeRepository.AuthorizeAsync(username, hashedPassword);

            if (employee == null)
            {
                ModelState.AddModelError("Error", "Sai tài khoản hoặc mật khẩu");
                return View();
            }

            var userData = new WebUserData()
            {
                UserId = employee.EmployeeID.ToString(),
                UserName = employee.Email,
                DisplayName = employee.FullName,
                Email = employee.Email,
                Photo = employee.Photo ?? "nophoto.png",
                Roles = new List<string> { "Administrator" }
            };

            var principal = userData.CreatePrincipal();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return RedirectToAction("Index", "Home");
        }

        // ================= LOGOUT =================
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        // ================= CHANGE PASSWORD =================
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ViewBag.Message = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var user = User.GetUserData();

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            string oldHash = CryptHelper.HashMD5(oldPassword);

            var employee = await _employeeRepository.AuthorizeAsync(user.UserName, oldHash);

            if (employee == null)
            {
                ViewBag.Message = "Mật khẩu cũ không đúng";
                return View();
            }

            string newHash = CryptHelper.HashMD5(newPassword);

            await _employeeRepository.UpdatePasswordAsync(employee.EmployeeID, newHash);

            ViewBag.Message = "Đổi mật khẩu thành công";
            return View();
        }
    }
}
