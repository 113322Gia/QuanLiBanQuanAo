using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Route("Admin/[controller]/[action]")]
    public class BaseController : Controller
    {

        public override void OnActionExecuting(ActionExecutingContext context) //OnActionExecuting sẽ tự động chạy trước mỗi action
        {
            var employeeid = HttpContext.Session.GetInt32("EmployeeId");
            var role = HttpContext.Session.GetString("Role");
            if (employeeid == null)
            {
                TempData["ErrorMessage"] = "Bạn phải đăng nhập";
                context.Result = RedirectToAction("Login", "Accout", new { area = "Admin" });
            }

            if (role == "Employee")
            {
                string actionName = context.ActionDescriptor.RouteValues["action"]?.ToLower(); // Lấy tên action từ route
                Console.WriteLine($"ActionName: {actionName}");

                var restrictedActions = new[] { "deleteproduct", "editproduct", "createproduct", "edit_employee", "delete_employee" };

                if (restrictedActions.Contains(actionName))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này.";
                    context.Result = RedirectToAction("Index", "HomeAdmin", new { area = "Admin" });
                    return;
                }
            }
            base.OnActionExecuting(context);
        }






    }
}
