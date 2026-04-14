namespace ShoppingSystem.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = "";
        public string ProductName { get; set; } = "";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public string ReviewDate { get; set; } = "";
    }
}