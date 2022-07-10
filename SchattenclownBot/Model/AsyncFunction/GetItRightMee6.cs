using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using DisCatSharp.Net.Models;
namespace SchattenclownBot.Model.AsyncFunction
{
    internal class GetItRightMee6
    {
        internal static Task ItRight(DiscordClient client, ChannelCreateEventArgs e)
        {
            if (e.Channel.Name.Contains("🥇AFK-Farm#"))
            {
                e.Channel.ModifyAsync(x => x.Bitrate = 256000);
            }

            return Task.CompletedTask;
        }
    }
}
