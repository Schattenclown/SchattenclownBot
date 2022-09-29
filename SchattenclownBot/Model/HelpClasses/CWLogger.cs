using SchattenclownBot.Model.HelpClasses;
using System;

namespace SchattenclownBot
{
   internal class CWLogger
   {
      public static void Write(string data, string state, ConsoleColor color)
      {
         if (data.Contains("CREATE TABLE IF NOT EXISTS"))
            data = StringCutter.RemoveAfterWord(data, " (`", 0);

         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"[{DateTime.Now} +02:00] [    /            ]");
         Console.ForegroundColor = color;
         Console.Write($" [{state}] ");
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine($"{data}");
      }
      public static void Write(string data,string something, string state, ConsoleColor color)
      {
         if (data.Contains("CREATE TABLE IF NOT EXISTS"))
            data = StringCutter.RemoveAfterWord(data, " (`", 0);

         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"[{DateTime.Now} +02:00] [    /{something.PadRight(12)}]");
         Console.ForegroundColor = color;
         Console.Write($" [{state}] ");
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine($"{data}");
      }
   }
}
