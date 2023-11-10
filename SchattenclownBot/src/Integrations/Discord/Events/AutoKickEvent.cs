using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;

namespace SchattenclownBot.Integrations.Discord.Events
{
    public class AutoKickEvent
    {
        public async Task ConnectedEvent(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.User.Id == 941349242209984613 && eventArgs.Guild.Id == 807254423592108032)
                {
                    DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
                    await discordMember.DisconnectFromVoiceAsync();
                    await discordMember.SendMessageAsync("BB");
                }
            }
            catch
            {
                //ignore
            }
        }
    }
}