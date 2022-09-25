using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchattenclownBot
{
   internal class CWLogger
   {
      public static void Write(string data, string state, ConsoleColor color)
      {
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.Write($"{DateTime.Now}");
         Console.ForegroundColor = color;
         Console.Write($" [{state}] ");
         Console.ForegroundColor = ConsoleColor.Gray;
         Console.WriteLine($"{data}\n");
      }
   }
}
