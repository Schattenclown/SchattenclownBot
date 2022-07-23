using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using System;
using System.Threading.Tasks;

namespace SchattenclownBot
{
    /// <summary>
    /// The program boot class.
    /// </summary>
    internal class Program
    {
        private static Bot bot;
        /// <summary>
        /// the boot task
        /// </summary>
        /// <returns>Nothing</returns>
        private static async Task Main()
        {
            #region ConsoleSize
            try
            {
#pragma warning disable CA1416 // Plattformkompatibilität überprüfen
                Console.SetWindowSize(300, 30);
            }
            catch (Exception)
            {
                Console.SetWindowSize(100, 10);
#pragma warning restore CA1416 // Plattformkompatibilität überprüfen
            }
            #endregion
            await Task.Run(async () =>
            {
                try
                {
                    bot = new Bot();
                    bot.RunAsync().Wait();
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
