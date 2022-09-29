using SchattenclownBot.Model.HelpClasses;
using System;
using System.Reflection;

namespace SchattenclownBot
{
   internal class CWLogger
   {
      public static void Write(string writeLineString, string callerClass, ConsoleColor color)
      {
         if (writeLineString.Contains("CREATE TABLE IF NOT EXISTS"))
         {
            writeLineString = StringCutter.RemoveAfterWord(writeLineString, "` (`", 0);
            writeLineString = StringCutter.RemoveAfterWord(writeLineString, " (`", 0);
            writeLineString = StringCutter.RemoveUntilWord(writeLineString, "CREATE TABLE IF NOT EXISTS `", "CREATE TABLE IF NOT EXISTS `".Length);
         }

         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"[{DateTime.Now} +02:00] [69  /{"Info".PadRight(12)}]");
         Console.ForegroundColor = color;
         Console.Write($" [{callerClass}] ");
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine($"{writeLineString}");
      }

      public static void Write(Exception ex, string caller, ConsoleColor color)
      {
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"[{DateTime.Now} +02:00] [420 /{"Exception".PadRight(12)}]");
         Console.ForegroundColor = color;
         Console.Write($" [{caller}] ");
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine($"{ex.Message}");
      }
   }
}
