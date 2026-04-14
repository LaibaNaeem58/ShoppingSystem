using ShoppingSystem.Models;
using ShoppingSystem.Services;

namespace ShoppingSystem.UI
{
    public class CustomerPanel
    {
        private readonly User _u;
        private readonly ProductService _ps = new();
        private readonly CartService _cs = new();
        private readonly OrderService _os = new();
        private readonly ReviewService _rs = new();
        private readonly PaymentService _pay = new();

        public CustomerPanel(User user) => _u = user;

        public void Show()
        {
            while (true)
            {
                D.Header($"Customer Panel  —  {_u.Name}");
                D.Menu("CUSTOMER MENU", new[]
                {
                    "View All Products",
                    "Search Products by Category",
                    "View Product Details & Reviews",
                    "Add Product to Cart",
                    "View My Cart",
                    "Update Cart (Increase / Decrease / Remove)",
                    "Checkout & Place Order",
                    "My Order History",
                    "My Payment History",
                    "Write a Product Review",
                    "Logout"
                });
                string c = D.Ask("Select option");
                switch (c)
                {
                    case "1": ViewProducts(); break;
                    case "2": SearchByCategory(); break;
                    case "3": ProductDetails(); break;
                    case "4": AddToCart(); break;
                    case "5": ViewCart(); break;
                    case "6": UpdateCart(); break;
                    case "7": Checkout(); break;
                    case "8": OrderHistory(); break;
                    case "9": PaymentHistory(); break;
                    case "10": WriteReview(); break;
                    case "11": case "0": return;
                    default: D.Err("Invalid option."); Thread.Sleep(500); break;
                }
            }
        }

        // ────────────────────────────────────────────────────────────
        // 1. View Products
        // ────────────────────────────────────────────────────────────
        private void ViewProducts(List<Product>? overrideList = null)
        {
            D.Header("Product Catalog");
            var list = overrideList ?? _ps.GetAll();
            if (!list.Any()) { D.Warn("No products available."); D.Pause(); return; }
            PrintProductTable(list);
            D.Pause();
        }

        private void PrintProductTable(List<Product> list)
        {
            D.TableHeader(
                $"  {"ID",-5} {"Product Name",-26} {"Category",-13} {"Price (Rs.)",11} {"Stock",6}  {"Rating",12}");
            foreach (var p in list)
            {
                string stock = p.Stock > 0 ? p.Stock.ToString() : "OUT";
                string rating = p.ReviewCount > 0
                    ? $"{p.AverageRating:F1}★ ({p.ReviewCount})"
                    : "No reviews";
                string row = $"  {p.Id,-5} {p.Name,-26} {p.Category,-13} {p.Price,11:N0} {stock,6}  {rating,12}";
                if (p.Stock == 0) D.RowRed(row[2..]);
                else D.Row(row[2..]);
            }
            D.Sep();
        }

        // ────────────────────────────────────────────────────────────
        // 2. Search by Category
        // ────────────────────────────────────────────────────────────
        private void SearchByCategory()
        {
            D.Header("Search by Category");
            var cats = _ps.GetCategories();
            D.TableHeader("Available Categories:");
            for (int i = 0; i < cats.Count; i++)
                D.Row($"[{i + 1}] {cats[i]}");
            D.Sep();
            string s = D.Ask("Select number");
            if (int.TryParse(s, out int n) && n >= 1 && n <= cats.Count)
                ViewProducts(_ps.GetAll(cats[n - 1]));
            else { D.Err("Invalid selection."); D.Pause(); }
        }

        // ────────────────────────────────────────────────────────────
        // 3. Product Details & Reviews
        // ────────────────────────────────────────────────────────────
        private void ProductDetails()
        {
            D.Header("Product Details");
            PrintProductTable(_ps.GetAll());
            if (!int.TryParse(D.Ask("Enter Product ID"), out int id))
            { D.Err("Invalid ID."); D.Pause(); return; }
            var p = _ps.GetById(id);
            if (p == null) { D.Err("Product not found."); D.Pause(); return; }

            D.Header($"  {p.Name}");
            D.Row($"Category    : {p.Category}");
            D.Row($"Description : {p.Description}");
            D.Row($"Price       : Rs. {p.Price:N0}");
            D.Row($"Stock       : {(p.Stock > 0 ? p.Stock.ToString() + " units" : "OUT OF STOCK")}");
            Console.Write("  Rating      : ");
            if (p.ReviewCount > 0) D.Stars((int)Math.Round(p.AverageRating));
            else Console.WriteLine("No reviews yet.");
            D.Sep();

            var reviews = _rs.GetByProduct(id);
            if (!reviews.Any()) { D.Info("No customer reviews for this product yet."); }
            else
            {
                D.SubHeader($"Customer Reviews ({reviews.Count})");
                foreach (var rv in reviews)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"  {rv.CustomerName,-22} ");
                    Console.ResetColor();
                    D.Stars(rv.Rating);
                    D.Row($"  \"{rv.Comment}\"");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  {rv.ReviewDate}");
                    Console.ResetColor();
                    D.Sep('·', 50);
                }
            }
            D.Pause();
        }

        // ────────────────────────────────────────────────────────────
        // 4. Add to Cart
        // ────────────────────────────────────────────────────────────
        private void AddToCart()
        {
            D.Header("Add Product to Cart");
            PrintProductTable(_ps.GetAll());
            if (!int.TryParse(D.Ask("Enter Product ID"), out int id))
            { D.Err("Invalid ID."); D.Pause(); return; }
            var p = _ps.GetById(id);
            if (p == null) { D.Err("Product not found."); D.Pause(); return; }
            if (p.Stock == 0) { D.Err($"'{p.Name}' is out of stock."); D.Pause(); return; }

            D.Info($"Selected: {p.Name}  —  Rs. {p.Price:N0}  (Available: {p.Stock})");
            if (!int.TryParse(D.Ask("Quantity"), out int qty) || qty <= 0)
            { D.Err("Invalid quantity."); D.Pause(); return; }

            string result = _cs.Add(_u.Id, id, qty);
            if (result == "OK")
            { D.Ok($"{qty}x '{p.Name}' added to your cart."); D.Notify("Item added to cart!"); }
            else D.Err(result);
            D.Pause();
        }

        // ────────────────────────────────────────────────────────────
        // 5. View Cart
        // ────────────────────────────────────────────────────────────
        private void ViewCart()
        {
            D.Header("My Shopping Cart");
            var items = _cs.GetCart(_u.Id);
            if (!items.Any()) { D.Warn("Your cart is empty."); D.Pause(); return; }
            PrintCartTable(items);
            D.Pause();
        }

        private void PrintCartTable(List<CartItem> items)
        {
            D.TableHeader($"  {"Cart ID",-8} {"Product",-26} {"Unit Price",11} {"Qty",5} {"Subtotal",12}");
            double total = 0;
            foreach (var i in items)
            {
                double sub = i.ProductPrice * i.Quantity;
                total += sub;
                D.Row($"{i.Id,-8} {i.ProductName,-26} {i.ProductPrice,11:N0} {i.Quantity,5} {sub,12:N0}");
            }
            D.Sep();
            D.RowGreen($"{"CART TOTAL",-54} Rs. {total:N0}");
            Console.WriteLine();
        }

        // ────────────────────────────────────────────────────────────
        // 6. Update Cart  (increase / decrease / remove)
        // ────────────────────────────────────────────────────────────
        private void UpdateCart()
        {
            D.Header("Update My Cart");
            var items = _cs.GetCart(_u.Id);
            if (!items.Any()) { D.Warn("Your cart is empty."); D.Pause(); return; }
            PrintCartTable(items);

            D.Menu("CART OPTIONS", new[]
            {
                "Change Item Quantity (increase or decrease)",
                "Remove an Item Completely"
            });
            string c = D.Ask("Select");

            if (c == "1")
            {
                if (!int.TryParse(D.Ask("Enter Cart ID"), out int cid))
                { D.Err("Invalid Cart ID."); D.Pause(); return; }

                var item = items.FirstOrDefault(x => x.Id == cid);
                if (item == null) { D.Err("Cart item not found."); D.Pause(); return; }
                D.Info($"Current quantity of '{item.ProductName}': {item.Quantity}");
                D.Info("Enter new quantity (enter 0 to remove):");

                if (!int.TryParse(D.Ask("New Quantity"), out int newQty) || newQty < 0)
                { D.Err("Invalid quantity."); D.Pause(); return; }

                if (_cs.UpdateQty(cid, _u.Id, newQty))
                    D.Ok(newQty == 0 ? "Item removed from cart." : $"Quantity updated to {newQty}.");
                else D.Err("Update failed.");
            }
            else if (c == "2")
            {
                if (!int.TryParse(D.Ask("Enter Cart ID to remove"), out int cid))
                { D.Err("Invalid Cart ID."); D.Pause(); return; }
                if (_cs.Remove(cid, _u.Id)) D.Ok("Item removed from cart.");
                else D.Err("Remove failed.");
            }
            D.Pause();
        }

        // ────────────────────────────────────────────────────────────
        // 7. Checkout
        // ────────────────────────────────────────────────────────────
        private void Checkout()
        {
            D.Header("Checkout");
            var items = _cs.GetCart(_u.Id);
            if (!items.Any()) { D.Warn("Your cart is empty. Add items first."); D.Pause(); return; }

            // ── Show cart summary ────────────────────────────────────
            D.SubHeader("Your Order Summary");
            PrintCartTable(items);
            double subTotal = items.Sum(i => i.ProductPrice * i.Quantity);

            // ── Delivery Details ─────────────────────────────────────
            D.SubHeader("DELIVERY DETAILS");
            string fullName = D.Ask("Full Name");
            string phone = D.Ask("Mobile Number");
            string street = D.Ask("Street Address");
            string city = D.Ask("City");
            string delivAddr = $"{street}, {city}";

            // ── Delivery Type ────────────────────────────────────────
            Console.Clear();
            D.Header("Select Delivery Type");
            D.Row("  [1]  Standard Delivery   (3-5 business days)  FREE");
            D.Row("  [2]  Express Delivery    (1-2 business days)  Rs. 300");
            D.Sep();
            string dt = D.Ask("Select (1-2)");
            string delType = dt == "2" ? "Express" : "Standard";
            double fee = delType == "Express" ? 300 : 0;
            double total = subTotal + fee;

            // ── Payment Gateway ──────────────────────────────────────
            Console.Clear();
            D.Header("PAYMENT GATEWAY");
            D.Row($"  Total Amount: Rs. {total:N0}");
            Console.WriteLine();
            D.Row("  Select Digital Payment Method:");
            D.Row("  [1]  EasyPaisa");
            D.Row("  [2]  JazzCash");
            D.Row("  [3]  Credit / Debit Card");
            D.Row("  [4]  Cash on Delivery (COD)");
            D.Sep();
            string pm = D.Ask("Select (1-4)");
            string payMethod = pm switch
            {
                "1" => "EasyPaisa",
                "2" => "JazzCash",
                "3" => "Credit/Debit Card",
                "4" => "Cash on Delivery",
                _ => "Cash on Delivery"
            };

            // Simulate payment input
            if (pm == "1" || pm == "2")
            {
                D.Info($"Simulating {payMethod} payment...");
                D.Ask("Registered Mobile Number");
                D.AskPass("MPIN");
                D.Ok($"{payMethod} payment verified.");
            }
            else if (pm == "3")
            {
                D.Info("Enter card details (simulation):");
                D.Ask("Card Number (16 digits)");
                D.Ask("Expiry (MM/YY)");
                D.AskPass("CVV");
                D.Ok("Card details verified.");
            }
            else
            {
                D.Info("Cash on Delivery selected. Pay when your order arrives.");
            }

            // ── Final Order Summary ──────────────────────────────────
            Console.Clear();
            D.Header("Order Confirmation");
            D.Row($"  Recipient    : {fullName}");
            D.Row($"  Mobile       : {phone}");
            D.Row($"  Address      : {delivAddr}");
            D.Sep('·', 50);
            D.Row($"  Sub-Total    : Rs. {subTotal:N0}");
            D.Row($"  Delivery Fee : Rs. {fee:N0}");
            D.RowGreen($"  TOTAL        : Rs. {total:N0}");
            D.Sep('·', 50);
            D.Row($"  Payment      : {payMethod}");
            D.Row($"  Delivery     : {delType}");
            D.Sep();

            // ── Reduce stock & place order ───────────────────────────
            foreach (var item in items)
            {
                if (!_ps.ReduceStock(item.ProductId, item.Quantity))
                { D.Err($"Insufficient stock for '{item.ProductName}'. Order aborted."); D.Pause(); return; }
            }

            var order = _os.PlaceOrder(_u.Id, fullName, items, payMethod, delType, delivAddr);
            if (order != null)
            {
                _cs.Clear(_u.Id);
                ShowOrderSuccess(order);
            }
            else
            {
                D.Err("Order could not be placed. Please try again.");
            }
            D.Pause();
        }

        private static void ShowOrderSuccess(Order o)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("  ╔══════════════════════════════════════════════════════╗");
            Console.WriteLine($"  ║          ✔  ORDER PLACED SUCCESSFULLY               ║");
            Console.WriteLine("  ╠══════════════════════════════════════════════════════╣");
            Console.WriteLine($"  ║  Order ID         : #{o.Id,-35}║");
            Console.WriteLine($"  ║  Payment Method   : {o.PaymentMethod,-35}║");
            Console.WriteLine($"  ║  Delivery Type    : {o.DeliveryType,-35}║");
            Console.WriteLine($"  ║  Estimated By     : {o.EstimatedDelivery,-35}║");
            Console.WriteLine($"  ║  Total Amount     : Rs. {o.TotalAmount,-32:N0}║");
            Console.WriteLine("  ╠══════════════════════════════════════════════════════╣");
            Console.WriteLine("  ║     Thank you for shopping with us!  🛒              ║");
            Console.WriteLine("  ╚══════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        // ────────────────────────────────────────────────────────────
        // 8. Order History
        // ────────────────────────────────────────────────────────────
        private void OrderHistory()
        {
            D.Header("My Order History");
            var orders = _os.GetByUser(_u.Id);
            if (!orders.Any()) { D.Warn("No orders found yet."); D.Pause(); return; }

            foreach (var o in orders)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n  ┌── Order #{o.Id}  ─────────────────────────────────────");
                Console.ResetColor();
                D.Row($"  Date         : {o.OrderDate}");
                D.Row($"  Payment      : {o.PaymentMethod}  [{o.PaymentStatus}]");
                D.Row($"  Delivery     : {o.DeliveryType}  → ETA: {o.EstimatedDelivery}");
                D.Row($"  Address      : {o.DeliveryAddress}");
                D.Row($"  Status       : {o.Status}");
                D.RowGreen($"  Total        : Rs. {o.TotalAmount:N0}");
                D.Sep('·', 52);

                var its = _os.GetOrderItems(o.Id);
                foreach (var it in its)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"     • {it.ProductName,-30} x{it.Quantity}  Rs.{it.UnitPrice:N0}  =  Rs.{it.SubTotal:N0}");
                    Console.ResetColor();
                }
                D.Sep();
            }
            D.Pause();
        }

        // ────────────────────────────────────────────────────────────
        // 9. Payment History
        // ────────────────────────────────────────────────────────────
        private void PaymentHistory()
        {
            D.Header("My Payment History");
            var pays = _pay.GetByUser(_u.Id);
            if (!pays.Any()) { D.Warn("No payment records found."); D.Pause(); return; }

            D.TableHeader($"  {"ID",-5} {"Order#",-8} {"Amount (Rs.)",14} {"Method",-20} {"Status",-10} {"Transaction ID",-16} Date");
            foreach (var p in pays)
            {
                string row = $"{p.Id,-5} {p.OrderId,-8} {p.Amount,14:N0} {p.Method,-20} {p.Status,-10} {p.TransactionId,-16} {p.PaymentDate}";
                if (p.Status == "Success") D.RowGreen(row);
                else D.RowRed(row);
            }
            D.Pause();
        }

        // ────────────────────────────────────────────────────────────
        // 10. Write Review
        // ────────────────────────────────────────────────────────────
        private void WriteReview()
        {
            D.Header("Write a Product Review");
            var orders = _os.GetByUser(_u.Id);
            if (!orders.Any()) { D.Warn("You have no orders. Place an order first!"); D.Pause(); return; }

            // Show only ordered products
            var purchased = orders
                .SelectMany(o => _os.GetOrderItems(o.Id))
                .Select(i => new { i.ProductId, i.ProductName })
                .DistinctBy(x => x.ProductId)
                .ToList();

            D.SubHeader("Your Purchased Products");
            foreach (var op in purchased)
                D.Row($"  ID: {op.ProductId,-5}  {op.ProductName}");
            D.Sep();

            if (!int.TryParse(D.Ask("Enter Product ID to review"), out int pid))
            { D.Err("Invalid ID."); D.Pause(); return; }

            D.Info("Rating guide:  1=Very Bad  2=Bad  3=OK  4=Good  5=Excellent");
            if (!int.TryParse(D.Ask("Your Rating (1-5)"), out int rating))
            { D.Err("Invalid rating."); D.Pause(); return; }

            D.Stars(Math.Clamp(rating, 1, 5));
            string comment = D.Ask("Your Comment");

            var (ok, msg) = _rs.Add(_u.Id, _u.Name, pid, rating, comment);
            if (ok) { D.Ok(msg); D.Notify("Review submitted! Thank you."); }
            else D.Err(msg);
            D.Pause();
        }
    }
}