using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : BaseController
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
        public IActionResult Customer_Index()
        {
            var lst = _DbContext.Users
                .Include(x => x.Customers)
                .ToList();
            return View(lst);
        }
    }
}
