using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeThongBanHang.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Username { get; set; } = null!;

    
    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? CustomerEmployeeId { get; set; }
    public bool IsDeleted { get; set; } = false;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual CustomerEmployee? CustomerEmployee { get; set; }
}
