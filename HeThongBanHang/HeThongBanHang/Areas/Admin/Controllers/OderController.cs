using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public IActionResult Order_Index(DateTime? fromDate, DateTime? toDate)
        {
            // kiểm tra ngày
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                TempData["ErrorMessage"] = "Ngày bắt đầu không được lớn hơn ngày kết thúc.";
                return RedirectToAction("Order_Index");
            }

            var ordersQuery = _DbContext.Orders
                .Include(o => o.User)
                .Include(o => o.Employee)
                .Include(o => o.Payment)
                .AsQueryable();

            if (fromDate.HasValue && toDate.HasValue)
            {
                var startDate = fromDate.Value.Date;
                var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);
            }
            if (fromDate.HasValue && !toDate.HasValue)
            {
                var date = fromDate.Value.Date;
                var nextDate = date.AddDays(1);
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= date && o.CreatedAt < nextDate);
            }
            else if (!fromDate.HasValue && toDate.HasValue)
            {
                var date = toDate.Value.Date;
                var nextDate = date.AddDays(1);
                ordersQuery = ordersQuery.Where(o => o.CreatedAt >= date && o.CreatedAt < nextDate);
            }

            var orderList = ordersQuery
                .OrderByDescending(o => o.CreatedAt)
                .Take(50)  // Giới hạn kết quả
                .ToList();
            // kiểm tra nếu orderlist có 0 phần tử
            if(orderList.Count == 0) // count đại diện cho số nguyên
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
            if (order.Status == 2)
            {
                TempData["ErrorMessage"] = "Đơn hàng đã được hủy trước đó";
                return RedirectToAction("Order_Index");
            }
            else if (order.Status == 4)
            {
                order.Status = 3; // Đã xác nhận
                order.EmployeeId = employeeId.Value;
                TempData["SuccessMessage"] = "Xác nhận thành công";

                _DbContext.SaveChanges();
            }


            return RedirectToAction("Order_Index"); // quay lại danh sách đơn hàng
        }



     

    }
}
