namespace HeThongBanHang.Models;

public partial class CartItem
{
    public int Id { get; set; }

    public int? CartId { get; set; }

    public int? ProductId { get; set; }

    public int Quantity { get; set; }

    public int? ProductVariantId { get; set; }


    public virtual Cart? Cart { get; set; }

    public virtual Product? Product { get; set; }
    public ProductVariant? ProductVariant { get; set; }

}
