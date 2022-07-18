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
using DisCatSharp.Enums;

namespace SchattenclownBot.Model.AsyncFunction
{
    internal class PlayMusic
    {
        public async Task PlayMusicAsync()
        {
            
        }
        internal static Task ChangeStatus(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            if(e.User == Bot.Client.CurrentUser)
            {
                if(e.Channel == null)
                {
                    var activity = new DiscordActivity()
                    {
                        Name = $"/help",
                        ActivityType = ActivityType.Competing,
                        
                    };
                    Bot.Client.UpdateStatusAsync(activity: activity, userStatus: UserStatus.Online, idleSince: null);
                }
            }

            return Task.CompletedTask;
        }
    }
}
