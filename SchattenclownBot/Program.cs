using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using System;
using System.Threading.Tasks;

namespace SchattenclownBot
{
    /// <summary>
    /// The program boot class.
    /// </summary>
    internal class Program
    {
        private static Bot _bot;
        /// <summary>
        /// the boot task
        /// </summary>
        /// <returns>Nothing</returns>
        private static async Task Main()
        {
            /*
            #region ConsoleSize
            try
            {
#pragma warning disable CA1416
                Console.SetWindowSize(300, 30);
            }
            catch (Exception)
            {
                Console.SetWindowSize(100, 10);
#pragma warning restore CA1416
            }
#endregion
            */
            
            await Task.Run(async () =>
            {
                try
                {
                    Connections connections = Connections.GetConnections();
                    Console.WriteLine(connections.MySqlConStr);
                    Console.WriteLine(connections.YouTubeApiKey);
                    Console.WriteLine(connections.Token);
                    Console.WriteLine(connections.DiscordBotKey);
                    Console.WriteLine(connections.AcoustIdApiKey);
                    Console.WriteLine(connections.Token.ClientSecret);
                    Console.WriteLine(connections.Token.ClientId);
                    _bot = new Bot();
                    _bot.RunAsync().Wait();
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    //Reset.RestartProgram();
                }
            });
            
        }
    }
}
