namespace HeThongBanHang.Models
{
    public class InfoShop
    {
        public int Id { get; set; }
                 // KHÔNG nullable, nhưng khởi tạo với `null!` để tránh warning

        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;


        public ICollection<Branch> ?Branches { get; set; }

    }
}
