using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LayoutController : BaseController
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        public LayoutController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Footer_Content()
        {
            var lst = _DbContext.Branches
                .Include(x => x.InfoShop)
                .ToList();
            return View(lst);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string address, int shopId)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return RedirectToAction("Footer_Content");
            }

            var branch = new Branch
            {
                Name = name,
                Address = address,
                ShopId = shopId
            };

            _DbContext.Branches.Add(branch);

            try
            {
                await _DbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm chi nhánh thành công!";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi thêm chi nhánh: " + ex.Message;
                Console.WriteLine(ex);
            }

            return RedirectToAction("Footer_Content");
        }

        // hàm chỉnh sửa địa chỉ chi nhánh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string address)
        {
            var branch = await _DbContext.Branches
                                        .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy chi nhánh cần chỉnh sửa.";
                return RedirectToAction("Footer_Content");
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return RedirectToAction("Footer_Content");
            }

            branch.Name = name;
            branch.Address = address;

            _DbContext.Branches.Update(branch);

            try
            {
                await _DbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Chỉnh sửa chi nhánh thành công!";
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi chỉnh sửa chi nhánh: " + ex.Message;
                Console.WriteLine(ex);
            }

            return RedirectToAction("Footer_Content");
        }



        // --- Action xử lý xóa chi nhánh (từ AJAX request) ---
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var branchToDelete = await _DbContext.Branches
                .Include(b => b.InfoShop)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branchToDelete == null)
            {
                return Json(new { success = false, message = "Không tìm thấy chi nhánh để xóa." });
            }

            try
            {
                // KHÔNG xóa InfoShop vì là dùng chung
                _DbContext.Branches.Remove(branchToDelete);

                await _DbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa chi nhánh thành công!";
                return Json(new { success = true });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("FK_") == true)
                {
                    TempData["ErrorMessage"] = "Không thể xóa chi nhánh này vì có dữ liệu liên quan.";
                    return Json(new { success = false, message = "Không thể xóa chi nhánh này vì có dữ liệu liên quan." });
                }
                TempData["ErrorMessage"] = "Lỗi khi xóa chi nhánh: " + ex.Message;
                Console.WriteLine(ex.ToString());
                return Json(new { success = false, message = "Lỗi khi xóa chi nhánh." });
            }
        }
        // Hàm chỉnh sửa thông tin chung
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateInfoShop(string Email, string PhoneNumber)
        {
            var infoShop = await _DbContext.InfoShop.FirstOrDefaultAsync(); // hoặc theo ID nếu có nhiều
            if (infoShop != null)
            {
                infoShop.Email = Email;
                infoShop.PhoneNumber = PhoneNumber;
                await _DbContext.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin chung thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin chung.";
            }

            return RedirectToAction("Footer_Content");
        }


    }
}
