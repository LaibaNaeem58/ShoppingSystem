namespace ShoppingSystem.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CustomerName { get; set; } = "";
        public double SubTotal { get; set; }
        public double DeliveryFee { get; set; }
        public double TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = "";
        public string PaymentStatus { get; set; } = "Paid";
        public string DeliveryType { get; set; } = "";
        public string DeliveryAddress { get; set; } = "";
        public string Status { get; set; } = "Confirmed";
        public string OrderDate { get; set; } = "";
        public string EstimatedDelivery { get; set; } = "";
    }
}