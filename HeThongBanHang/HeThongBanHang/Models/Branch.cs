namespace HeThongBanHang.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int ShopId { get; set; } // FK
        public string? Address { get; set; }

        public InfoShop? InfoShop { get; set; }
    }
}
