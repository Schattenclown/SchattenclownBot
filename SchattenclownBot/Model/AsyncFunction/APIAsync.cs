using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.AppCommands;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class ApiAsync
   {
      public static async Task ReadFromApiAsync()
      {
         await Task.Run(async () =>
         {
            List<DiscordGuild> guildList;
            do
            {
               guildList = Bot.DiscordClient.Guilds.Values.ToList();
               await Task.Delay(1000);
            } while (guildList.Count == 0);

            while (true)
            {
               List<Api> aPiObjects = Api.Get();
               foreach (var item in aPiObjects)
               {
                  switch (item.Command)
                  {
                     case "Next_Song":
                        PlayMusic.NextRequestApi(item);
                        break;
                     case "RequestUserName":
                        RequestUserNameAnswer(item);
                        break;
                  }

               }
               await Task.Delay(100);
            }
         });
      }
      public static async void RequestUserNameAnswer(Api aPi)
      {
         Api.Delete(aPi.CommandRequestId);
         DiscordUser discordUser = await Bot.DiscordClient.GetUserAsync(aPi.RequestDiscordUserId);
         aPi.Data = discordUser.Username;
         aPi.Command = "RequestUserNameAnswer";
         Api.Put(aPi);
      }
   }
}
