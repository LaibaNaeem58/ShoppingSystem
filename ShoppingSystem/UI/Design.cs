namespace ShoppingSystem.UI
{
    /// <summary>
    /// Centralised console design / drawing helpers.
    /// All panels use this class exclusively – no direct Console colour changes elsewhere.
    /// </summary>
    public static class D
    {
        // ── Colours ──────────────────────────────────────────────────
        private const ConsoleColor C_TITLE = ConsoleColor.DarkCyan;
        private const ConsoleColor C_HEAD = ConsoleColor.White;
        private const ConsoleColor C_OK = ConsoleColor.Green;
        private const ConsoleColor C_ERR = ConsoleColor.Red;
        private const ConsoleColor C_WARN = ConsoleColor.Yellow;
        private const ConsoleColor C_INFO = ConsoleColor.Cyan;
        private const ConsoleColor C_MUTED = ConsoleColor.DarkGray;
        private const ConsoleColor C_MENU = ConsoleColor.DarkYellow;
        private const ConsoleColor C_TEXT = ConsoleColor.Gray;

        // ── Header (double-box) ───────────────────────────────────────
        public static void Header(string subtitle)
        {
            Console.Clear();
            Write(C_TITLE, "  ╔══════════════════════════════════════════════════════╗\n");
            Write(C_TITLE, "  ║");
            Write(C_HEAD, "     ONLINE SHOPPING MANAGEMENT SYSTEM                 ");
            Write(C_TITLE, "║\n");
            Write(C_TITLE, "  ╠══════════════════════════════════════════════════════╣\n");
            string t = subtitle.Length > 52 ? subtitle[..52] : subtitle;
            string pad = t.PadLeft((52 + t.Length) / 2).PadRight(52);
            Write(C_TITLE, "  ║  ");
            Write(C_WARN, pad);
            Write(C_TITLE, "  ║\n");
            Write(C_TITLE, "  ╚══════════════════════════════════════════════════════╝\n\n");
        }

        // ── Sub-header (single line) ─────────────────────────────────
        public static void SubHeader(string text)
        {
            Console.WriteLine();
            Write(C_INFO, $"  ┌─ {text} ");
            Write(C_MUTED, new string('─', Math.Max(2, 50 - text.Length - 4)) + "┐\n");
        }

        // ── Separators ───────────────────────────────────────────────
        public static void Sep(char ch = '─', int n = 58)
        { Write(C_MUTED, "  " + new string(ch, n) + "\n"); }

        // ── Status lines ─────────────────────────────────────────────
        public static void Ok(string m) { Write(C_OK, $"  ✔  {m}\n"); }
        public static void Err(string m) { Write(C_ERR, $"  ✘  {m}\n"); }
        public static void Info(string m) { Write(C_INFO, $"  ℹ  {m}\n"); }
        public static void Warn(string m) { Write(C_WARN, $"  ⚠  {m}\n"); }

        // ── Notification banner ───────────────────────────────────────
        public static void Notify(string msg, ConsoleColor col = ConsoleColor.Green)
        { Write(col, $"\n  ▶  {msg}\n"); Thread.Sleep(450); }

        // ── Input prompt ─────────────────────────────────────────────
        public static string Ask(string prompt)
        {
            Write(C_WARN, "  >> ");
            Write(C_HEAD, prompt + ": ");
            Console.ResetColor();
            return Console.ReadLine()?.Trim() ?? "";
        }

        public static string AskPass(string prompt = "Password")
        {
            Write(C_WARN, "  >> ");
            Write(C_HEAD, prompt + ": ");
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

        // ── Menu ─────────────────────────────────────────────────────
        public static void Menu(string title, string[] items)
        {
            Write(C_WARN, $"\n  {title}\n");
            Sep();
            for (int i = 0; i < items.Length; i++)
            {
                Write(C_MENU, $"  [{i + 1}] ");
                Write(C_TEXT, items[i] + "\n");
            }
            Write(C_MUTED, "  [0] Back / Logout\n");
            Sep();
        }

        // ── Pause ────────────────────────────────────────────────────
        public static void Pause(string m = "  Press any key to continue...")
        { Write(C_MUTED, "\n" + m + "\n"); Console.ReadKey(true); }

        // ── Star rating ──────────────────────────────────────────────
        public static void Stars(int r)
        {
            r = Math.Clamp(r, 1, 5);
            Write(C_WARN, "  " + new string('★', r) + new string('☆', 5 - r));
            Write(C_MUTED, $"  ({r}/5)\n");
        }

        // ── Table row helpers ─────────────────────────────────────────
        public static void TableHeader(string text)
        { Write(C_WARN, "  " + text + "\n"); Sep(); }

        public static void Row(string text)
        { Write(C_TEXT, "  " + text + "\n"); }

        public static void RowGreen(string text)
        { Write(C_OK, "  " + text + "\n"); }

        public static void RowRed(string text)
        { Write(C_ERR, "  " + text + "\n"); }

        // ── Internal helper ───────────────────────────────────────────
        private static void Write(ConsoleColor col, string text)
        { Console.ForegroundColor = col; Console.Write(text); Console.ResetColor(); }
    }
}