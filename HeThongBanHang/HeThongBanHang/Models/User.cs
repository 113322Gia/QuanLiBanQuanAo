using System;
using System.Collections.Generic;


namespace HeThongBanHang.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int? CustomerId { get; set; }
    public virtual Cart? Cart { get; set; }
    public virtual Customer? Customers { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
