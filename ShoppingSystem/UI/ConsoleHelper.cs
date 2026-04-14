namespace ShoppingSystem.UI
{
    public static class CH
    {
        public static void Header(string title)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╔══════════════════════════════════════════════════════╗");
            Console.WriteLine("  ║          ONLINE SHOPPING MANAGEMENT SYSTEM           ║");
            Console.WriteLine("  ╠══════════════════════════════════════════════════════╣");
            Console.ForegroundColor = ConsoleColor.White;
            string t = title.Length > 52 ? title[..52] : title;
            string padded = t.PadLeft((52 + t.Length) / 2).PadRight(52);
            Console.WriteLine($"  ║  {padded}  ║");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  ╚══════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
        }

        public static void Sep(char c = '─', int n = 56)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  " + new string(c, n));
            Console.ResetColor();
        }

        public static void OK(string m) { Console.ForegroundColor = ConsoleColor.Green; Console.WriteLine($"  ✔  {m}"); Console.ResetColor(); }
        public static void Err(string m) { Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"  ✘  {m}"); Console.ResetColor(); }
        public static void Info(string m) { Console.ForegroundColor = ConsoleColor.Cyan; Console.WriteLine($"  ℹ  {m}"); Console.ResetColor(); }
        public static void Warn(string m) { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine($"  ⚠  {m}"); Console.ResetColor(); }

        public static string Ask(string prompt)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  >> {prompt}: ");
            Console.ResetColor();
            return Console.ReadLine()?.Trim() ?? "";
        }

        public static string AskPassword(string prompt = "Password")
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  >> {prompt}: ");
            Console.ResetColor();
            var sb = new System.Text.StringBuilder();
            ConsoleKeyInfo k;
            do
            {
                k = Console.ReadKey(true);
                if (k.Key != ConsoleKey.Backspace && k.Key != ConsoleKey.Enter)
                { sb.Append(k.KeyChar); Console.Write("*"); }
                else if (k.Key == ConsoleKey.Backspace && sb.Length > 0)
                { sb.Remove(sb.Length - 1, 1); Console.Write("\b \b"); }
            } while (k.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return sb.ToString();
        }

        public static void Pause(string m = "Press any key to continue...")
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"\n  {m}");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        public static void Menu(string[] opts, string title = "")
        {
            if (!string.IsNullOrEmpty(title))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"  {title}");
                Console.ResetColor();
                Sep();
            }
            for (int i = 0; i < opts.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write($"  [{i + 1}] ");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(opts[i]);
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("  [0] Back / Logout");
            Console.ResetColor();
            Sep();
        }

        public static void Stars(int rating)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("  " + new string('★', rating) + new string('☆', 5 - rating));
            Console.ResetColor();
            Console.WriteLine($"  ({rating}/5)");
        }

        public static void Notify(string msg, ConsoleColor col = ConsoleColor.Cyan)
        {
            Console.ForegroundColor = col;
            Console.WriteLine($"\n  ▶ {msg}");
            Console.ResetColor();
            Thread.Sleep(500);
        }
    }
}