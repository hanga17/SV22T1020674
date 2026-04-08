using Microsoft.AspNetCore.Mvc;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Partner;
using SV22T1020674.BusinessLayers;
using System.Threading.Tasks;

namespace SV22T1020674.Shop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICustomerRepository _customerRepository;

        public AccountController(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        // ===== LOGIN =====
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            string hashedPassword = CryptHelper.HashMD5(password);
            var customer = _customerRepository.GetByEmail(email);

            if (customer == null || customer.Password != hashedPassword)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            HttpContext.Session.SetString("User", customer.CustomerName);
            HttpContext.Session.SetInt32("CustomerID", customer.CustomerID);
            return RedirectToAction("Index", "Home");
        }

        // ===== LOGOUT =====
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "Đã đăng xuất thành công";
            return RedirectToAction("Index", "Home");
        }

        // ===== REGISTER =====
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string fullName, string address, string phone, string email, string password)
        {
            if (!await _customerRepository.ValidateEmailAsync(email))
            {
                ViewBag.Error = "Email đã tồn tại";
                return View();
            }

            var customer = new Customer
            {
                CustomerName = fullName,
                ContactName = fullName,
                Address = address,
                Phone = phone,
                Email = email,
                Password = CryptHelper.HashMD5(password),
                IsLocked = false
            };

            await _customerRepository.AddAsync(customer);
            TempData["Message"] = "Đăng ký thành công";
            return RedirectToAction("Login");
        }

        // ===== PROFILE =====
        public async Task<IActionResult> Profile()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null) return RedirectToAction("Login");

            var customer = await _customerRepository.GetAsync(customerId.Value);
            return View(customer);
        }

        // ===== CHANGE PASSWORD =====
        public IActionResult ChangePassword() => View();

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null) return RedirectToAction("Login");

            var customer = await _customerRepository.GetAsync(customerId.Value);
            string oldHash = CryptHelper.HashMD5(oldPassword);
            if (customer == null || customer.Password != oldHash)
            {
                TempData["Error"] = "Mật khẩu cũ không đúng";
                return RedirectToAction("ChangePassword");
            }

            customer.Password = CryptHelper.HashMD5(newPassword);
            await _customerRepository.UpdateAsync(customer);

            TempData["Success"] = "Đổi mật khẩu thành công";
            return RedirectToAction("Profile");
        }

        // ===== EDIT PROFILE =====
        public async Task<IActionResult> EditProfile()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerID");
            if (customerId == null) return RedirectToAction("Login");

            var customer = await _customerRepository.GetAsync(customerId.Value);
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(Customer model)
        {
            var customer = await _customerRepository.GetAsync(model.CustomerID);
            if (customer != null)
            {
                customer.CustomerName = model.CustomerName;
                customer.Address = model.Address;
                customer.Phone = model.Phone;
                await _customerRepository.UpdateAsync(customer);
            }

            TempData["Success"] = "Cập nhật hồ sơ thành công";
            return RedirectToAction("Profile");
        }
    }
}
