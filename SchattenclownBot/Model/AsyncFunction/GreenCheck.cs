using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using SchattenclownBot.Model.Discord.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class GreenCheck
   {
      public static async Task CheckGreenTask(int executeSecond)
      {
         await Task.Run(async () =>
         {
            while (DateTime.Now.Second != executeSecond)
            {
               await Task.Delay(1000);
            }

            var listPositiv = new List<DiscordRole>();

            var guild = Bot.DiscordClient.GetGuildAsync(928930967140331590).Result;
            listPositiv.Add(guild.GetRole(928932796934799401)); //#b1b1b1
            listPositiv.Add(guild.GetRole(981575801214492752)); //⁣     ⁣  ⁣   Voice Channel Level             ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(949045273978630214)); //⁣Server Booster
            listPositiv.Add(guild.GetRole(928955073285984268)); //⁣     ⁣  ⁣   Sympathie Rating             ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(949045281108922459)); //⁣Sympathie Rating 1
            listPositiv.Add(guild.GetRole(949045284355342386)); //⁣Sympathie Rating 2
            listPositiv.Add(guild.GetRole(949045286808981565)); //⁣Sympathie Rating 3
            listPositiv.Add(guild.GetRole(949045289728233552)); //⁣Sympathie Rating 4
            listPositiv.Add(guild.GetRole(949045292282544148)); //⁣Sympathie Rating 5
            listPositiv.Add(guild.GetRole(928946804400193558)); //⁣     ⁣  ⁣   ⁣Über mich              ⁣   ⁣               ⁣
            listPositiv.Add(guild.GetRole(945300974623391754)); //⁣     ⁣  ⁣   Geburtstag             ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(945300961679794226)); //⁣     ⁣  ⁣   Sternzeichen             ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(928947169719910421)); //⁣     ⁣  ⁣   Kanäle⁣               ⁣   ⁣               ⁣    ⁣
            listPositiv.Add(guild.GetRole(928947030590644274)); //⁣     ⁣  ⁣   Benutzerdefiniert⁣⁣⁣              ⁣   ⁣
            listPositiv.Add(guild.GetRole(928947119425982615)); //⁣     ⁣  ⁣   Spiele             ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(931950722256371752)); //⁣     ⁣  ⁣   Shooter             ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(931950729663492207)); //⁣     ⁣  ⁣   Survival             ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(931950732658237501)); //⁣     ⁣  ⁣   Andere Spiele              ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(939236265587515462)); //⁣     ⁣  ⁣   16P             ⁣   ⁣               ⁣      ⁣
            listPositiv.Add(guild.GetRole(939216240512208956)); //⁣     ⁣  ⁣   BDSM             ⁣
            listPositiv.Add(guild.GetRole(945007672577642537)); //⁣     ⁣  ⁣   Energy             ⁣   ⁣               ⁣      ⁣


            var flagged = guild.GetRole(945023948645629973); //Flagged
            var flagged91 = guild.GetRole(980071522427363368); //Flagged +91

            var greeen = guild.GetRole(928934209484099615); //#b1b1b1
            var grey = guild.GetRole(928932796934799401); //#b1b1b1


            var discordUser = guild.Members.Values.ToList();

            foreach (var user in discordUser)
            {
               var userRoles = user.Roles;
               bool hasVoiceLevelProb = false;
               bool hasOtherRoles = false;

               foreach (var role in userRoles)
               {
                  if (!listPositiv.Contains(role))
                     hasVoiceLevelProb = true;

                  if (hasVoiceLevelProb && !listPositiv.Contains(role))
                     hasOtherRoles = true;
               }

               if (hasOtherRoles && !userRoles.Contains(flagged) && !userRoles.Contains(flagged91))
               {
                  await user.RevokeRoleAsync(grey);
                  await user.GrantRoleAsync(greeen);
               }
            }
            
            await Task.Delay(1000);
         });
      }
   }
}
