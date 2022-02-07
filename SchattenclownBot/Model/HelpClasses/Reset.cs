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
        public static void RestartProgram(Exception exception)
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
            ConsoleForamter.Center(exception.Message);
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");

            // Get file path of current process 
            var filePath = Assembly.GetExecutingAssembly().Location;
            var newFilepath = "";
            //BotDLL.dll

            if (filePath.Contains("Debug"))
            {
                filePath = WordCutter.RemoveAfterWord(filePath, "Debug", 0);
                newFilepath = filePath + "Debug\\net6.0\\SchattenclownBot.exe";
            }
            else if (filePath.Contains("Release"))
            {
                filePath = WordCutter.RemoveAfterWord(filePath, "Release", 0);
                newFilepath = filePath + "Release\\net6.0\\SchattenclownBot.exe";
            }

            Console.WriteLine("Sleeping for 60 seconds!");
            Thread.Sleep(1000 * 60);
            // Start program
            Process.Start(newFilepath);

            // For all Windows application but typically for Console app.
            Environment.Exit(0);
        }
    }
}
