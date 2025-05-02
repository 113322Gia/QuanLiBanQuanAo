using System;
using System.Collections.Generic;

namespace HeThongBanHang.Models;

public partial class Order
{
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public decimal? TotalPrice { get; set; }

    public int? UserId { get; set; }
    public int? CustomerId { get; set; }

    public int? EmployeeId { get; set; }
    public int? PaymentId { get; set; }

    public int? Status { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? User { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual Payment? Payment { get; set; }
}
