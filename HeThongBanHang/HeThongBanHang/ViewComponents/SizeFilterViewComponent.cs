using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.ViewComponents
{
    public class SizeFilterViewComponent : ViewComponent
    {
        private readonly QuanLiBanQuanAoContext _DbContext;

        public SizeFilterViewComponent(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<string> selectedSizes)
        {
            // Lấy danh sách các kích thước duy nhất từ ProductVariant
            var sizes = await _DbContext.ProductVariants
            .Select(pv => pv.Size)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

            ViewBag.SelectedSizes = selectedSizes ?? new List<string>();
            return View(sizes);
        }
    }
}