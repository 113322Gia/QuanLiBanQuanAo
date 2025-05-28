using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.Controllers
{
    public class UserController : Controller
    {
        private readonly QuanLiBanQuanAoContext _DbContext;

        public UserController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UpdateInfo()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login_Auth", "Auth");
            }

            var user = _DbContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            Customer? customer;     // dấu ? có 2 tác dụng (1: khai báo biến nullable nghĩa là có thể nhận giá trị null, 2: dùng để kiểm tra trong toán tử để tránh lỗi bị null)

            if (user.CustomerId != null)
            {
                // Đã có CustomerId → lấy dữ liệu Customer
                customer = _DbContext.Customers.FirstOrDefault(x => x.Id == user.CustomerId);

                if (customer == null)
                {
                    return NotFound();
                }
            }
            else
            {
                // Chưa có CustomerId → tạo customer trống
                customer = new Customer();
            }

            return View(customer); // gửi model xuống view
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateInfo(Customer model)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login_Auth", "Auth");
            }

            var user = _DbContext.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (user.CustomerId == null)
            {
                // Người dùng chưa có Customer → tạo mới
                _DbContext.Customers.Add(model);
                _DbContext.SaveChanges();

                // Gán CustomerId mới cho user
                user.CustomerId = model.Id;
                TempData["customerInfo"] = user.Username;
                _DbContext.SaveChanges();
            }
            else
            {
                // Người dùng đã có Customer → update
                var customer = _DbContext.Customers.FirstOrDefault(c => c.Id == user.CustomerId);
                if (customer == null)
                {
                    return NotFound();
                }

                customer.FullName = model.FullName;
                customer.Email = model.Email;
                customer.Address = model.Address;
                customer.City = model.City;
                customer.Tel = model.Tel;
                
                _DbContext.SaveChanges();
            }

            TempData["Success"] = "Cập nhật thành công!";
            
            return RedirectToAction("UpdateInfo");
        }

        // hàm xem lịch sử của khách hàng 
        public IActionResult HistoryOrder()
        {
            //Lấy id của khách hàng
            var userId = HttpContext.Session.GetInt32("UserId");
            // kiểm tra có userId không
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập";
                return RedirectToAction("Login_Auth", "Auth");
            }
            var user = _DbContext.Users
                .Include(x => x.Customers)
                .Include(x => x.Orders)
                .ThenInclude(x => x.OrderItems)
                .FirstOrDefault(x => x.Id == userId);

            return View(user);
        }

        [HttpPost]
        public IActionResult Toggle([FromBody] int productId) // hàm yêu thích sản phẩm
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Bạn cần đăng nhập để thực hiện hành động này." });
            }

            var productExists = _DbContext.Products.Any(p => p.Id == productId);
            if (!productExists)
            {
                return BadRequest(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            var favorite = _DbContext.Favorite.FirstOrDefault(f => f.UserId == userId && f.ProductId == productId);

            bool isFavoritedNow = false;

            try
            {
                if (favorite == null)
                {
                    // Nếu chưa có, tạo mới bản ghi Favorite
                    var newFavorite = new Favorite
                    {
                        UserId = userId.Value,
                        ProductId = productId
                    };
                    _DbContext.Favorite.Add(newFavorite);
                    isFavoritedNow = true;
                }
                else
                {
                    // Nếu đã có, xóa đi (bỏ yêu thích)
                    _DbContext.Favorite.Remove(favorite);
                    isFavoritedNow = false;
                }

                _DbContext.SaveChanges();

                return Ok(new { success = true, isFavorited = isFavoritedNow });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        } 









    }
}
