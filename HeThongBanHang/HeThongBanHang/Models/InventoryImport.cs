namespace HeThongBanHang.Models
{
    public class InventoryImport
    {
        public int Id { get; set; }
        public int ProductVariantId { get; set; }

        public int InitialQuantity { get; set; }   // Số lượng trước khi nhập kho
        public int QuantityChanged { get; set; }   // Số lượng nhập thêm
        public int NewQuantity { get; set; }       // Số lượng sau khi cộng vào

        public int EmployeeId { get; set; }        // Nhân viên thực hiện nhập kho
        public DateTime ChangeDate { get; set; }
        public string? Note { get; set; }

        public virtual Employee? Employee { get; set; }
        public virtual ProductVariant? ProductVariant { get; set; }
    }
}
