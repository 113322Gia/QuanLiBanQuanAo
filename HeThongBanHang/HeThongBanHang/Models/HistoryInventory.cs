namespace HeThongBanHang.Models
{
    public class HistoryInventory
    {
        public int Id { get; set; }
        public int? ProductVariantId { get; set; }
        public string? ChangeType { get; set; }

        public int? QuantityChanged { get; set; }
        public int? NewQuantity { get; set; }
        public DateTime? ChangeDate { get; set; }

        public int? OrderId { get; set; }

        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }


        public virtual Order? Order { get; set; }
        public virtual ProductVariant? Productvariant { get; set; }

    }
}
