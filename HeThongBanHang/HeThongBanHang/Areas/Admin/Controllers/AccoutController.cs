using HeThongBanHang.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AccoutController : Controller
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        private readonly PasswordHasher<Employee> _passwordHasher;

        public AccoutController(QuanLiBanQuanAoContext dbContext)
        {
            _DbContext = dbContext;
            _passwordHasher = new PasswordHasher<Employee>();
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string FullName, string Username, string Password, string ConfirmPassword, string Role)
        {
            // so sánh password với confirmpassword
            if (Password != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu chưa trùng khớp";
                return RedirectToAction("Register");
            }
            // kiểm tra tài khoản trùng
            var exitingEmployee = _DbContext.Employees.FirstOrDefault(x => x.Username == Username);
            if (exitingEmployee != null)
            {
                TempData["ErrorMessage"] = "Tên tài khoản tồn tại";
                return RedirectToAction("Register");
            }

            var tempEmployee = new Employee { Username = Username };
            //mã hóa password
            var hashedPassword = _passwordHasher.HashPassword(tempEmployee, Password);

            var employee = new Employee
            {
                FullName = FullName,
                Username = Username,
                PasswordHash = hashedPassword,
                Role = Role,
                IsDeleted = false
            };
            _DbContext.Employees.Add(employee);
            _DbContext.SaveChanges();
            TempData["SuccessMessage"] = "Register successfully";

            return RedirectToAction("Login");
        }









        // Đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string Username, string Password)
        {
            var employee = _DbContext.Employees.FirstOrDefault(x => x.Username == Username );
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại";
                return RedirectToAction("Login");
            }
            if (employee.IsDeleted == true) // kiểm tra nhân viên nếu đã nghỉ thì không cho đăng nhập
            {
                TempData["ErrorMessage"] = "Tài khoản này đã bị vô hiệu hóa";
                return RedirectToAction("Login");
            }
            var result = _passwordHasher.VerifyHashedPassword(employee, employee.PasswordHash, Password);
            if (result == PasswordVerificationResult.Failed)
            {
                TempData["ErrorMessage"] = "Mật khẩu không đúng";
                return RedirectToAction("Login");
            }
            
            

            //lưu vào session
            HttpContext.Session.SetInt32("EmployeeId", employee.Id);
            HttpContext.Session.SetString("EmployeeName", employee.FullName);
            HttpContext.Session.SetString("Role", employee.Role);
            return RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
        }







        // Đăng xuất
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
