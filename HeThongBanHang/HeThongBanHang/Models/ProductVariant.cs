using System;
using System.Collections.Generic;

namespace HeThongBanHang.Models;

public partial class ProductVariant
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string Size { get; set; } = null!;

    public int? Quantity { get; set; }

    
    public virtual Product? Product { get; set; }  // <-- Cho phép null

}
