using Microsoft.Data.Sqlite;
using ShoppingSystem.Database;
using ShoppingSystem.Models;

namespace ShoppingSystem.Services
{
    public class OrderService
    {
        public Order? PlaceOrder(
            int userId, string customerName, List<CartItem> items,
            string paymentMethod, string deliveryType, string deliveryAddress)
        {
            double sub = items.Sum(i => i.ProductPrice * i.Quantity);
            double fee = deliveryType == "Express" ? 300 : 0;
            double total = sub + fee;
            string eta = deliveryType == "Express"
                ? DateTime.Now.AddDays(2).ToString("dd MMM yyyy")
                : DateTime.Now.AddDays(5).ToString("dd MMM yyyy");

            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();
            try
            {
                var oc = conn.CreateCommand(); oc.Transaction = tx;
                oc.CommandText = @"INSERT INTO Orders(UserId,CustomerName,SubTotal,DeliveryFee,
                    TotalAmount,PaymentMethod,PaymentStatus,DeliveryType,DeliveryAddress,
                    Status,OrderDate,EstimatedDelivery)
                    VALUES($u,$cn,$st,$df,$ta,$pm,'Paid',$dt,$da,'Confirmed',
                    datetime('now'),$ed); SELECT last_insert_rowid();";
                oc.Parameters.AddWithValue("$u", userId);
                oc.Parameters.AddWithValue("$cn", customerName);
                oc.Parameters.AddWithValue("$st", sub);
                oc.Parameters.AddWithValue("$df", fee);
                oc.Parameters.AddWithValue("$ta", total);
                oc.Parameters.AddWithValue("$pm", paymentMethod);
                oc.Parameters.AddWithValue("$dt", deliveryType);
                oc.Parameters.AddWithValue("$da", deliveryAddress);
                oc.Parameters.AddWithValue("$ed", eta);
                int orderId = (int)(long)(oc.ExecuteScalar() ?? 0L);

                foreach (var item in items)
                {
                    var ic = conn.CreateCommand(); ic.Transaction = tx;
                    ic.CommandText = @"INSERT INTO OrderItems(OrderId,ProductId,ProductName,
                        Quantity,UnitPrice,SubTotal)
                        VALUES($oid,$pid,$pn,$q,$up,$s)";
                    ic.Parameters.AddWithValue("$oid", orderId);
                    ic.Parameters.AddWithValue("$pid", item.ProductId);
                    ic.Parameters.AddWithValue("$pn", item.ProductName);
                    ic.Parameters.AddWithValue("$q", item.Quantity);
                    ic.Parameters.AddWithValue("$up", item.ProductPrice);
                    ic.Parameters.AddWithValue("$s", item.ProductPrice * item.Quantity);
                    ic.ExecuteNonQuery();
                }

                string txId = "TXN" + DateTime.Now.Ticks.ToString()[^10..];
                var pc = conn.CreateCommand(); pc.Transaction = tx;
                pc.CommandText = @"INSERT INTO Payments(OrderId,UserId,Amount,Method,
                    Status,TransactionId,PaymentDate)
                    VALUES($oid,$uid,$amt,$mth,'Success',$txid,datetime('now'))";
                pc.Parameters.AddWithValue("$oid", orderId);
                pc.Parameters.AddWithValue("$uid", userId);
                pc.Parameters.AddWithValue("$amt", total);
                pc.Parameters.AddWithValue("$mth", paymentMethod);
                pc.Parameters.AddWithValue("$txid", txId);
                pc.ExecuteNonQuery();

                tx.Commit();
                return new Order
                {
                    Id = orderId,
                    UserId = userId,
                    CustomerName = customerName,
                    SubTotal = sub,
                    DeliveryFee = fee,
                    TotalAmount = total,
                    PaymentMethod = paymentMethod,
                    DeliveryType = deliveryType,
                    DeliveryAddress = deliveryAddress,
                    Status = "Confirmed",
                    OrderDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    EstimatedDelivery = eta
                };
            }
            catch { tx.Rollback(); return null; }
        }

        public List<Order> GetByUser(int userId) => Fetch("WHERE UserId=$u", userId);
        public List<Order> GetAll() => Fetch(null, null);
        public double GetTotalSales() { using var c = Open(); var cmd = c.CreateCommand(); cmd.CommandText = "SELECT COALESCE(SUM(TotalAmount),0) FROM Orders"; return Convert.ToDouble(cmd.ExecuteScalar()); }
        public int GetTotalOrders() { using var c = Open(); var cmd = c.CreateCommand(); cmd.CommandText = "SELECT COUNT(*) FROM Orders"; return (int)(long)(cmd.ExecuteScalar() ?? 0L); }

        private SqliteConnection Open() { var c = new SqliteConnection(DatabaseManager.ConnectionString); c.Open(); return c; }

        private List<Order> Fetch(string? where, int? uid)
        {
            var list = new List<Order>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id,UserId,CustomerName,SubTotal,DeliveryFee,TotalAmount,
                PaymentMethod,PaymentStatus,DeliveryType,DeliveryAddress,Status,
                OrderDate,EstimatedDelivery FROM Orders"
                + (where != null ? " " + where : "") + " ORDER BY Id DESC";
            if (uid.HasValue) cmd.Parameters.AddWithValue("$u", uid.Value);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(new Order
            {
                Id = r.GetInt32(0),
                UserId = r.GetInt32(1),
                CustomerName = r.GetString(2),
                SubTotal = r.GetDouble(3),
                DeliveryFee = r.GetDouble(4),
                TotalAmount = r.GetDouble(5),
                PaymentMethod = r.GetString(6),
                PaymentStatus = r.GetString(7),
                DeliveryType = r.GetString(8),
                DeliveryAddress = r.IsDBNull(9) ? "" : r.GetString(9),
                Status = r.GetString(10),
                OrderDate = r.GetString(11),
                EstimatedDelivery = r.IsDBNull(12) ? "" : r.GetString(12)
            });
            return list;
        }

        public List<OrderItem> GetOrderItems(int orderId)
        {
            var list = new List<OrderItem>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id,OrderId,ProductId,ProductName,Quantity,UnitPrice,SubTotal
                FROM OrderItems WHERE OrderId=$id";
            cmd.Parameters.AddWithValue("$id", orderId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new OrderItem
                {
                    Id = r.GetInt32(0),
                    OrderId = r.GetInt32(1),
                    ProductId = r.GetInt32(2),
                    ProductName = r.GetString(3),
                    Quantity = r.GetInt32(4),
                    UnitPrice = r.GetDouble(5),
                    SubTotal = r.GetDouble(6)
                });
            return list;
        }
    }
}