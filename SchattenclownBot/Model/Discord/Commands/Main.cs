using DisCatSharp.CommandsNext;
using DisCatSharp.CommandsNext.Attributes;
using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord.Commands
{
    /// <summary>
    ///     Old commands.
    /// </summary>
    internal class Main : BaseCommandModule
    {
        /// <summary>
        ///     Main class for old commands.
        /// </summary>
        /// <param name="commandContext">The commandContext.</param>
        /// <returns>A Task.</returns>
        [Command("ping"), Description("Ping")]
        public async Task PingAsync(CommandContext commandContext)
        {
            await commandContext.RespondAsync($"{commandContext.Client.Ping}ms");
        }

        /*/// <summary>
        ///     Set a new appearance for the bot per command.
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="sts"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        [Command("mode"), Description("Change the appearance of the bots status")]
        public async Task SetModeAsync(CommandContext commandContext, [Description("Status mode to set. [ 1=offline | 2=online | 3=dnd] ")] int sts = 2, [Description("Status message to set."), RemainingText] string msg = null)
        {
            msg ??= $"{Bot.Prefix}help";

            UserStatus status = sts switch
            {
                1 => UserStatus.Invisible,
                2 => UserStatus.Online,
                3 => UserStatus.DoNotDisturb,
                4 => UserStatus.Idle,
                5 => UserStatus.Streaming,
                _ => UserStatus.Online,
            };

            DiscordActivity activity = new()
            {
                Name = msg,
                ActivityType = status == UserStatus.Streaming ? ActivityType.Streaming : ActivityType.Watching,
                Platform = status == UserStatus.Streaming ? "twitch" : null,
                StreamUrl = status == UserStatus.Streaming ? "https://twitch.tv/lulalaby" : null
            };
            await Bot.DiscordClient.UpdateStatusAsync(activity: activity, userStatus: status, idleSince: null);
            Bot.Custom = true;
            Bot.CustomState = msg;
            Bot.CustomStatus = status;

            await commandContext.Message.DeleteAsync("Command Hide");
        }*/
    }
}
