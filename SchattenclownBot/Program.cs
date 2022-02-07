using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using SchattenclownBot.Model.Discord;
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
        private static DiscordBot dBot;
        /// <summary>
        /// the boot task
        /// </summary>
        /// <returns>Nothing</returns>
        static async Task Main()
        {
            await Task.Run(async () =>
            {
                try
                {
                    DB_ScTimers.CreateTable_ScTimers();
                    DB_ScAlarmClocks.CreateTable_ScAlarmClocks();

                    dBot = new DiscordBot();
    #pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                    await dBot.RunAsync();
    #pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
                }
                catch
                {
                    Reset.RestartProgram();
                }
            });
        }
    }
}
