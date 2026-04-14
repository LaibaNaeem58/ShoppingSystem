using Microsoft.Data.Sqlite;
using ShoppingSystem.Database;
using ShoppingSystem.Models;

namespace ShoppingSystem.Services
{
    public class ReviewService
    {
        public (bool ok, string msg) Add(int userId, string customerName,
            int productId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                return (false, "Rating must be 1-5.");
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var chk = conn.CreateCommand();
            chk.CommandText = "SELECT COUNT(*) FROM Reviews WHERE UserId=$u AND ProductId=$p";
            chk.Parameters.AddWithValue("$u", userId);
            chk.Parameters.AddWithValue("$p", productId);
            if ((long)(chk.ExecuteScalar() ?? 0L) > 0)
                return (false, "You already reviewed this product.");
            var pc = conn.CreateCommand();
            pc.CommandText = "SELECT COUNT(*) FROM Products WHERE Id=$id";
            pc.Parameters.AddWithValue("$id", productId);
            if ((long)(pc.ExecuteScalar() ?? 0L) == 0)
                return (false, "Product not found.");
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Reviews(ProductId,UserId,CustomerName,
                Rating,Comment,ReviewDate)
                VALUES($pid,$uid,$cn,$r,$c,datetime('now'))";
            cmd.Parameters.AddWithValue("$pid", productId);
            cmd.Parameters.AddWithValue("$uid", userId);
            cmd.Parameters.AddWithValue("$cn", customerName);
            cmd.Parameters.AddWithValue("$r", rating);
            cmd.Parameters.AddWithValue("$c", comment.Trim());
            cmd.ExecuteNonQuery();
            return (true, "Review submitted! Thank you.");
        }

        public List<Review> GetByProduct(int productId)
            => Fetch("WHERE r.ProductId=$pid", productId);

        public List<Review> GetAll() => Fetch(null, null);

        private List<Review> Fetch(string? where, int? param)
        {
            var list = new List<Review>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT r.Id,r.ProductId,r.UserId,r.CustomerName,
                p.Name,r.Rating,r.Comment,r.ReviewDate
                FROM Reviews r JOIN Products p ON r.ProductId=p.Id"
                + (where != null ? " " + where : "") + " ORDER BY r.Id DESC";
            if (param.HasValue) cmd.Parameters.AddWithValue("$pid", param.Value);
            using var rd = cmd.ExecuteReader();
            while (rd.Read())
                list.Add(new Review
                {
                    Id = rd.GetInt32(0),
                    ProductId = rd.GetInt32(1),
                    UserId = rd.GetInt32(2),
                    CustomerName = rd.GetString(3),
                    ProductName = rd.GetString(4),
                    Rating = rd.GetInt32(5),
                    Comment = rd.IsDBNull(6) ? "" : rd.GetString(6),
                    ReviewDate = rd.GetString(7)
                });
            return list;
        }

        public bool Delete(int reviewId)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Reviews WHERE Id=$id";
            cmd.Parameters.AddWithValue("$id", reviewId);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}