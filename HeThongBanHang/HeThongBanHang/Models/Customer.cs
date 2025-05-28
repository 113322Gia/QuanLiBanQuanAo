namespace HeThongBanHang.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Tel { get; set; }




        public ICollection<Order>? Orders { get; set; }

    }
}
