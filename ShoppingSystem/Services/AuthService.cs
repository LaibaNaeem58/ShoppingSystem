using Microsoft.Data.Sqlite;
using ShoppingSystem.Database;
using ShoppingSystem.Models;

namespace ShoppingSystem.Services
{
    public class AuthService
    {
        public (bool ok, string msg, User? user) Register(
            string name, string email, string password, string phone, string address)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return (false, "Name, email and password are required.", null);

            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var chk = conn.CreateCommand();
            chk.CommandText = "SELECT COUNT(*) FROM Users WHERE Email=$e";
            chk.Parameters.AddWithValue("$e", email.Trim().ToLower());
            if ((long)(chk.ExecuteScalar() ?? 0L) > 0)
                return (false, "Email already registered.", null);

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Users(Name,Email,Password,Role,Phone,Address,CreatedAt)
                VALUES($n,$e,$p,'Customer',$ph,$a,datetime('now'));
                SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$n", name.Trim());
            cmd.Parameters.AddWithValue("$e", email.Trim().ToLower());
            cmd.Parameters.AddWithValue("$p", password);
            cmd.Parameters.AddWithValue("$ph", phone.Trim());
            cmd.Parameters.AddWithValue("$a", address.Trim());
            int id = (int)(long)(cmd.ExecuteScalar() ?? 0L);
            return (true, "Registration successful!", new User
            {
                Id = id,
                Name = name.Trim(),
                Email = email.Trim().ToLower(),
                Role = "Customer",
                Phone = phone,
                Address = address
            });
        }

        public (bool ok, string msg, User? user) Login(string email, string password)
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id,Name,Email,Role,Phone,Address FROM Users WHERE Email=$e AND Password=$p";
            cmd.Parameters.AddWithValue("$e", email.Trim().ToLower());
            cmd.Parameters.AddWithValue("$p", password);
            using var r = cmd.ExecuteReader();
            if (r.Read())
                return (true, "Login successful.", new User
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1),
                    Email = r.GetString(2),
                    Role = r.GetString(3),
                    Phone = r.IsDBNull(4) ? "" : r.GetString(4),
                    Address = r.IsDBNull(5) ? "" : r.GetString(5)
                });
            return (false, "Invalid email or password.", null);
        }

        public int GetTotalCustomers()
        {
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Role='Customer'";
            return (int)(long)(cmd.ExecuteScalar() ?? 0L);
        }

        public List<User> GetAllCustomers()
        {
            var list = new List<User>();
            using var conn = new SqliteConnection(DatabaseManager.ConnectionString);
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id,Name,Email,Phone,Address,CreatedAt FROM Users WHERE Role='Customer' ORDER BY Id";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new User
                {
                    Id = r.GetInt32(0),
                    Name = r.GetString(1),
                    Email = r.GetString(2),
                    Phone = r.IsDBNull(3) ? "" : r.GetString(3),
                    Address = r.IsDBNull(4) ? "" : r.GetString(4),
                    CreatedAt = r.IsDBNull(5) ? "" : r.GetString(5)
                });
            return list;
        }
    }
}