using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The configuration for the application.
        /// </summary>
        public static IConfigurationRoot Config { get; } = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        /// <returns></returns>
        public static async Task Main()
        {
            try
            {
                new CustomLogger().Create();
                new CustomLogger().Information("Starting SchattenclownBot...", ConsoleColor.Green);
                await new DiscordBot().RunAsync();

                DateTime dateTime = DateTime.Now.AddDays(1);
                dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 4, 0, 0, 0);
                TimeSpan timespan = dateTime - DateTime.Now;

                await Task.Delay(timespan);
                new Reset().RestartProgram();
            }
            catch (Exception exception)
            {
                new CustomLogger().Error(exception);
                new Reset().RestartProgram();
            }
        }
    }
}