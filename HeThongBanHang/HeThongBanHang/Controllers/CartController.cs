using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using HeThongBanHang.Services;

namespace HeThongBanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly QuanLiBanQuanAoContext _DbContext;

        public CartController(IHttpContextAccessor httpContextAccessor, QuanLiBanQuanAoContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _DbContext = dbContext;
        }
        public IActionResult Index()
        {

            return View();
        }




        /*Người dùng bấm “Add to cart”.

        Gửi POST lên CartController.AddToCart.

        Kiểm tra sản phẩm tồn tại trong DB.

        Đọc giỏ hàng từ session.

        Cập nhật hoặc thêm mới sản phẩm.

        Ghi session lại.

        Chuyển về trang chủ.*/

        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1, int productVariantId = 0)
        {
            var product = _DbContext.Products.FirstOrDefault(p => p.Id == productId);
            var variant = _DbContext.ProductVariants.FirstOrDefault(v => v.Id == productVariantId);

            if (product == null || variant == null)
            {
                return BadRequest(); // Trả lỗi cho Ajax
            }

            var session = HttpContext.Session;
            var cartJson = session.GetString("MY_CART");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == productVariantId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = productId,
                    ProductVariantId = productVariantId,
                    Quantity = quantity
                });
            }

            session.SetString("MY_CART", JsonConvert.SerializeObject(cart));

            return Ok(); // Báo thành công cho Ajax
        }



        public IActionResult CartDetail()
        {
            return View();
        }



        public IActionResult CheckOut()
        {
            var session = HttpContext.Session;
            var cartJson = session.GetString("MY_CART");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            foreach (var cartItem in cart)
            {
                var product = _DbContext.Products.Include(p => p.ProductVariants)
                    .FirstOrDefault(p => p.Id == cartItem.ProductId);

                if (product != null)
                {
                    cartItem.Product = product;
                    var productVariant = product.ProductVariants.FirstOrDefault(v => v.Id == cartItem.ProductVariantId);
                    if (productVariant != null)
                    {
                        cartItem.ProductVariant = productVariant;
                    }
                }
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var user = _DbContext.Users.FirstOrDefault(u => u.Id == userId.Value);
                if (user != null && user.CustomerId.HasValue)
                {
                    var customer = _DbContext.Customers.FirstOrDefault(c => c.Id == user.CustomerId.Value);
                    ViewBag.Customer = customer;
                }
            }
            var paymentMethod = _DbContext.Payment.ToList();
            ViewBag.PaymentMethod = paymentMethod;
            return View(cart);
        }




        ///thay đổi số lượng trong giỏ hàng

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            try
            {
                // Lấy sản phẩm từ database
                var product = _DbContext.Products.FirstOrDefault(p => p.Id == productId);
                if (product == null)
                {
                    return NotFound(); // Nếu sản phẩm không tìm thấy
                }

                // Lấy giỏ hàng từ session
                var session = HttpContext.Session;
                var cartJson = session.GetString("MY_CART");
                var cart = string.IsNullOrEmpty(cartJson)
                    ? new List<CartItem>()
                    : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

                // Tìm sản phẩm trong giỏ hàng và cập nhật số lượng
                var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity = quantity; // Cập nhật số lượng
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.Id,
                        Quantity = quantity
                    });
                }

                // Lưu lại giỏ hàng vào session
                session.SetString("MY_CART", JsonConvert.SerializeObject(cart));

                return Json(new { success = true }); // Gửi phản hồi JSON
            }
            catch (Exception ex)
            {
                // Log lỗi chi tiết
                Console.WriteLine($"Error in UpdateCart: {ex.Message}");
                return BadRequest(); // Trả về lỗi nếu có ngoại lệ
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            var session = HttpContext.Session;
            var cartJson = session.GetString("MY_CART");
            var cart = string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson);

            var existingItem = cart.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                cart.Remove(existingItem);
                session.SetString("MY_CART", JsonConvert.SerializeObject(cart));
            }
            else
            {
                Console.WriteLine("Sản phẩm không có trong giỏ hàng.");
            }

            return Ok(); // Trả về phản hồi thành công
        }


        [HttpPost]
        public IActionResult PlaceOrder(List<CartItem> cartItems, string fullName, string email, string address, string city, string tel, int PaymentId)
        {
            if (cartItems == null || !cartItems.Any())
            {
                return RedirectToAction("Index", "Cart"); // Không có sản phẩm nào
            }

            Customer? customer = null;

            // Kiểm tra nếu khách hàng đã đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue) // Nếu đã đăng nhập
            {
                var user = _DbContext.Users.FirstOrDefault(u => u.Id == userId.Value);
                if (user != null && user.CustomerId.HasValue)
                {
                    customer = _DbContext.Customers.FirstOrDefault(c => c.Id == user.CustomerId);
                }
            }

            if (customer == null) // Nếu chưa có CustomerId, tức là khách vãng lai
            {
                customer = _DbContext.Customers.FirstOrDefault(c => c.Email == email);
                if (customer == null)
                {
                    // Tạo mới khách hàng
                    customer = new Customer
                    {
                        FullName = fullName,
                        Email = email,
                        Address = address,
                        City = city,
                        Tel = tel
                    };
                    _DbContext.Customers.Add(customer);
                    _DbContext.SaveChanges();
                }

                // Lưu CustomerId vào bảng User nếu là khách hàng mới
                if (userId.HasValue)
                {
                    var user = _DbContext.Users.FirstOrDefault(u => u.Id == userId.Value);
                    if (user != null && user.CustomerId == null)
                    {
                        user.CustomerId = customer.Id;
                        _DbContext.SaveChanges();
                    }
                }
            }

            // Lưu thông tin khách hàng vào session
            HttpContext.Session.SetInt32("CustomerId", customer.Id);

            // Tính tổng tiền
            var total = cartItems.Sum(item =>
            {
                var product = _DbContext.Products.FirstOrDefault(p => p.Id == item.ProductId);
                return product != null ? product.Price * item.Quantity : 0;
            });

            // Tạo đơn hàng với Status = 1 (Đang xử lý)
            var order = new Order
            {
                CreatedAt = DateTime.Now,
                TotalPrice = total,
                CustomerId = customer.Id, // Gán CustomerId cho đơn hàng
                UserId = userId,           // Nếu đã đăng nhập, gán UserId
                EmployeeId = null,         // Chờ xác nhận bởi nhân viên
                Status = 1,                // Đang xử lý
                PaymentId = PaymentId,
                OrderItems = new List<OrderItem>()
            };

            foreach (var item in cartItems)
            {
                var product = _DbContext.Products.FirstOrDefault(p => p.Id == item.ProductId);
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    UnitPrice = product?.Price
                });
            }

            _DbContext.Orders.Add(order);
            _DbContext.SaveChanges();
            HttpContext.Session.SetInt32("LastOrderId", order.Id);
            TempData["NewOrderId"] = order.Id;

            // Xóa giỏ hàng sau khi đặt hàng
            HttpContext.Session.Remove("MY_CART");

            // Trả về trang hóa đơn
            return RedirectToAction("ViewInvoice", new { orderId = order.Id });
        }








        // Hàm hiển thị hóa đơn của khách hàng vừa đặt sản phẩm
        public IActionResult ViewInvoice(int orderId)
        {
            var order = _DbContext.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var checkCustomerId = HttpContext.Session.GetInt32("CustomerId");
            var checkUserId = HttpContext.Session.GetInt32("UserId");  // người dùng đã đăng nhập
            bool isErrorFromTempData = TempData["ErrorMessage"] != null;

            if (!isErrorFromTempData)
            {
                // Nếu là khách vãng lai → so sánh CustomerId
                if (checkUserId == null && (checkCustomerId == null || checkCustomerId != order.CustomerId))
                {
                    TempData["ErrorMessage"] = "Bạn không được phép xem hóa đơn của người khác.";
                    return RedirectToAction("Index", "Home");
                }

                // Nếu là khách đăng nhập → so sánh UserId
                if (checkUserId != null && checkUserId != order.UserId)
                {
                    TempData["ErrorMessage"] = "Bạn không được phép xem hóa đơn của người khác.";
                    return RedirectToAction("Index", "Home");
                }
            }

            // Load thông tin khách vãng lai (nếu có)
            var customer = _DbContext.Customers.FirstOrDefault(c => c.Id == order.CustomerId);
            if (customer != null)
            {
                ViewBag.FullName = customer.FullName;
                ViewBag.Email = customer.Email;
                ViewBag.Address = customer.Address;
                ViewBag.City = customer.City;
                ViewBag.Tel = customer.Tel;
            }

            return View(order);
        }







        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var orderId = HttpContext.Session.GetInt32("LastOrderId");

            if (orderId == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index", "Home");
            }

            var order = _DbContext.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefault(o => o.Id == orderId.Value);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Đơn hàng không tồn tại.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra nếu đơn hàng đã được xác nhận (Status == 3)
            if (order.Status == 3)
            {
                TempData["ErrorMessage"] = "Đơn hàng này đã được xác nhận, không thể hủy.";
                return RedirectToAction("ViewInvoice", new { orderId = order.Id });
            }

            // Kiểm tra trạng thái đơn hàng có phải là đang xử lý hoặc đã xác nhận
            if (order.Status != 1 && order.Status != 4) // Chỉ cho hủy đơn Đang xử lý hoặc Vừa xác nhận
            {
                TempData["ErrorMessage"] = "Không thể hủy đơn hàng ở trạng thái hiện tại.";
                return RedirectToAction("ViewInvoice", new { orderId = order.Id });
            }

            // Cộng lại tồn kho
            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariant != null)
                {
                    // Kiểm tra nếu Quantity có giá trị null hay không
                    int quantityChanged = item.Quantity ?? 0; // Sử dụng giá trị mặc định 0 nếu Quantity là null

                    // Cập nhật tồn kho
                    item.ProductVariant.Quantity = (item.ProductVariant.Quantity ?? 0) + quantityChanged;

                    // Ghi lại lịch sử thay đổi tồn kho
                    int newQuantity = item.ProductVariant.Quantity ?? 0;
                    await LogInventoryChange(item.ProductVariant.Id, "Increase", quantityChanged, newQuantity, order.Id);
                }
            }

            // Cập nhật trạng thái đơn hàng
            order.Status = 2; // Đã hủy
            _DbContext.SaveChanges();

            // Xóa thông tin đơn hàng trong session
            HttpContext.Session.Remove("LastOrderId");

            TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công và hoàn trả tồn kho.";
            return RedirectToAction("ViewInvoice", new { orderId = order.Id });
        }








        //hàm xác nhận của khách hàng tích hợp gửi mail
        [HttpPost]
        public async Task<IActionResult> ConfirmByCustomer(int id)
        {
            var order = _DbContext.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .Include(o => o.Customer)
                .Include(o => o.User)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == 2)
            {
                TempData["ErrorMessage"] = "Đơn hàng đã bị hủy trước đó.";
                return RedirectToAction("ViewInvoice", new { orderId = id });
            }

            if (order.Status == 3 || order.Status == 4)
            {
                TempData["ErrorMessage"] = "Đơn hàng đã được xác nhận và không thể thay đổi.";
                return RedirectToAction("ViewInvoice", new { orderId = id });
            }

            // Status là 1 thì cập nhật
            if (order.Status == 1)
            {
                foreach (var item in order.OrderItems)
                {
                    var variant = item.ProductVariant;
                    if (variant != null && variant.Quantity >= item.Quantity)
                    {
                        variant.Quantity -= item.Quantity;

                        // Ghi lại lịch sử thay đổi tồn kho
                        await LogInventoryChange(variant.Id, "Decrease", item.Quantity.Value, variant.Quantity.Value, order.Id);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Không đủ hàng trong kho cho sản phẩm: " + variant?.Size;
                        return RedirectToAction("ViewInvoice", new { orderId = order.Id });
                    }
                }

                order.Status = 4; // Khách đã đồng ý
                _DbContext.SaveChanges();

                // Gửi email
                var emailService = new EmailService();
                var customerName = order.Customer?.FullName ?? "Quý khách";
                var email = order.Customer?.Email;

                if (!string.IsNullOrEmpty(email))
                {
                    // Tạo chi tiết đơn hàng
                    string orderDetails = "<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 100%;'>";
                    string subject = $"Xác nhận đơn hàng #{order.Id}";

                    orderDetails += "<thead><tr><th>Product</th><th>Size</th><th>Quantity</th><th>Price</th><th>Total</th></tr></thead><tbody>";

                    foreach (var item in order.OrderItems)
                    {
                        var productName = item.Product?.Name ?? "Không có tên sản phẩm";
                        var productSize = item.ProductVariant?.Size ?? "Không có size";
                        var productPrice = item.Product?.Price ?? 0;
                        var totalPrice = item.Quantity * productPrice;

                        var productPriceStr = productPrice.ToString("N0") + "₫";
                        var totalPriceStr = (totalPrice ?? 0).ToString("N0") + "₫";

                        orderDetails += $"<tr><td>{productName}</td><td>{productSize}</td><td>{item.Quantity}</td><td>{productPriceStr}</td><td>{totalPriceStr}</td></tr>";
                    }

                    orderDetails += "</tbody></table>";

                    // Nội dung email
                    string body = $@"
            <h3>Xin chào {customerName},</h3>
            <p>Chúng tôi đã nhận được xác nhận từ bạn cho đơn hàng #{order.Id}.</p>
            <p>Đơn hàng sẽ được xử lý và giao hàng trong thời gian sớm nhất.</p>
            <p>Dưới đây là chi tiết đơn hàng của bạn:</p>
            {orderDetails}
            <p>Cảm ơn bạn đã mua sắm tại Shop Quần Áo!</p>
            <p>Trân trọng,</p>
            <strong>Shop Quần Áo</strong>";

                    // Gửi email
                    await emailService.SendEmailAsync(email, subject, body);
                }

                TempData["SuccessMessage"] = "Order Confirmation Successful. Đã gửi email xác nhận.";
            }

            return RedirectToAction("ViewInvoice", new { orderId = order.Id });
        }




        // hàm ghi lại lịch sử thay đổi số lượng
        private async Task LogInventoryChange(int productVariantId, string actionType, int quantityChanged, int newQuantity, int orderId)
        {
            if (string.IsNullOrEmpty(actionType))
            {
                throw new ArgumentException("Action type cannot be null or empty", nameof(actionType));
            }

            // Tạo đối tượng HistoryInventory
            var log = new HistoryInventory
            {
                ProductVariantId = productVariantId,
                ChangeType = actionType,
                QuantityChanged = quantityChanged,
                NewQuantity = newQuantity,
                OrderId = orderId,
                ChangeDate = DateTime.UtcNow // Dùng UTC thay vì Local Time
            };

            // Thêm vào DbContext
            _DbContext.HistoryInventory.Add(log);
            await _DbContext.SaveChangesAsync();
        }


    }
}
