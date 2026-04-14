using ShoppingSystem.Database;
using ShoppingSystem.UI;
using ShoppingSystem.Models;

namespace ShoppingSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            // ✅ Initialize database (VERY IMPORTANT)
            DatabaseManager.Initialize();

            var authMenu = new AuthMenu();

            User? user = authMenu.Show();

            if (user == null)
                return;

            if (user.Role == "Admin")
            {
                var adminPanel = new AdminPanel(user);
                adminPanel.Show();
            }
            else
            {
                var customerPanel = new CustomerPanel(user);
                customerPanel.Show();
            }
        }
    }
}