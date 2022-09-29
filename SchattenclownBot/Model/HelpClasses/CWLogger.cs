using SchattenclownBot.Model.HelpClasses;
using System;
using System.Reflection;

namespace SchattenclownBot
{
   internal class CWLogger
   {
      public static void Write(string WriteLineString, string callerClass, ConsoleColor color)
      {
         if (WriteLineString.Contains("CREATE TABLE IF NOT EXISTS"))
            WriteLineString = StringCutter.RemoveAfterWord(WriteLineString, " (`", 0);

         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"[{DateTime.Now} +02:00] [    /{"INFO".PadRight(12)}]");
         Console.ForegroundColor = color;
         Console.Write($" [{callerClass}] ");
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine($"{WriteLineString}");
      }

      public static void Write(Exception ex, string caller, ConsoleColor color)
      {
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"[{DateTime.Now} +02:00] [    /{"EXCEPTION".PadRight(12)}]");
         Console.ForegroundColor = color;
         Console.Write($" [{caller}] ");
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine($"{ex.Message}");
      }
   }
}
