using System.Threading.Tasks;

using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;

using SchattenclownBot.Model.Discord.Main;

namespace SchattenclownBot.Model.Discord.Commands
{
    /// <summary>
    /// The MAIN.
    /// </summary>
    internal class Main : BaseCommandModule
    {
        /// <summary>
        /// prob. does nothing
        /// </summary>
        /// <param name="ctx">The ctx.</param>
        /// <returns>A Task.</returns>
        [Command("ping"), Description("Ping")]
        public async Task PingAsync(CommandContext ctx)
        {
            await ctx.RespondAsync($"{ctx.Client.Ping}ms");
        }

        [Command("mode"), Description("Change the appearance of the bots status")]
        public async Task SetModeAsync(CommandContext ctx, [Description("Status mode to set. [ 1=offline | 2=online | 3=dnd] ")] int sts = 2, [Description("Status message to set."), RemainingText] string msg = null)
        {
            if (msg == null)
            {
                msg = $"{Bot.prefix}help";
            }

            var status = sts switch
            {
                1 => UserStatus.Invisible,
                2 => UserStatus.Online,
                3 => UserStatus.DoNotDisturb,
                4 => UserStatus.Idle,
                5 => UserStatus.Streaming,
                _ => UserStatus.Online,
            };

            DiscordActivity activity = new DiscordActivity()
            {
                Name = msg,
                ActivityType = status == UserStatus.Streaming ? ActivityType.Streaming : ActivityType.Watching,
                Platform = status == UserStatus.Streaming ? "twitch" : null,
                StreamUrl = status == UserStatus.Streaming ? "https://twitch.tv/lulalaby" : null
            };
            await Bot.Client.UpdateStatusAsync(activity: activity, userStatus: status, idleSince: null);
            Bot.custom = true;
            Bot.customstate = msg;
            Bot.customstatus = status;

            await ctx.Message.DeleteAsync("Command Hide");
        }
    }
}
