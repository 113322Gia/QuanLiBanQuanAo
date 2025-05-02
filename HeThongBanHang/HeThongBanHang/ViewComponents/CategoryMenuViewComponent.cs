using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.ViewComponents
{
    
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        public CategoryMenuViewComponent(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories =await _DbContext.Categories.ToListAsync();
            return View(categories);
        }
    }
}


