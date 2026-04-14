using Microsoft.Data.Sqlite;
using ShoppingSystem.Database;
using ShoppingSystem.Models;

namespace ShoppingSystem.Services
{
    public class PaymentService
    {
        public List<Payment> GetByUser(int userId) => Fetch("WHERE UserId=$u", userId);
        public List<Payment> GetAll() => Fetch(null, null);

        private List<Payment> Fetch(string? where, int? uid)
        {
            var list = new List<Payment>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT Id,OrderId,UserId,Amount,Method,
                Status,TransactionId,PaymentDate FROM Payments"
                + (where != null ? " " + where : "") + " ORDER BY Id DESC";
            if (uid.HasValue) cmd.Parameters.AddWithValue("$u", uid.Value);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new Payment
                {
                    Id = r.GetInt32(0),
                    OrderId = r.GetInt32(1),
                    UserId = r.GetInt32(2),
                    Amount = r.GetDouble(3),
                    Method = r.GetString(4),
                    Status = r.GetString(5),
                    TransactionId = r.IsDBNull(6) ? "" : r.GetString(6),
                    PaymentDate = r.GetString(7)
                });
            return list;
        }
    }
}