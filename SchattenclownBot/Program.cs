using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using SchattenclownBot.Model.Discord;
using SchattenclownBot.Model.Persistence;
using SchattenclownBot.Model.Objects;

namespace SchattenclownBot
{
    /// <summary>
    /// The program boot class.
    /// </summary>
    class Program
    {
        private static DiscordBot dBot;
        /// <summary>
        /// the boot task
        /// </summary>
        /// <returns>Nothing</returns>
        static async Task Main()
        {
            try
            {
                DB_ScTimers.CreateTable_ScTimers();
                DB_ScAlarmClocks.CreateTable_ScAlarmClocks();

                dBot = new DiscordBot();
                await dBot.RunAsync();

                await Task.Delay(1000 * 60 * 60 * 24);
                Program p = new Program();
                await p.RestartProgram();
            }
            catch
            {
                Program p = new Program();
                await p.RestartProgram();
            }
        }

        /// <summary>
        /// Centers the console.
        /// </summary>
        /// <param name="s">The text.</param>
        static void Center(string s)
        {
            try
            {
                Console.Write("██");
                Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
                Console.Write(s);
                Console.SetCursorPosition((Console.WindowWidth - 4), Console.CursorTop);
                Console.WriteLine("██");
            }
            catch (Exception)
            {
                s = "Console to smoll EXC";
                Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
                Console.Write(s);
                Console.SetCursorPosition((Console.WindowWidth - 4), Console.CursorTop);
                Console.WriteLine("██");
            }
        }

        /// <summary>
        /// Restarts the program.
        /// </summary>
        private async Task RestartProgram()
        {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
            Center(" ");
            Center(@"██████╗ ███████╗███████╗████████╗ █████╗ ██████╗ ████████╗██╗███╗   ██╗ ██████╗ ");
            Center(@"██╔══██╗██╔════╝██╔════╝╚══██╔══╝██╔══██╗██╔══██╗╚══██╔══╝██║████╗  ██║██╔════╝ ");
            Center(@"██████╔╝█████╗  ███████╗   ██║   ███████║██████╔╝   ██║   ██║██╔██╗ ██║██║  ███╗");
            Center(@"██╔══██╗██╔══╝  ╚════██║   ██║   ██╔══██║██╔══██╗   ██║   ██║██║╚██╗██║██║   ██║");
            Center(@"██║  ██║███████╗███████║   ██║   ██║  ██║██║  ██║   ██║   ██║██║ ╚████║╚██████╔╝");
            Center(@"╚═╝  ╚═╝╚══════╝╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ");
            Center(" ");
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
            Center("Restating in 10 secounds.");
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
            await Task.Delay(1000 * 10);

            dBot.Dispose();
            // Get file path of current process 
            var filePath = Assembly.GetExecutingAssembly().Location;
            //var filePath = Application.ExecutablePath;  // for WinForms

            // Start program
            Process.Start(filePath);

            // For all Windows application but typically for Console app.
            Environment.Exit(0);
        }
    }
}
