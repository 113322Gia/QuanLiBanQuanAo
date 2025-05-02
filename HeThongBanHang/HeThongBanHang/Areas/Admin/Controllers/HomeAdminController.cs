using Microsoft.AspNetCore.Mvc;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Route("Admin")]
    //[Route("Admin/homeadmin")]

    public class HomeAdminController : BaseController
    {
        //[Route("")]
        //[Route("index")]
        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }
    }
}
    