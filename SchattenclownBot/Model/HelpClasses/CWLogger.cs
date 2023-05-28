using System;

namespace SchattenclownBot.Model.HelpClasses
{
   internal class CwLogger
   {
      // Helper method to log to console in a specific color
      private static int _colors = 1;

      public static void Write(string writeLineString, string callerFunction, ConsoleColor color)
      {
         _colors++;
         if (_colors > 15)
         {
            _colors = 1;
         }

         Array colors = Enum.GetValues(typeof(ConsoleColor));
         int colorIndex = Array.IndexOf(colors, ConsoleColor.Black) + _colors;
         ConsoleColor consoleColor = (ConsoleColor)colors.GetValue(colorIndex)!;

         if (writeLineString.Contains("CREATE TABLE IF NOT EXISTS"))
         {
            writeLineString = StringCutter.RmAfter(writeLineString, "` (`", 0);
            writeLineString = StringCutter.RmAfter(writeLineString, " (`", 0);
            writeLineString = StringCutter.RmUntil(writeLineString, "CREATE TABLE IF NOT EXISTS `", "CREATE TABLE IF NOT EXISTS `".Length);
         }

         if (callerFunction.Contains("<") || callerFunction.Contains(">"))
         {
            callerFunction = StringCutter.RmUntil(callerFunction, "<", "<".Length);
            callerFunction = StringCutter.RmUntil(callerFunction, "<", "<".Length);
            callerFunction = StringCutter.RmAfter(callerFunction, ">", 0);
            callerFunction = StringCutter.RmAfter(callerFunction, ">", 0);
            color = ConsoleColor.Cyan;
         }

         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"[{DateTime.Now} +02:00] [69  /{"Info",-12}]");
         Console.ForegroundColor = color;
         Console.Write($" [{callerFunction,-40}] ");
         Console.ForegroundColor = consoleColor;
         Console.WriteLine($"{writeLineString}");
         Console.ForegroundColor = ConsoleColor.Gray;
      }

      public static void Write(Exception ex, string callerFunction, ConsoleColor color)
      {
         if (callerFunction.Contains("<") || callerFunction.Contains(">"))
         {
            callerFunction = StringCutter.RmUntil(callerFunction, "<", "<".Length);
            callerFunction = StringCutter.RmAfter(callerFunction, ">", 0);
            color = ConsoleColor.Cyan;
         }

         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"[{DateTime.Now} +02:00] [420 /{"Exception",-12}]");
         Console.ForegroundColor = color;
         Console.Write($" [{callerFunction,-40}] ");
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine($"{ex.Message}");
      }
   }
}