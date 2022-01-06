using DisCatSharp;
using DisCatSharp.EventArgs;
using System;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord.Events
{
    /// <summary>
    /// The guild events.
    /// </summary>
    internal class GuildEvents
    {
        public static Task Client_GuildUnavailable(DiscordClient dcl, GuildDeleteEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Attention! Guild " + e.Guild.Name + " is unavailable");
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }

        public static Task Client_GuildAvailable(DiscordClient dcl, GuildCreateEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Information: Guild " + e.Guild.Name + " is available (" + e.Guild.Id + ")");
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }
        public static Task Client_GuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Left Guild: " + e.Guild.Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }

        public static Task Client_GuildCreated(DiscordClient sender, GuildCreateEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Joined Guild: " + e.Guild.Name);
            Console.WriteLine("Reloading cache");
            //sender.ReconnectAsync(true);
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }
    }
}
