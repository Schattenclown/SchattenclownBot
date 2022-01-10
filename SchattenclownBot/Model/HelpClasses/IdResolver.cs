using System;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.HelpClasses
{
    public class IdResolver
    {
        public static async Task Resolve(ulong id)
        {
            var discordUser = await Discord.DiscordBot.Client.GetUserAsync(id);

            Console.WriteLine(discordUser.Username);

            await Task.Delay(1000);
        }
    }
}
