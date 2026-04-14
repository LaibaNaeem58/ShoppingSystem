using ShoppingSystem.Models;
using ShoppingSystem.Services;

namespace ShoppingSystem.UI
{
    public class AuthMenu
    {
        private readonly AuthService _auth = new();

        public User? Show()
        {
            while (true)
            {
                D.Header("Welcome — Please Login or Register");
                D.Menu("MAIN MENU", new[] { "Login to Your Account", "Create New Account" });
                string c = D.Ask("Select option");
                switch (c)
                {
                    case "1": var u = DoLogin(); if (u != null) return u; break;
                    case "2": DoRegister(); break;
                    case "0": return null;
                    default: D.Err("Invalid choice."); Thread.Sleep(600); break;
                }
            }
        }

        private User? DoLogin()
        {
            D.Header("Login");
            string email = D.Ask("Email");
            string pass = D.AskPass();
            var (ok, msg, user) = _auth.Login(email, pass);
            if (ok)
            {
                D.Ok($"Welcome back, {user!.Name}!  [ Role: {user.Role} ]");
                D.Notify("Login successful.", ConsoleColor.Green);
                Thread.Sleep(600);
                return user;
            }
            D.Err(msg); D.Pause(); return null;
        }

        private void DoRegister()
        {
            D.Header("Create New Account");
            string name = D.Ask("Full Name");
            string email = D.Ask("Email Address");
            string pass = D.AskPass("Password");
            string confirm = D.AskPass("Confirm Password");
            if (pass != confirm) { D.Err("Passwords do not match."); D.Pause(); return; }
            string phone = D.Ask("Phone Number");
            string address = D.Ask("Default Delivery Address");

            var (ok, msg, _) = _auth.Register(name, email, pass, phone, address);
            if (ok) { D.Ok(msg); D.Notify("Account created! You can now login."); }
            else D.Err(msg);
            D.Pause();
        }
    }
}