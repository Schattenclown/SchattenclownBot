using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace SchattenclownBot.Model.HelpClasses;

public class Reset
{
   /// <summary>
   ///    Restarts the program.
   /// </summary>
   public static void RestartProgram()
   {
      Console.ForegroundColor = ConsoleColor.Red;
      Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
      ConsoleStringFormatter.Center(" ");
      ConsoleStringFormatter.Center(@"██████╗ ███████╗███████╗████████╗ █████╗ ██████╗ ████████╗██╗███╗   ██╗ ██████╗ ");
      ConsoleStringFormatter.Center(@"██╔══██╗██╔════╝██╔════╝╚══██╔══╝██╔══██╗██╔══██╗╚══██╔══╝██║████╗  ██║██╔════╝ ");
      ConsoleStringFormatter.Center(@"██████╔╝█████╗  ███████╗   ██║   ███████║██████╔╝   ██║   ██║██╔██╗ ██║██║  ███╗");
      ConsoleStringFormatter.Center(@"██╔══██╗██╔══╝  ╚════██║   ██║   ██╔══██║██╔══██╗   ██║   ██║██║╚██╗██║██║   ██║");
      ConsoleStringFormatter.Center(@"██║  ██║███████╗███████║   ██║   ██║  ██║██║  ██║   ██║   ██║██║ ╚████║╚██████╔╝");
      ConsoleStringFormatter.Center(@"╚═╝  ╚═╝╚══════╝╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ");
      ConsoleStringFormatter.Center(" ");
      Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
      ConsoleStringFormatter.Center("DB IS DEAD");
      Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
      Console.ForegroundColor = ConsoleColor.Gray;

      // HandlerReader file path of current process 
      string filePath = Assembly.GetExecutingAssembly().Location;
      string newFilepath = "";
      //BotDLL.dll

      if (filePath.Contains("Debug"))
      {
         filePath = StringCutter.RmAfter(filePath, "Debug", 0);
         newFilepath = filePath + "Debug\\net6.0\\SchattenclownBot.exe";
      }
      else if (filePath.Contains("Release"))
      {
         filePath = StringCutter.RmAfter(filePath, "Release", 0);
         newFilepath = filePath + "Release\\net6.0\\SchattenclownBot.exe";
      }

      Console.WriteLine("Before 120 second sleep");
      Thread.Sleep(1000 * 60);
      Console.WriteLine("After 120 second sleep");
      // Start program
      Process.Start(newFilepath);

      // For all Windows application but typically for Console app.
      Environment.Exit(0);
   }
}