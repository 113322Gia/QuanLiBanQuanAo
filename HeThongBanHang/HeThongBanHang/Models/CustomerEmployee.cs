namespace HeThongBanHang.Models
{
    public class CustomerEmployee
    {
        public int Id { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }

        public virtual Employee? Employee { get; set; }
    }

}
