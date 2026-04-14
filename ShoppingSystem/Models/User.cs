namespace ShoppingSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "Customer";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }
}