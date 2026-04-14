using Microsoft.Data.Sqlite;
using ShoppingSystem.Database;
using ShoppingSystem.Models;

namespace ShoppingSystem.Services
{
    public class ProductService
    {
        private const string SelectSql = @"
            SELECT p.Id, p.Name, p.Category, p.Description, p.Price, p.Stock,
                   COALESCE(AVG(r.Rating),0), COUNT(r.Id)
            FROM Products p
            LEFT JOIN Reviews r ON p.Id = r.ProductId";

        public List<Product> GetAll(string? category = null)
        {
            var list = new List<Product>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = SelectSql
                + (category != null ? " WHERE p.Category=$c" : "")
                + " GROUP BY p.Id ORDER BY p.Id";
            if (category != null) cmd.Parameters.AddWithValue("$c", category);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(Map(r));
            return list;
        }

        public Product? GetById(int id)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = SelectSql + " WHERE p.Id=$id GROUP BY p.Id";
            cmd.Parameters.AddWithValue("$id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? Map(r) : null;
        }

        private static Product Map(SqliteDataReader r) => new()
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
            Category = r.GetString(2),
            Description = r.IsDBNull(3) ? "" : r.GetString(3),
            Price = r.GetDouble(4),
            Stock = r.GetInt32(5),
            AverageRating = Math.Round(r.GetDouble(6), 1),
            ReviewCount = r.GetInt32(7)
        };

        public bool Add(string name, string category, string desc, double price, int stock)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO Products(Name,Category,Description,Price,Stock) VALUES($n,$c,$d,$p,$s)";
            cmd.Parameters.AddWithValue("$n", name);
            cmd.Parameters.AddWithValue("$c", category);
            cmd.Parameters.AddWithValue("$d", desc);
            cmd.Parameters.AddWithValue("$p", price);
            cmd.Parameters.AddWithValue("$s", stock);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Update(int id, string name, string category, string desc, double price, int stock)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "UPDATE Products SET Name=$n,Category=$c,Description=$d,Price=$p,Stock=$s WHERE Id=$id";
            cmd.Parameters.AddWithValue("$n", name);
            cmd.Parameters.AddWithValue("$c", category);
            cmd.Parameters.AddWithValue("$d", desc);
            cmd.Parameters.AddWithValue("$p", price);
            cmd.Parameters.AddWithValue("$s", stock);
            cmd.Parameters.AddWithValue("$id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Products WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ReduceStock(int productId, int qty)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "UPDATE Products SET Stock=Stock-$q WHERE Id=$id AND Stock>=$q";
            cmd.Parameters.AddWithValue("$q", qty);
            cmd.Parameters.AddWithValue("$id", productId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<Product> GetLowStock(int threshold = 10)
            => GetAll().Where(p => p.Stock <= threshold).ToList();

        public List<string> GetCategories()
        {
            var list = new List<string>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT DISTINCT Category FROM Products ORDER BY Category";
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(r.GetString(0));
            return list;
        }
    }
}