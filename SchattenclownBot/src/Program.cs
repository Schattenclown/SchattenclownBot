using System;
using System.Threading.Tasks;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot
{
    public static class Program
    {
        private static DiscordBot _discordBot;

        private static async Task Main()
        {
            try
            {
                CustomLogger.Create();
                CustomLogger.Information("Starting SchattenclownBot...", ConsoleColor.Green);
                _discordBot = new DiscordBot();
                await _discordBot.RunAsync();
                await Task.Delay(-1);
            }
            catch
            {
                CustomLogger.Information("SchattenclownBot resetting!", ConsoleColor.Red);
                Reset.RestartProgram();
            }
        }
    }
}