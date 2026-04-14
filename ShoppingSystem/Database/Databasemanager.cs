using Microsoft.Data.Sqlite;

namespace ShoppingSystem.Database
{
    public static class DatabaseManager
    {
        // ✅ FIXED PATH (IMPORTANT)
        private static readonly string DbFolder =
            Path.Combine(Directory.GetCurrentDirectory(), "Data");

        private static readonly string DbFile =
            Path.Combine(DbFolder, "shopping.db");

        public static string ConnectionString => $"Data Source={DbFile}";

        public static void Initialize()
        {
            // ✅ Ensure folder exists
            Directory.CreateDirectory(DbFolder);

            // ✅ Ensure DB file exists
            if (!File.Exists(DbFile))
            {
                using (File.Create(DbFile)) { }
            }

            Console.WriteLine("DB Path: " + DbFile); // Debug (optional)

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            // ── USERS ───────────────────────────────
            Exec(conn, @"CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL,
                Role TEXT NOT NULL DEFAULT 'Customer',
                Phone TEXT DEFAULT '',
                Address TEXT DEFAULT '',
                CreatedAt TEXT NOT NULL DEFAULT (datetime('now'))
            );");

            // ── PRODUCTS ────────────────────────────
            Exec(conn, @"CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Category TEXT NOT NULL,
                Description TEXT,
                Price REAL NOT NULL,
                Stock INTEGER NOT NULL DEFAULT 0
            );");

            // ── CART ────────────────────────────────
            Exec(conn, @"CREATE TABLE IF NOT EXISTS Cart (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER,
                ProductId INTEGER,
                Quantity INTEGER
            );");

            // ── ORDERS ──────────────────────────────
            Exec(conn, @"CREATE TABLE IF NOT EXISTS Orders (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER,
                CustomerName TEXT,
                TotalAmount REAL,
                PaymentMethod TEXT,
                DeliveryType TEXT,
                DeliveryAddress TEXT,
                OrderDate TEXT
            );");

            // ── ORDER ITEMS ─────────────────────────
            Exec(conn, @"CREATE TABLE IF NOT EXISTS OrderItems (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER,
                ProductId INTEGER,
                ProductName TEXT,
                Quantity INTEGER,
                UnitPrice REAL
            );");

            // ── SEED ADMIN ─────────────────────────
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Role='Admin'";

            if ((long)(cmd.ExecuteScalar() ?? 0) == 0)
            {
                Exec(conn, @"INSERT INTO Users
                (Name, Email, Password, Role)
                VALUES ('Admin', 'admin@shop.com', 'admin123', 'Admin');");
            }

            // ── SEED PRODUCTS ──────────────────────
            cmd.CommandText = "SELECT COUNT(*) FROM Products";

            if ((long)(cmd.ExecuteScalar() ?? 0) == 0)
            {
                Exec(conn, @"INSERT INTO Products(Name,Category,Price,Stock)
                VALUES
                ('Laptop','Electronics',95000,10),
                ('Phone','Electronics',50000,20),
                ('Headphones','Electronics',3000,30);");
            }
        }

        private static void Exec(SqliteConnection conn, string sql)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }
    }
}