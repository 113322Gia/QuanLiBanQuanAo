using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang.Controllers
{
    public class ProDuctController : Controller
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        public ProDuctController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
