using System.ComponentModel.DataAnnotations;

namespace HeThongBanHang.Models;

public partial class Category
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    public int? ObjectTypeId { get; set; }
    public ObjectType? ObjectType { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
