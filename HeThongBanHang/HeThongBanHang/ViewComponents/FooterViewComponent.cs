using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        public FooterViewComponent(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var branches = await _DbContext.Branches
                .Include(x => x.InfoShop)
                .ToListAsync();
            return View(branches);
        }
    }
}
