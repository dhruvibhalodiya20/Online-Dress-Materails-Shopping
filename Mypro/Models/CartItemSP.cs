namespace Mypro.Models
{
    public class CartItemSP
    {
        public int Id { get; set; }
        public int ImageId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string HexColor { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}
