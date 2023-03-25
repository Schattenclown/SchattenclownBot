using System;
using System.Threading.Tasks;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot
{
   /// <summary>
   ///    The program boot class.
   /// </summary>
   internal class Program
   {
      private static Bot _bot;

      /// <summary>
      ///    the boot task
      /// </summary>
      /// <returns>Nothing</returns>
      private static async Task Main()
      {
         #region ConsoleSize

         try
         {
#pragma warning disable CA1416
            Console.SetWindowSize(250, 30);
         }
         catch (Exception)
         {
            Console.SetWindowSize(100, 10);
#pragma warning restore CA1416
         }

         #endregion

         await Task.Run(async () =>
         {
            try
            {
               _bot = new Bot();
               _bot.RunAsync().Wait();
               await Task.Delay(1000);
            }
            catch
            {
               Reset.RestartProgram();
            }
         });
      }
   }
}