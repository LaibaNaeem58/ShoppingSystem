using ShoppingSystem.Models;
using ShoppingSystem.Services;

namespace ShoppingSystem.UI
{
    public class AdminPanel
    {
        private readonly User _admin;
        private readonly ProductService _ps = new();
        private readonly AuthService _as = new();
        private readonly OrderService _os = new();
        private readonly ReviewService _rs = new();
        private readonly PaymentService _pay = new();

        public AdminPanel(User admin) => _admin = admin;

        public void Show()
        {
            while (true)
            {
                D.Header($"Admin Panel  —  {_admin.Name}");
                D.Menu("ADMIN MENU", new[]
                {
                    "View All Products",
                    "Add New Product",
                    "Update Product",
                    "Delete Product",
                    "Low Stock Alerts",
                    "View All Orders",
                    "View All Payments",
                    "View All Reviews",
                    "Delete a Review",
                    "View All Customers",
                    "Dashboard & Reports",
                    "Logout"
                });
                string c = D.Ask("Select option");
                switch (c)
                {
                    case "1": ViewProducts(); break;
                    case "2": AddProduct(); break;
                    case "3": UpdateProduct(); break;
                    case "4": DeleteProduct(); break;
                    case "5": LowStock(); break;
                    case "6": ViewOrders(); break;
                    case "7": ViewPayments(); break;
                    case "8": ViewReviews(); break;
                    case "9": DeleteReview(); break;
                    case "10": ViewCustomers(); break;
                    case "11": Dashboard(); break;
                    case "12": case "0": return;
                    default: D.Err("Invalid option."); Thread.Sleep(500); break;
                }
            }
        }

        // ── 1. View Products ─────────────────────────────────────────
        private void ViewProducts()
        {
            D.Header("All Products");
            var list = _ps.GetAll();
            if (!list.Any()) { D.Warn("No products in catalog."); D.Pause(); return; }
            D.TableHeader(
                $"  {"ID",-5} {"Name",-24} {"Category",-13} {"Price (Rs.)",11} {"Stock",6} {"Rating",14}");
            foreach (var p in list)
            {
                string stock = p.Stock.ToString();
                string rating = p.ReviewCount > 0
                    ? $"{p.AverageRating:F1}★({p.ReviewCount})" : "-";
                string row = $"{p.Id,-5} {p.Name,-24} {p.Category,-13} {p.Price,11:N0} {stock,6} {rating,14}";
                if (p.Stock <= 5) D.RowRed(row);
                else if (p.Stock <= 10) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("  " + row); Console.ResetColor(); }
                else D.Row(row);
            }
            D.Sep();
            D.Info($"Total: {list.Count} product(s)");
            D.Pause();
        }

        // ── 2. Add Product ───────────────────────────────────────────
        private void AddProduct()
        {
            D.Header("Add New Product");
            string name = D.Ask("Product Name");
            if (string.IsNullOrEmpty(name)) { D.Err("Name is required."); D.Pause(); return; }
            string cat = D.Ask("Category (e.g. Electronics, Clothing, Home, Footwear)");
            string desc = D.Ask("Description");
            if (!double.TryParse(D.Ask("Price (Rs.)"), out double price) || price <= 0)
            { D.Err("Invalid price."); D.Pause(); return; }
            if (!int.TryParse(D.Ask("Stock Quantity"), out int stock) || stock < 0)
            { D.Err("Invalid stock."); D.Pause(); return; }

            if (_ps.Add(name, cat, desc, price, stock))
            { D.Ok($"Product '{name}' added to catalog."); D.Notify("Catalog updated."); }
            else D.Err("Failed to add product.");
            D.Pause();
        }

        // ── 3. Update Product ────────────────────────────────────────
        private void UpdateProduct()
        {
            D.Header("Update Product");
            ViewProducts();
            if (!int.TryParse(D.Ask("Enter Product ID to update"), out int id))
            { D.Err("Invalid ID."); D.Pause(); return; }
            var p = _ps.GetById(id);
            if (p == null) { D.Err("Product not found."); D.Pause(); return; }

            D.Info($"Current: {p.Name} | {p.Category} | Rs.{p.Price:N0} | Stock:{p.Stock}");
            D.Info("Press Enter to keep current value.");

            string name = D.Ask($"Name [{p.Name}]");
            if (string.IsNullOrEmpty(name)) name = p.Name;
            string cat = D.Ask($"Category [{p.Category}]");
            if (string.IsNullOrEmpty(cat)) cat = p.Category;
            string desc = D.Ask($"Description [{p.Description}]");
            if (string.IsNullOrEmpty(desc)) desc = p.Description;
            string ps2 = D.Ask($"Price [{p.Price:N0}]");
            double price = string.IsNullOrEmpty(ps2) ? p.Price : double.Parse(ps2);
            string ss = D.Ask($"Stock [{p.Stock}]");
            int newStock = string.IsNullOrEmpty(ss) ? p.Stock : int.Parse(ss);

            if (_ps.Update(id, name, cat, desc, price, newStock)) D.Ok("Product updated successfully.");
            else D.Err("Update failed.");
            D.Pause();
        }

        // ── 4. Delete Product ────────────────────────────────────────
        private void DeleteProduct()
        {
            D.Header("Delete Product");
            ViewProducts();
            if (!int.TryParse(D.Ask("Enter Product ID to delete"), out int id))
            { D.Err("Invalid ID."); D.Pause(); return; }
            var p = _ps.GetById(id);
            if (p == null) { D.Err("Product not found."); D.Pause(); return; }
            D.Warn($"Delete '{p.Name}'? This cannot be undone.");
            if (D.Ask("Type 'yes' to confirm").ToLower() == "yes")
            { if (_ps.Delete(id)) D.Ok("Product deleted."); else D.Err("Delete failed."); }
            else D.Info("Delete cancelled.");
            D.Pause();
        }

        // ── 5. Low Stock ─────────────────────────────────────────────
        private void LowStock()
        {
            D.Header("Low Stock Alerts  (≤ 10 units)");
            var low = _ps.GetLowStock();
            if (!low.Any()) { D.Ok("All products are well-stocked."); D.Pause(); return; }
            D.TableHeader($"  {"ID",-5} {"Product Name",-28} {"Category",-14} Stock");
            foreach (var p in low)
            {
                string row = $"{p.Id,-5} {p.Name,-28} {p.Category,-14} {p.Stock}";
                if (p.Stock <= 5) D.RowRed(row);
                else { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("  " + row); Console.ResetColor(); }
            }
            D.Sep();
            D.Warn($"{low.Count} product(s) need restocking.");
            D.Pause();
        }

        // ── 6. View All Orders ───────────────────────────────────────
        private void ViewOrders()
        {
            D.Header("All Orders");
            var orders = _os.GetAll();
            if (!orders.Any()) { D.Warn("No orders placed yet."); D.Pause(); return; }
            D.TableHeader(
                $"  {"ID",-5} {"Customer",-18} {"Total (Rs.)",12} {"Payment",-20} {"Delivery",-10} {"Status",-12} Date");
            foreach (var o in orders)
                D.Row($"{o.Id,-5} {o.CustomerName,-18} {o.TotalAmount,12:N0} {o.PaymentMethod,-20} {o.DeliveryType,-10} {o.Status,-12} {o.OrderDate}");
            D.Sep();
            D.Info($"Orders: {orders.Count}  |  Revenue: Rs. {orders.Sum(x => x.TotalAmount):N0}");
            D.Pause();
        }

        // ── 7. View All Payments ─────────────────────────────────────
        private void ViewPayments()
        {
            D.Header("All Payments");
            var pays = _pay.GetAll();
            if (!pays.Any()) { D.Warn("No payment records."); D.Pause(); return; }
            D.TableHeader(
                $"  {"ID",-5} {"Order#",-7} {"Amount (Rs.)",14} {"Method",-22} {"Status",-10} {"Transaction ID",-16} Date");
            foreach (var p in pays)
            {
                string row = $"{p.Id,-5} {p.OrderId,-7} {p.Amount,14:N0} {p.Method,-22} {p.Status,-10} {p.TransactionId,-16} {p.PaymentDate}";
                if (p.Status == "Success") D.RowGreen(row);
                else D.RowRed(row);
            }
            D.Sep();
            D.Info($"Total Collected: Rs. {pays.Where(x => x.Status == "Success").Sum(x => x.Amount):N0}");
            D.Pause();
        }

        // ── 8. View All Reviews ──────────────────────────────────────
        private void ViewReviews()
        {
            D.Header("All Customer Reviews");
            var revs = _rs.GetAll();
            if (!revs.Any()) { D.Warn("No reviews yet."); D.Pause(); return; }
            foreach (var r in revs)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"  [#{r.Id}]  {r.ProductName,-28}  {r.CustomerName,-20}  ");
                Console.ResetColor();
                D.Stars(r.Rating);
                D.Row($"       \"{r.Comment}\"");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"       {r.ReviewDate}");
                Console.ResetColor();
                D.Sep('·', 52);
            }
            D.Info($"Total Reviews: {revs.Count}");
            D.Pause();
        }

        // ── 9. Delete Review ─────────────────────────────────────────
        private void DeleteReview()
        {
            D.Header("Delete a Review");
            ViewReviews();
            if (!int.TryParse(D.Ask("Enter Review ID to delete"), out int rid))
            { D.Err("Invalid ID."); D.Pause(); return; }
            if (D.Ask("Confirm delete? (yes/no)").ToLower() == "yes")
            { if (_rs.Delete(rid)) D.Ok("Review deleted."); else D.Err("Not found."); }
            else D.Info("Cancelled.");
            D.Pause();
        }

        // ── 10. View Customers ───────────────────────────────────────
        private void ViewCustomers()
        {
            D.Header("All Registered Customers");
            var custs = _as.GetAllCustomers();
            if (!custs.Any()) { D.Warn("No customers registered yet."); D.Pause(); return; }
            D.TableHeader(
                $"  {"ID",-5} {"Name",-20} {"Email",-28} {"Phone",-16} Joined");
            foreach (var u in custs)
                D.Row($"{u.Id,-5} {u.Name,-20} {u.Email,-28} {u.Phone,-16} {u.CreatedAt}");
            D.Sep();
            D.Info($"Total Customers: {custs.Count}");
            D.Pause();
        }

        // ── 11. Dashboard ────────────────────────────────────────────
        private void Dashboard()
        {
            D.Header("Dashboard & Reports");
            int totalOrders = _os.GetTotalOrders();
            double totalSales = _os.GetTotalSales();
            int totalCusts = _as.GetTotalCustomers();
            var allProducts = _ps.GetAll();
            int lowStock = _ps.GetLowStock().Count;
            int totalReviews = _rs.GetAll().Count;
            var pays = _pay.GetAll();

            D.Sep('═', 54);
            D.RowGreen($"  Total Revenue          : Rs. {totalSales:N0}");
            D.Row($"  Total Orders           : {totalOrders}");
            D.Row($"  Total Customers        : {totalCusts}");
            D.Row($"  Total Products         : {allProducts.Count}");
            D.Row($"  Low-Stock Products     : {lowStock}");
            D.Row($"  Total Reviews          : {totalReviews}");
            D.Row($"  Total Payments         : {pays.Count}");
            D.Sep('═', 54);

            D.SubHeader("Payment Method Breakdown");
            var grouped = pays.GroupBy(p => p.Method);
            foreach (var g in grouped)
                D.Row($"  {g.Key,-26} {g.Count(),4} payments   Rs. {g.Sum(x => x.Amount):N0}");

            D.Sep();
            D.SubHeader("Top Categories by Products");
            var cats = allProducts.GroupBy(p => p.Category)
                .Select(g => new { Cat = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count);
            foreach (var cat in cats)
                D.Row($"  {cat.Cat,-26} {cat.Count} product(s)");

            D.Pause();
        }
    }
}