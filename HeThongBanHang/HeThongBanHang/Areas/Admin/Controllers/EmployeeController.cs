using HeThongBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HeThongBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class EmployeeController : BaseController
    {
        private readonly QuanLiBanQuanAoContext _DbContext;
        public EmployeeController(QuanLiBanQuanAoContext context)
        {
            _DbContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Employee_Index()
        {
            var employee = _DbContext.Employees
                .Include(o => o.CustomerEmployee)
                .ToList();
            return View(employee);
        }


        public IActionResult Edit_Employee(int id)
        {
            var employee = _DbContext.Employees
                .Include(e => e.CustomerEmployee)
                .FirstOrDefault(e => e.Id == id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên.";
                return RedirectToAction("Employee_Index");
            }

            if (employee.CustomerEmployee == null)
            {
                employee.CustomerEmployee = new CustomerEmployee();
            }

            return View(employee);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit_Employee(int id, Employee model)
        {
            var employee = _DbContext.Employees
                .Include(e => e.CustomerEmployee)
                .FirstOrDefault(e => e.Id == id);

            if (employee != null)
            {
                employee.FullName = model.FullName;
                employee.Username = model.Username;
                employee.Role = model.Role;
                employee.IsDeleted = model.IsDeleted;

                if (employee.CustomerEmployee == null)
                {
                    var newCustomerEmployee = new CustomerEmployee
                    {
                        Email = model.CustomerEmployee?.Email,
                        Phone = model.CustomerEmployee?.Phone
                    };

                    // Gán ngược quan hệ
                    employee.CustomerEmployee = newCustomerEmployee;

                    _DbContext.CustomerEmployee.Add(newCustomerEmployee);
                }
                else
                {
                    employee.CustomerEmployee.Email = model.CustomerEmployee?.Email;
                    employee.CustomerEmployee.Phone = model.CustomerEmployee?.Phone;

                }

                _DbContext.SaveChanges();
                TempData["SuccessMessage"] = "Employee details updated successfully.";
                return RedirectToAction("Employee_Index");
            }

            TempData["ErrorMessage"] = "Employee not found.";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete_Employee(int id)
        {
            var employee = _DbContext.Employees.Find(id);
            if (employee == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên";
                return RedirectToAction("Employee_Index");
            }

            // Soft Delete: đánh dấu là đã xóa
            employee.IsDeleted = true;
            _DbContext.SaveChanges();

            TempData["SuccessMessage"] = "Xóa thành công (Soft Delete)";
            return RedirectToAction("Employee_Index");
        }



    }
}
