using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;


namespace HeThongBanHang.Areas.Admin.Controllers
{

    [Area("Admin")]
    public class IventoryController : BaseController
    {
        private readonly QuanLiBanQuanAoContext _DbContext;

        public IventoryController(QuanLiBanQuanAoContext Context)
        {
            _DbContext = Context;
        }



        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Inventory_Index()
        {
            var products = _DbContext.Products
                                     .Include(p => p.ProductVariants)  // Lấy luôn các biến thể
                                     .ToList();

            foreach (var product in products)
            {
                if (product.ProductVariants != null && product.ProductVariants.Any())
                {
                    // Nếu có biến thể, bạn có thể tính toán lại số lượng nếu cần thiết
                    // (ví dụ: trừ đi số lượng đã bán trong đơn hàng)
                    Console.WriteLine($"Product: {product.Name} has variants.");
                }
                else
                {
                    Console.WriteLine($"Product: {product.Name} has no variants.");
                }
            }

            return View(products);
        }


        // thêm size, price , quantity
        [HttpGet]
        public IActionResult CreateVariant(int productId)
        {
            var product = _DbContext.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ProductName = product.Name;

            var model = new ProductVariant
            {
                ProductId = productId // gán trực tiếp vào model luôn
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateVariant(ProductVariant variant)
        {
            if (ModelState.IsValid)
            {
                if (variant.ProductId == 0)
                {
                    TempData["ErrorMessage"] = "Không xác định được sản phẩm!";
                    return View(variant);
                }

                try
                {
                    var employeeId = HttpContext.Session.GetInt32("EmployeeId");
                    if (employeeId == null)
                    {
                        TempData["ErrorMessage"] = "Không xác định được nhân viên!";
                        return View(variant);
                    }

                    int initialQuantity = 0;
                    int quantityChanged = variant.Quantity ?? 0;
                    int newQuantity = 0;

                    var existingVariant = _DbContext.ProductVariants
                        .FirstOrDefault(v => v.ProductId == variant.ProductId && v.Size == variant.Size);

                    if (existingVariant != null)
                    {
                        initialQuantity = existingVariant.Quantity ?? 0;
                        existingVariant.Quantity += quantityChanged;
                        newQuantity = existingVariant.Quantity ?? 0;

                        _DbContext.ProductVariants.Update(existingVariant);
                    }
                    else
                    {
                        initialQuantity = 0;
                        newQuantity = quantityChanged;

                        _DbContext.ProductVariants.Add(variant);
                        _DbContext.SaveChanges(); // Lưu để lấy được variant.Id mới
                    }

                    // Nếu vừa thêm mới, cần đảm bảo có Id để lưu vào InventoryImport
                    var variantId = existingVariant?.Id ?? variant.Id;

                    var importRecord = new InventoryImport
                    {
                        ProductVariantId = variantId,
                        InitialQuantity = initialQuantity,
                        QuantityChanged = quantityChanged,
                        NewQuantity = newQuantity,
                        EmployeeId = employeeId.Value,
                        ChangeDate = DateTime.Now,
                        Note = "Nhập kho"
                    };

                    _DbContext.InventoryImports.Add(importRecord);
                    _DbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Thêm hoặc cập nhật biến thể thành công!";
                    return RedirectToAction("Inventory_Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi lưu dữ liệu: {ex.Message}";
                    return View(variant);
                }
            }

            TempData["ErrorMessage"] = "Lỗi khi thêm biến thể! (ModelState không hợp lệ)";
            return View(variant);
        }

       

        public ActionResult UpdateStockQuantity(int orderId)
        {
            // Lấy đơn hàng từ DbContext
            var order = _DbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại!";
                return RedirectToAction("Order_Detail", new { id = orderId });
            }

            // Lặp qua tất cả OrderItems để cập nhật số lượng kho
            foreach (var orderItem in order.OrderItems)
            {
                var productVariant = _DbContext.ProductVariants
                    .FirstOrDefault(pv => pv.Id == orderItem.ProductVariantId);

                if (productVariant != null)
                {
                    // Kiểm tra nếu số lượng trong kho và số lượng đơn hàng đều có giá trị
                    if (productVariant.Quantity.HasValue && orderItem.Quantity.HasValue)
                    {
                        // Kiểm tra nếu số lượng trong kho đủ
                        if (productVariant.Quantity.Value >= orderItem.Quantity.Value)
                        {
                            // Giảm số lượng trong kho
                            productVariant.Quantity -= orderItem.Quantity.Value;
                            _DbContext.ProductVariants.Update(productVariant);
                        }
                        else
                        {
                            TempData["ErrorMessage"] = $"Không đủ số lượng sản phẩm {productVariant.Size}!";
                            return RedirectToAction("Order_Detail", new { id = orderId });
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Số lượng trong kho hoặc số lượng đơn hàng không hợp lệ!";
                        return RedirectToAction("Order_Detail", new { id = orderId });
                    }
                }
                else
                {
                    // Nếu không có biến thể cho sản phẩm, bỏ qua
                    TempData["ErrorMessage"] = "Không có biến thể cho sản phẩm!";
                    return RedirectToAction("Order_Detail", new { id = orderId });
                }
            }

            // Lưu thay đổi vào DbContext
            _DbContext.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật số lượng kho thành công!";
            return RedirectToAction("Order_Detail", new { id = orderId });
        }



        public IActionResult HistoryChange(int id)
        {
            var history = _DbContext.HistoryInventory
            .Include(o => o.Order)
            .Include(o => o.Productvariant)
            .ThenInclude(oi => oi.Product)
            .Where(x => x.Productvariant.ProductId == id)
            .OrderByDescending(x => x.ChangeDate)
            .ToList();
            if (history == null || !history.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Inventory_Index");
            }
            return View(history);
        }



       

    }
}
