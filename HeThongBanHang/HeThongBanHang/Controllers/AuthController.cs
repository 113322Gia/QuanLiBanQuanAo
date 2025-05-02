using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using HeThongBanHang.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.Scripting;
using System.Net;

namespace HeThongBanHang.Controllers
{
    

    public class AuthController : Controller
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        private readonly PasswordHasher<User> _passwordHasher;
        public AuthController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
            _passwordHasher = new PasswordHasher<User>();
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register_Auth()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register_Auth(string Username, string Email, string Password, string ConfirmPassword)
        {
            if (Password != ConfirmPassword)
            {
                TempData["Error"] = "Mật khẩu và xác nhận mật khẩu không khớp.";
                return RedirectToAction("Register_Auth");
            }
            var existingUser = _DbContext.Users.FirstOrDefault(u => u.Username == Username);
            if (existingUser != null)
            {
                TempData["Error"] = "Tên đăng nhập hoặc Email đã tồn tại.";
                return RedirectToAction("Register_Auth");
            }
            var tempUser = new User { Username = Username };
            var hashedPassword = _passwordHasher.HashPassword(tempUser, Password);


            var user = new User
            {
                Username = Username,
                PasswordHash = hashedPassword,
                Role = "User",
                CustomerId = null,
            };
            _DbContext.Users.Add(user);
            _DbContext.SaveChanges();

            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login_Auth");
        }

        public IActionResult Login_Auth()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Login_Auth(string Username, string Password)
        {
            var user = _DbContext.Users.FirstOrDefault(u => u.Username == Username);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("Login_Auth");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, Password);
            if (result == PasswordVerificationResult.Failed)
            {
                TempData["Error"] = "Mật khẩu không đúng.";
                return RedirectToAction("Login_Auth");
            }

            // Lưu session (có thể thêm Role nếu cần)
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);

            return RedirectToAction("Index", "Home"); // Hoặc trang nào bạn muốn
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xoá toàn bộ session
            return RedirectToAction("Index", "Home");
        }

    }
}
