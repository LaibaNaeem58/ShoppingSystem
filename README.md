# 🛒 Online Shopping Management System

Console-based C#  application with SQLite database.

---

## 🚀 How to Run (Visual Studio / VS Code)

### Step 1 — Open the project
```
File → Open → Folder → ShoppingSystem
```

### Step 2 — Run
```
dotnet run
```
Or press **F5** in Visual Studio.

> ✅ The `Data/` folder and `shopping.db` are created **automatically** on first run.

---

## 🔑 Default Login

| Role  | Email             | Password  |
|-------|-------------------|-----------|
| Admin | admin@shop.com    | admin123  |

---

## 👤 Features

### Admin
- View / Add / Update / Delete Products
- Low Stock Alerts
- View All Orders, Payments, Reviews
- Delete Reviews
- View All Customers
- Dashboard & Reports

### Customer
- Browse & Search Products by Category
- View Product Details & Reviews
- Add to Cart / Update Quantity / Remove Items
- Checkout with Delivery Form
- Payment Gateway: EasyPaisa, JazzCash, Card, COD
- Order Success Screen
- Order History & Payment History
- Write Product Reviews (only for purchased products)

---

## 🗄️ Database Tables

| Table       | Description                       |
|-------------|-----------------------------------|
| Users       | Admin & Customer accounts         |
| Products    | Product catalog with stock        |
| Cart        | Customer shopping cart            |
| Orders      | Placed orders                     |
| OrderItems  | Items inside each order           |
| Payments    | Payment records with TxID         |
| Reviews     | Product ratings & comments        |

---

## 📁 Project Structure

```
ShoppingSystem/
├── Program.cs
├── ShoppingSystem.csproj
├── Database/
│   └── DatabaseManager.cs      ← Creates DB + seeds data automatically
├── Models/
│   ├── User.cs
│   ├── Product.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   ├── Payment.cs
│   └── Review.cs
├── Services/
│   ├── AuthService.cs
│   ├── ProductService.cs
│   ├── CartService.cs
│   ├── OrderService.cs
│   ├── PaymentService.cs
│   └── ReviewService.cs
├── UI/
│   ├── Design.cs               ← All console colours & drawing
│   ├── AuthMenu.cs
│   ├── AdminPanel.cs
│   └── CustomerPanel.cs
├── Data/                       ← Auto-created at runtime
│   └── shopping.db             ← SQLite database (auto-created)
├── Dockerfile
└── README.md
```

---

## 🐛 Fix: SQLite Error 14

If you see `SQLite Error 14: unable to open database file`, it means the
`Data/` folder did not exist. This version fixes it by calling
`Directory.CreateDirectory(dataFolder)` **before** opening the connection.

---

## 🐳 Docker

```bash
docker build -t shopping-system .
docker run -it -v shopping_data:/app/Data shopping-system
```
