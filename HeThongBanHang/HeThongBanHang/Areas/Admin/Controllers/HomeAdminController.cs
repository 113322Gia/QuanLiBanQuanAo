using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Route("Admin")]
    //[Route("Admin/homeadmin")]

    public class HomeAdminController : BaseController
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        public HomeAdminController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            //truyền dữ liệu vào viewbag từ hàm CompareRevenueData
            var revenueData = CompareRevenueData();
            ViewBag.CurrentRevenue = revenueData.CurrentRevenue;
            ViewBag.PercentageChange = revenueData.PercentageChange;
            //truyền dữ liệu vào viewbag từ hàm CompareOrderCount
            var orderCountData = CompareOrderCount();
            ViewBag.CurrentWeekOrderCount = orderCountData.CurrentWeekOrderCount;
            ViewBag.PercentageChangeOrderCount = orderCountData.PercentageChangeOrderCount;

            return View(); 
            
        }
        [HttpGet]
        public IActionResult Revenue() //hàm thể hiện doanh thu trên biểu đồ
        {
            var year = DateTime.Now.Year;
            var revenueData = _DbContext.Orders
                .Where(o => o.Status == 3 && o.CreatedAt.HasValue && o.CreatedAt.Value.Year == year)
                .GroupBy(o => o.CreatedAt!.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(o => o.TotalPrice ?? 0)
                })
                .OrderBy(x => x.Month)
                .ToList();

            var labels = revenueData.Select(x => $"Tháng {x.Month}").ToArray();
            var values = revenueData.Select(x => x.Total).ToArray();

            return Json(new { labels = labels, values = values });
        }


        // hàm so sánh doanh thu và tính phân trăm
        private (decimal CurrentRevenue, decimal PercentageChange) CompareRevenueData()
        {
            var now = DateTime.Now;

            var currentWeekStart = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);// Lấy ngày đầu tuần hiện tại (thứ Hai)

            var currentWeekEnd = currentWeekStart.AddDays(6);// Lấy ngày cuối tuần hiện tại (Chủ Nhật)

            var previousWeekStart = currentWeekStart.AddDays(-7); // Lấy ngày đầu tuần trước
            
            var previousWeekEnd = previousWeekStart.AddDays(6);// Lấy ngày cuối tuần trước

            // Tính tổng doanh thu tuần hiện tại với điều kiện hóa đơn có status =3
            var currentWeekRevenue = _DbContext.Orders
                .Where(o => o.Status == 3 && o.CreatedAt.HasValue &&
                            o.CreatedAt.Value >= currentWeekStart &&
                            o.CreatedAt.Value <= currentWeekEnd)
                .Sum(o => o.TotalPrice ?? 0);

            // Tính tổng doanh thu tuần trước
            var previousWeekRevenue = _DbContext.Orders
                .Where(o => o.Status == 3 && o.CreatedAt.HasValue &&
                            o.CreatedAt.Value >= previousWeekStart &&
                            o.CreatedAt.Value <= previousWeekEnd)
                .Sum(o => o.TotalPrice ?? 0);

            // Tính phần trăm tăng trưởng hoặc giảm sút
            decimal percentageChange = 0;
            if (previousWeekRevenue != 0)
            {
                percentageChange = ((currentWeekRevenue - previousWeekRevenue) / previousWeekRevenue) * 100;
            }

            return (currentWeekRevenue, percentageChange);
        }

        [HttpGet]
        public IActionResult GetTotalOrders() // hàm tính tổng hóa đơn
        {
            var year = DateTime.Now.Year;
            var totalOrdersData = _DbContext.Orders
                .Where(o => o.Status == 3 && o.CreatedAt.HasValue && o.CreatedAt.Value.Year == year)
                .GroupBy(o => o.CreatedAt!.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            var labels = totalOrdersData.Select(x => $"Tháng {x.Month}").ToArray();
            var values = totalOrdersData.Select(x => x.Count).ToArray();

            return Json(new { labels = labels, values = values });
        }


        private (int CurrentWeekOrderCount, decimal PercentageChangeOrderCount) CompareOrderCount()
        {
            var now = DateTime.Now;

            // Lấy ngày đầu tuần hiện tại (thứ Hai) và Lấy ngày cuối tuần hiện tại (Chủ Nhật)
            var currentWeekStart = now.AddDays(-(int)now.DayOfWeek + (int)DayOfWeek.Monday);
            
            var currentWeekEnd = currentWeekStart.AddDays(6);

            // Lấy ngày đầu tuần và cuối tuần của tuần trước 
            var previousWeekStart = currentWeekStart.AddDays(-7);
           
            var previousWeekEnd = previousWeekStart.AddDays(6);

            // Tính số lượng đơn tuần hiện tại
            var currentWeekOrderCount = _DbContext.Orders
                .Where(o => o.Status == 3 && o.CreatedAt.HasValue &&
                            o.CreatedAt.Value >= currentWeekStart &&
                            o.CreatedAt.Value <= currentWeekEnd)
                .Count();

            // Tính số lượng đơn tuần trước
            var previousWeekOrderCount = _DbContext.Orders
                .Where(o => o.Status == 3 && o.CreatedAt.HasValue &&
                            o.CreatedAt.Value >= previousWeekStart &&
                            o.CreatedAt.Value <= previousWeekEnd)
                .Count();

            // Tính phần trăm tăng trưởng hoặc giảm sút số lượng đơn hàng
            decimal percentageChangeOrderCount = 0;
            if (previousWeekOrderCount != 0)
            {
                percentageChangeOrderCount = ((decimal)(currentWeekOrderCount - previousWeekOrderCount) / previousWeekOrderCount) * 100;
            }

            return (currentWeekOrderCount, percentageChangeOrderCount);
        }
    }
}
