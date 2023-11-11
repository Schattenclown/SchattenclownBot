using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot
{
    public static class Program
    {
        public static IConfigurationRoot Config { get; } = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();

        public static async Task Main()
        {
            try
            {
                new CustomLogger().Create();
                new CustomLogger().Information("Starting SchattenclownBot...", ConsoleColor.Green);
                await new DiscordBot().RunAsync();
                await Task.Delay(-1);
            }
            catch
            {
                new CustomLogger().Information("SchattenclownBot resetting!", ConsoleColor.Red);
                new Reset().RestartProgram();
            }
        }
    }
}