namespace ShoppingSystem.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string ProductName { get; set; } = "";
        public double ProductPrice { get; set; }
        public string Category { get; set; } = "";
    }
}