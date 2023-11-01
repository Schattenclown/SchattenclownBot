using System;
using System.Threading.Tasks;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot
{
    internal class Program
    {
        private static DiscordBot _discordBot;

        private static async Task Main()
        {
            await Task.Run(async () =>
            {
                try
                {
                    CustomLogger.Create();
                    CustomLogger.ToConsole("Starting SchattenclownBot...", ConsoleColor.Green);
                    _discordBot = new DiscordBot();
                    _discordBot.RunAsync().Wait();
                    await Task.Delay(1000);
                }
                catch
                {
                    CustomLogger.ToConsole("SchattenclownBot resetting!", ConsoleColor.Red);
                    Reset.RestartProgram();
                }
            });
        }
    }
}