using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList.Extensions;


namespace HeThongBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QuanLiBanQuanAoContext _DbContext;

        public HomeController(ILogger<HomeController> logger, QuanLiBanQuanAoContext context)
        {
            _logger = logger;
            _DbContext = context;
        }


        [HttpGet]
        public IActionResult Index(int Page = 1)
        {
            int pageSize = 12;  // Số sản phẩm mỗi trang
            var products = _DbContext.Products.OrderBy(p => p.Id).ToPagedList(Page, pageSize);  // Sử dụng ToPagedList để phân trang

            var categories = _DbContext.Categories.ToList();
            ViewBag.Categories = categories; // Gán danh sách danh mục vào ViewBag
            // Lấy UserId từ Session để kiểm tra sản phẩm yêu thích
            var userId = HttpContext.Session.GetInt32("UserId");
            List<int> favoriteProductIds = new List<int>();
            if (userId != null)
            {
                favoriteProductIds = _DbContext.Favorite
                    .Where(f => f.UserId == userId && f.ProductId.HasValue)
                    .Select(f => f.ProductId!.Value) // dụng value vì productid kiểu int? nullable
                    .ToList(); // chuyển kết quả thành dạng list 
            }
            ViewBag.Favorites = favoriteProductIds;
            
            
            return View(products);
        }
        [HttpGet]
        public IActionResult IsFavorited(int productId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var isFavorited = _DbContext.Favorite.Any(f => f.UserId == userId && f.ProductId == productId);
                return Json(new { isFavorited });
            }

            return Json(new { isFavorited = false });
        }

        //trang xem chi tiết sản phẩm
        [HttpGet]
        public IActionResult QuickView(int id)
        {
            var product = _DbContext.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefault(x => x.Id == id);
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


        // partialview menu loaị sản phẩm tích hợp phân trang , lưu ý  menu header
        public IActionResult ByCategory(int id, int page = 1)
        {
            int pageSize = 12;

            // Lọc sản phẩm theo category
            var productsByCategory = _DbContext.Products.Where(p => p.CategoryId == id);

            IQueryable<Product> finalProducts;
            if (productsByCategory.Any())
            {
                // Có sản phẩm → dùng kết quả lọc theo category
                finalProducts = productsByCategory;
                ViewBag.IsCategoryFallback = false;
            }
            else
            {
                // Không có sản phẩm → fallback: trả toàn bộ danh sách sản phẩm
                finalProducts = _DbContext.Products;
                ViewBag.IsCategoryFallback = true;
            }

            var pagedProducts = finalProducts.ToPagedList(page, pageSize);

            ViewData["CategoryId"] = id;
            return View("Index", pagedProducts);
        }



        public IActionResult ByObjectType(int objectTypeId)
        {
            // Lấy danh sách categories theo ObjectTypeId
            var categories = _DbContext.Categories
                .Where(c => c.ObjectTypeId == objectTypeId)
                .OrderBy(c => c.Name)
                .ToList();

            // Lưu danh sách categories vào ViewData
            ViewData["Categories"] = categories;
            ViewData["ObjectTypeId"] = objectTypeId; // Lưu ObjectTypeId vào ViewData

            return View("Index"); // Trả về View hiển thị danh sách Category
        }



        //menu con ở view sản phẩm
        public IActionResult Filter(string[] sizes, int[] categoryIds, string sortBy, int page = 1)
        {
            int pageSize = 12;
            var products = _DbContext.Products.AsQueryable();

            // Lọc size nếu có
            if (sizes != null && sizes.Length > 0)
            {
                products = products.Where(p =>
                !p.ProductVariants.Any() || // giữ lại sản phẩm không có biến thể
                p.ProductVariants.Any(v => sizes.Contains(v.Size)));
            }

            // Lọc category nếu có
            if (categoryIds != null && categoryIds.Length > 0)
            {
                var filteredByCategory = products
                    .Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value));

                // Nếu có sản phẩm theo category thì dùng kết quả đó
                if (filteredByCategory.Any())
                {
                    products = filteredByCategory;
                    ViewBag.IsCategoryFallback = false;
                }
                else
                {
                    // Không có sản phẩm -> fallback: giữ nguyên danh sách hiện tại (có thể đã lọc theo size)
                    ViewBag.IsCategoryFallback = true;
                }
            }
            else
            {
                ViewBag.IsCategoryFallback = false;
            }

            // Sắp xếp
            products = sortBy switch
            {
                "priceAsc" => products.OrderBy(p => p.Price),
                "priceDesc" => products.OrderByDescending(p => p.Price),
                _ => products.OrderBy(p => p.Id)
            };

            var pagedProducts = products.ToPagedList(page, pageSize);
            var categories = _DbContext.Categories.ToList();

            // Tạo dictionary map id (string) -> name
            var categoryMap = categories.ToDictionary(c => c.Id.ToString(), c => c.Name);  //dictionary có 2 phần tử key(chuyển ID thành string) và value 

            ViewBag.CategoryMap = categoryMap;

            return View("Index", pagedProducts);
        }

        
       
    }
}
