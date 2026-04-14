namespace ShoppingSystem.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public double Amount { get; set; }
        public string Method { get; set; } = "";
        public string Status { get; set; } = "Success";
        public string TransactionId { get; set; } = "";
        public string PaymentDate { get; set; } = "";
    }
}