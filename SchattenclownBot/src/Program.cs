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
                    ConsoleLogger.WriteLine("Starting DiscordBot...");
                    _discordBot = new DiscordBot();
                    _discordBot.RunAsync().Wait();
                    await Task.Delay(1000);
                }
                catch
                {
                    ConsoleLogger.WriteLine("DiscordBot resetting!", true);
                    Reset.RestartProgram();
                }
            });
        }
    }
}