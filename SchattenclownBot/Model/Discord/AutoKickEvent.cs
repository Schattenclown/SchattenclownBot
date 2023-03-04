using DisCatSharp;
using DisCatSharp.EventArgs;
using System;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord
{
   internal class AutoKickEvent
   {
      internal static async Task ConnectedEvent(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
      {
         try
         {
            if (eventArgs.User.Id == 941349242209984613 && eventArgs.Guild.Id == 807254423592108032)
            {
               var discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
               await discordMember.DisconnectFromVoiceAsync();
               await discordMember.SendMessageAsync("BB");
            }
         }
         catch (Exception e)
         {

         }
      }
   }
}
