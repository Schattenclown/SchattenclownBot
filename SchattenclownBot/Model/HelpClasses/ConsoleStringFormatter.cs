using System;
// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.HelpClasses
{
   public class ConsoleStringFormatter
   {
      /// <summary>
      /// Centers the console.
      /// </summary>
      /// <param name="centerString">The text.</param>
      public static void Center(string centerString)
      {
         try
         {
            Console.Write("██");
            Console.SetCursorPosition((Console.WindowWidth - centerString.Length) / 2, Console.CursorTop);
            Console.Write(centerString);
            Console.SetCursorPosition(Console.WindowWidth - 4, Console.CursorTop);
            Console.WriteLine("██");
         }
         catch
         {
            centerString = "Console to small!";
            Console.SetCursorPosition((Console.WindowWidth - centerString.Length) / 2, Console.CursorTop);
            Console.Write(centerString);
            Console.SetCursorPosition(Console.WindowWidth - 4, Console.CursorTop);
            Console.WriteLine("██");
         }
      }
      public static void FillRow()
      {
         Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
      }
   }
}

