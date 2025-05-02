using System.Diagnostics;
using System.Net.NetworkInformation;
using HeThongBanHang.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;


namespace HeThongBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QuanLiBanQuanAoContext _DbContext;

        public HomeController(ILogger<HomeController> logger , QuanLiBanQuanAoContext context)
        {
            _logger = logger;
            _DbContext = context;
        }


        [HttpGet]
        public IActionResult Index(int Page = 1)
        {
            int pageSize = 8;  // Số sản phẩm mỗi trang
            var products = _DbContext.Products.OrderBy(p => p.Id).ToPagedList(Page, pageSize);  // Sử dụng ToPagedList để phân trang

            return View(products);
      
        }
        //trang xem chi tiết sản phẩm
        [HttpGet]
        public IActionResult QuickView(int id)
        {
            var product = _DbContext.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefault(x=>x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        // partialview menu loaị sản phẩm tích hợp phân trang
        public IActionResult ByCategory(int id, int page = 1)
        {
            int pageSize = 8; // Số sản phẩm mỗi trang
            var products = _DbContext.Products
                                     .Where(x => x.CategoryId == id)
                                     .OrderBy(x => x.Id)  // Sắp xếp theo Id
                                     .ToPagedList(page, pageSize);  // Phân trang

            

            ViewData["CategoryId"] = id;  
            return View("Index",products);
        }


    }
}
