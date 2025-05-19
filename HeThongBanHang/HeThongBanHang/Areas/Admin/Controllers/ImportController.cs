using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ImportController : BaseController
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        public ImportController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Import_Index(DateTime? fromDate, DateTime? toDate, string filter)
        {
            if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
            {
                TempData["ErrorMessage"] = "Ngày bắt đầu không được lớn hơn ngày kết thúc.";
                return RedirectToAction("Import_Index");
            }

            var query = _DbContext.InventoryImports
                .Include(x => x.ProductVariant).ThenInclude(x => x.Product)
                .Include(x => x.Employee)
                .AsQueryable();

            DateTime startDate, endDate;

            switch (filter)
            {
                case "today":
                    startDate = DateTime.Today;
                    endDate = startDate.AddDays(1);
                    query = query.Where(x => x.ChangeDate >= startDate && x.ChangeDate < endDate);
                    break;

                case "last7days":
                    startDate = DateTime.Today.AddDays(-7);
                    query = query.Where(x => x.ChangeDate >= startDate);
                    break;

                case "thismonth":
                    startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    query = query.Where(x => x.ChangeDate >= startDate);
                    break;
            }

            if (fromDate.HasValue && toDate.HasValue)
            {
                startDate = fromDate.Value.Date;
                endDate = toDate.Value.Date.AddDays(1); // include full day
                query = query.Where(x => x.ChangeDate >= startDate && x.ChangeDate < endDate);
            }

            var result = query.OrderByDescending(x => x.ChangeDate).ToList();
            return View(result);
        }



    }
}
