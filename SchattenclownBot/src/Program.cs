using System;
using System.Threading.Tasks;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot
{
    public static class Program
    {
        public static async Task Main()
        {
            try
            {
                new CustomLogger().Create();
                new CustomLogger().Information("Starting SchattenclownBot...", ConsoleColor.Green);
                _ = new DiscordBot().RunAsync();
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