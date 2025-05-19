using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    public class CategoryController : BaseController
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        
        public CategoryController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult CateGories_Index()
        {
            var categories = _DbContext.Categories.Include(c => c.ObjectType).ToList();

            ViewBag.ObjectTypes = _DbContext.ObjectType
                .Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }).ToList();

            return View(categories);
        }


        // Add category
        public IActionResult Create_Catgories()
        {
            
            return View();

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create_Catgories(Category categories)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thêm không thành công";
                return RedirectToAction("CateGories_Index");
            }

            _DbContext.Categories.Add(categories);
            _DbContext.SaveChanges();

            TempData["SuccessMessage"] = "Thêm thành công";
            return RedirectToAction("CateGories_Index");
        }





        //// sửa thể loại
        //public IActionResult Edit_Catgories(int id)
        //{
        //    var category = _DbContext.Categories.Find(id);
            
        //    return View(category);

        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Catgories(Category cagories)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ";
                return View(cagories);

            }
            var category = _DbContext.Categories.FirstOrDefault(x => x.Id == cagories.Id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "không tìm thấy thể loại";
                    return View(category);
                }
                category.Name = cagories.Name;
                category.ObjectTypeId = cagories.ObjectTypeId;
                _DbContext.Categories.Update(category);
                _DbContext.SaveChanges();
                TempData["SuccessMessage"] = "Sửa thành công";
            
            return RedirectToAction("CateGories_Index");

        }

        [HttpPost]
        public IActionResult Delete_Categories(int id)
        {
            var category = _DbContext.Categories.Find(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thể loại" });
            }
            var hasProducts = _DbContext.Products.Any(x => x.CategoryId == id);
            if (hasProducts)
            {
                return BadRequest("Không thể xóa thể loại này.Vui lòng xóa sản phẩm trước.");
            }
            _DbContext.Categories.Remove(category);
            _DbContext.SaveChanges();

            return Json(new { success = true });
        }


    }
}
