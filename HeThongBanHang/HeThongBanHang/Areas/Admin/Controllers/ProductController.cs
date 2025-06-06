﻿using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : BaseController
    {
        private readonly QuanLiBanQuanAoContext _DbContext; //biến _DbContext chứa instand của lớp đại diện Database
        public ProductController(QuanLiBanQuanAoContext context) // đây là 1 constructor
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {

            return View();
        }
        public async Task<IActionResult> Product_Index(string? filter = null) 
        {
            
            IQueryable<Product> products = _DbContext.Products
                .Include(p => p.Category)        
                .Include(t => t.ProductVariants); 
            ViewBag.CurrentFilter = filter;
            switch (filter)
            {
                case "FavoriteProduct":
                    var favoriteProductIds = await _DbContext.Favorite
                                                    .Select(fp => fp.ProductId)
                                                    .Distinct()
                                                    .ToListAsync();
                    products = products.Where(p => favoriteProductIds.Contains(p.Id));
                    break;

                case "BestSellerProduct":
                    
                    var bestSellerProductIds = await _DbContext.OrderItems
                                                    .GroupBy(od => od.ProductId) //gom nhóm productId trong bảng orderitem 
                                                    .Select(g => new { ProductId = g.Key, TotalQuantitySold = g.Sum(od => od.Quantity) })// tính tổng số lượng
                                                    .OrderByDescending(x => x.TotalQuantitySold) // sắp xếp lại từ cao đến thấp
                                                    .Take(10)  // xếp hạng 10 sản phẩm đầu tiên
                                                    .Select(x => x.ProductId) // lấy productid từ 10 sản phẩm đó 
                                                    .ToListAsync(); // trả về list 
                    products = products.Where(p => bestSellerProductIds.Contains(p.Id));
                    break;

                case "UnsoldProduct":
                    
                    var soldProductIds = await _DbContext.OrderItems
                                                .Select(od => od.ProductId)
                                                .Distinct()
                                                .ToListAsync();
                    
                    products = products.Where(p => !soldProductIds.Contains(p.Id));
                    break;

                default:
                    
                    break;
            }
            var productList = await products.ToListAsync();
            return View(productList);
        }

        // thêm sản phẩm
        [HttpGet]
        public IActionResult CreateProduct()
        {
            var cateGories = _DbContext.Categories.ToList();
            ViewBag.Categories = new SelectList(cateGories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateProduct(Product proDuct, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Xử lý ảnh sản phẩm
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }
                    proDuct.ImageUrl = "/Uploads/" + fileName;
                }
                _DbContext.Add(proDuct);
                _DbContext.SaveChanges();
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToAction("Product_Index");
            }
            // Nếu không hợp lệ, trả lại form với thông báo lỗi
            TempData["ErrorMessage"] = "There was an error creating the product. Please try again.";
            return View(proDuct);
        }








        //sửa sản phẩm

        [HttpGet]
        public IActionResult EditProduct(int id)
        {

            ViewBag.Categories = _DbContext.Categories.
                Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }).ToList();

            var proDuct = _DbContext.Products.Find(id);
            if (proDuct == null)
            {
                TempData["ErrorMessage"] = "Product not found";
            }
            return View(proDuct);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProduct(Product proDuct, IFormFile? imageFile, string? oldImageUrl)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = _DbContext.Products.FirstOrDefault(p => p.Id == proDuct.Id);
                if (existingProduct == null)
                {
                    TempData["ErrorMessage"] = "Product not found.";
                    return RedirectToAction("Product_Index");
                }

                // Cập nhật các thông tin sản phẩm
                existingProduct.Name = proDuct.Name;
                existingProduct.Price = proDuct.Price;
                //existingProduct.Quantity = proDuct.Quantity;
                existingProduct.CategoryId = proDuct.CategoryId;
                existingProduct.Description = proDuct.Description;
                //existingProduct.Size = proDuct.Size;

                // Xử lý ảnh mới (nếu có)
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Uploads");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }

                    existingProduct.ImageUrl = "/Uploads/" + fileName; // Lưu đường dẫn ảnh mới

                    // Xoá ảnh cũ nếu có
                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", oldImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath); // Xóa ảnh cũ
                        }
                    }
                }
                else
                {
                    // Nếu không có ảnh mới thì giữ ảnh cũ
                    existingProduct.ImageUrl = oldImageUrl;
                }

                // Lưu các thay đổi vào database
                _DbContext.SaveChanges();

                TempData["SuccessMessage"] = "Product updated successfully!";
                return RedirectToAction("Product_Index");
            }

            TempData["ErrorMessage"] = "Update failed. Please check the data and try again.";
            return View(proDuct); // Trả về view với thông tin sản phẩm nếu không hợp lệ
        }




        // xóa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteProduct(int id)
        {
            var proDuct = _DbContext.Products.FirstOrDefault(p => p.Id == id);
            if (proDuct == null)
            {
                TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Product_Index");
            }
            bool existsInOrder = _DbContext.OrderItems.Any(oi => oi.ProductId == id);
            if (existsInOrder)
            {
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm vì đã có trong đơn hàng.";
                return RedirectToAction("Product_Index");
            }
            _DbContext.Products.Remove(proDuct);
            _DbContext.SaveChanges();
            TempData["SuccessMessage"] = "Deleted successfully";
            return RedirectToAction("Product_Index");
        }

    }
}
