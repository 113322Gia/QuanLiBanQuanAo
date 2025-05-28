namespace HeThongBanHang.Models
{
    public class ObjectType
    {
        public int Id { get; set; }
        public string? Name { get; set; }  // Top, Bottom, Footwear, ...
        public ICollection<Category>? Categories { get; set; }
    }
}
