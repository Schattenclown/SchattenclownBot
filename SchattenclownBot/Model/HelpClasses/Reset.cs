using SchattenclownBot.HelpClasses;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace SchattenclownBot.Model.HelpClasses
{
    public class Reset
    {
        /// <summary>
        /// Restarts the program.
        /// </summary>
        public static void RestartProgram()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
            ConsoleForamter.Center(" ");
            ConsoleForamter.Center(@"██████╗ ███████╗███████╗████████╗ █████╗ ██████╗ ████████╗██╗███╗   ██╗ ██████╗ ");
            ConsoleForamter.Center(@"██╔══██╗██╔════╝██╔════╝╚══██╔══╝██╔══██╗██╔══██╗╚══██╔══╝██║████╗  ██║██╔════╝ ");
            ConsoleForamter.Center(@"██████╔╝█████╗  ███████╗   ██║   ███████║██████╔╝   ██║   ██║██╔██╗ ██║██║  ███╗");
            ConsoleForamter.Center(@"██╔══██╗██╔══╝  ╚════██║   ██║   ██╔══██║██╔══██╗   ██║   ██║██║╚██╗██║██║   ██║");
            ConsoleForamter.Center(@"██║  ██║███████╗███████║   ██║   ██║  ██║██║  ██║   ██║   ██║██║ ╚████║╚██████╔╝");
            ConsoleForamter.Center(@"╚═╝  ╚═╝╚══════╝╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ");
            ConsoleForamter.Center(" ");
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
            ConsoleForamter.Center("DB IS DEAD");
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");

            // Get file path of current process 
            var filePath = Assembly.GetExecutingAssembly().Location;
            var newFilepath = "";
            //BotDLL.dll

            if (filePath.Contains("Debug"))
            {
                filePath = WordCutter.RemoveAfterWord(filePath, "Debug", 0);
                newFilepath = filePath + "Debug\\netcoreapp3.1\\SchattenclownBot.exe";
            }
            else if (filePath.Contains("Release"))
            {
                filePath = WordCutter.RemoveAfterWord(filePath, "Release", 0);
                newFilepath = filePath + "Release\\netcoreapp3.1\\SchattenclownBot.exe";
            }

            Console.WriteLine("Before 120 secound sleep");
            Thread.Sleep(1000 * 60);
            Console.WriteLine("After 120 secound sleep");
            // Start program
            Process.Start(newFilepath);

            // For all Windows application but typically for Console app.
            Environment.Exit(0);
        }
    }
}
