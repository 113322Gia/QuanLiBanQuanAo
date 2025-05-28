using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]

    public class OderController : BaseController
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        public OderController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Order_Index(DateTime? fromDate, DateTime? toDate, string filter)
        {
            // Kiểm tra điều kiện ngày
            if (fromDate > toDate)
            {
                TempData["ErrorMessage"] = "Ngày bắt đầu không được lớn hơn ngày kết thúc.";
                return RedirectToAction("Order_Index");
            }

            var ordersQuery = _DbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Employee)
                .Include(o => o.Payment)
                .AsQueryable();

            // Lọc theo filter nhanh
            switch (filter)
            {
                case "today":
                    var today = DateTime.Today;
                    ordersQuery = ordersQuery.Where(x => x.CreatedAt.HasValue && x.CreatedAt.Value.Date == today);
                    break;

                case "last7days":
                    var last7Days = DateTime.Today.AddDays(-7);
                    ordersQuery = ordersQuery.Where(x => x.CreatedAt >= last7Days);
                    break;

                case "thismonth":
                    var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    ordersQuery = ordersQuery.Where(x => x.CreatedAt >= startOfMonth);
                    break;
            }


            // Lọc theo ngày cụ thể
            if (fromDate.HasValue || toDate.HasValue)
            {
                var startDate = fromDate?.Date ?? DateTime.MinValue;
                var endDate = (toDate?.Date.AddDays(1).AddTicks(-1)) ?? DateTime.MaxValue;
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
            }

            var orderList = ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .Take(50)
                .ToList();

            if (!orderList.Any())
            {
                TempData["ErrorMessage"] = "Không có đơn hàng nào trong khoảng thời gian này.";
                return RedirectToAction("Order_Index");
            }

            return View(orderList);
        }


        public IActionResult Order_Detail(int id)
        {
            // Lấy đơn hàng từ cơ sở dữ liệu
            var order = _DbContext.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            // Lấy thông tin khách hàng từ cơ sở dữ liệu
            var customer = _DbContext.Customers
                .FirstOrDefault(c => c.Id == order.CustomerId); // Lấy thông tin khách hàng từ bảng Customers

            if (customer != null)
            {
                ViewBag.FullName = customer.FullName;
                ViewBag.Email = customer.Email;
                ViewBag.Address = customer.Address;
                ViewBag.City = customer.City;
                ViewBag.Tel = customer.Tel;
            }

            return View(order);
        }

        [HttpGet]
        public IActionResult Confirm(int id)
        {
            var order = _DbContext.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            // Lấy EmployeeId từ session
            var employeeId = HttpContext.Session.GetInt32("EmployeeId");

            if (employeeId == null)
            {
                return RedirectToAction("Login", "Accout"); // hoặc chuyển về trang đăng nhập
            }
            if (order.Status == 1)
            {
                TempData["ErrorMessage"] = "Đơn hàng chưa được khách hàng xác nhận";
                return RedirectToAction("Order_Index");
            }
            if (order.Status == 2)
            {
                TempData["ErrorMessage"] = "Đơn hàng đã được hủy trước đó";
                return RedirectToAction("Order_Index");
            }
            else if (order.Status == 4)
            {
                order.Status = 3; // Đã xác nhận
                order.EmployeeId = employeeId.Value;
                TempData["SuccessMessage"] = $"Xác nhận thành công #{order.Id}";

                _DbContext.SaveChanges();
            }


            return RedirectToAction("Order_Index"); // quay lại danh sách đơn hàng
        }





    }
}
