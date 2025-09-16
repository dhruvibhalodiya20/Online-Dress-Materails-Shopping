namespace Mypro.Models
{
    public class CartSummarySP
    {
        public decimal SubTotal { get; set; }
        public decimal OrderDiscountPercent { get; set; }
        public decimal DateDiscountPercent { get; set; }
        public decimal TotalDiscountPercent { get; set; }
        public decimal TotalDiscountAmount { get; set; }
        public decimal GSTPercent { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public bool IsFirstOrder { get; set; }
    }
}
