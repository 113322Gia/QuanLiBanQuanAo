using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeThongBanHang.ViewComponents
{
    public class CategoryFilterViewComponent : ViewComponent
    {
        private readonly QuanLiBanQuanAoContext _DbContext;

        public CategoryFilterViewComponent(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(List<int> selectedCategoryIds)
        {
            var allCategories = await _DbContext.Categories.ToListAsync();

            ViewBag.SelectedCategoryIds = selectedCategoryIds ?? new List<int>();

            return View(allCategories);
        }
    }
}