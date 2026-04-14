using Microsoft.Data.Sqlite;
using ShoppingSystem.Database;
using ShoppingSystem.Models;

namespace ShoppingSystem.Services
{
    public class CartService
    {
        public string Add(int userId, int productId, int qty)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();

            var sc = conn.CreateCommand();
            sc.CommandText = "SELECT Stock,Name FROM Products WHERE Id=$id";
            sc.Parameters.AddWithValue("$id", productId);
            using var sr = sc.ExecuteReader();
            if (!sr.Read()) return "Product not found.";
            int stock = sr.GetInt32(0);
            string pname = sr.GetString(1);
            sr.Close();
            if (stock == 0) return $"'{pname}' is out of stock.";

            var ec = conn.CreateCommand();
            ec.CommandText = "SELECT Id,Quantity FROM Cart WHERE UserId=$u AND ProductId=$p";
            ec.Parameters.AddWithValue("$u", userId);
            ec.Parameters.AddWithValue("$p", productId);
            using var er = ec.ExecuteReader();
            if (er.Read())
            {
                int cartId = er.GetInt32(0);
                int existing = er.GetInt32(1);
                er.Close();
                int newQty = existing + qty;
                if (newQty > stock) return $"Only {stock} units available ({existing} already in cart).";
                var uc = conn.CreateCommand();
                uc.CommandText = "UPDATE Cart SET Quantity=$q WHERE Id=$id";
                uc.Parameters.AddWithValue("$q", newQty);
                uc.Parameters.AddWithValue("$id", cartId);
                uc.ExecuteNonQuery();
            }
            else
            {
                er.Close();
                if (qty > stock) return $"Only {stock} units available.";
                var ic = conn.CreateCommand();
                ic.CommandText = "INSERT INTO Cart(UserId,ProductId,Quantity) VALUES($u,$p,$q)";
                ic.Parameters.AddWithValue("$u", userId);
                ic.Parameters.AddWithValue("$p", productId);
                ic.Parameters.AddWithValue("$q", qty);
                ic.ExecuteNonQuery();
            }
            return "OK";
        }

        public List<CartItem> GetCart(int userId)
        {
            var list = new List<CartItem>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT c.Id,c.UserId,c.ProductId,c.Quantity,
                                       p.Name,p.Price,p.Category
                                FROM Cart c JOIN Products p ON c.ProductId=p.Id
                                WHERE c.UserId=$u ORDER BY c.Id";
            cmd.Parameters.AddWithValue("$u", userId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new CartItem
                {
                    Id = r.GetInt32(0),
                    UserId = r.GetInt32(1),
                    ProductId = r.GetInt32(2),
                    Quantity = r.GetInt32(3),
                    ProductName = r.GetString(4),
                    ProductPrice = r.GetDouble(5),
                    Category = r.GetString(6)
                });
            return list;
        }

        public bool Remove(int cartId, int userId)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Cart WHERE Id=$id AND UserId=$u";
            cmd.Parameters.AddWithValue("$id", cartId);
            cmd.Parameters.AddWithValue("$u", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateQty(int cartId, int userId, int newQty)
        {
            if (newQty <= 0) return Remove(cartId, userId);
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Cart SET Quantity=$q WHERE Id=$id AND UserId=$u";
            cmd.Parameters.AddWithValue("$q", newQty);
            cmd.Parameters.AddWithValue("$id", cartId);
            cmd.Parameters.AddWithValue("$u", userId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public void Clear(int userId)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Cart WHERE UserId=$u";
            cmd.Parameters.AddWithValue("$u", userId);
            cmd.ExecuteNonQuery();
        }
    }
}