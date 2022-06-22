using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot
{
    /// <summary>
    /// The program boot class.
    /// </summary>
    class Program
    {
        private static Bot bot;
        /// <summary>
        /// the boot task
        /// </summary>
        /// <returns>Nothing</returns>
        static async Task Main()
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
