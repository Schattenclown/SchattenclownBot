using DisCatSharp;
using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.EventArgs;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class GreenCheck
   {
      public static async Task CheckGreenTask(int executeSecond)
      {
         await Task.Run(async () =>
         {
            /*while (DateTime.Now.Second != executeSecond)
            {
               await Task.Delay(1000);
            }*/
            DiscordGuild guild = Bot.DiscordClient.GetGuildAsync(928930967140331590).Result;

            List<DiscordRole> roleCheckListNegativ = new();
            roleCheckListNegativ.Add(guild.GetRole(945023948645629973)); //Flagged        Flagged
            roleCheckListNegativ.Add(guild.GetRole(980071522427363368)); //Flagged +91    Flagged
            roleCheckListNegativ.Add(guild.GetRole(928934209484099615)); //#11ff11        Green
            roleCheckListNegativ.Add(guild.GetRole(928936283919745064)); //#6942ff        Purple
            roleCheckListNegativ.Add(guild.GetRole(929054038589317131)); //##e4e404       Yellow
            roleCheckListNegativ.Add(guild.GetRole(928936003551510549)); //#ff4269        Red
            roleCheckListNegativ.Add(guild.GetRole(928934609251627069)); //#226ace        Blue

            DiscordRole grey = guild.GetRole(928932796934799401); //#b1b1b1         Grey
            DiscordRole green = guild.GetRole(928934209484099615); //#11ff11        Green

            List<DiscordRole> roleCheckList = new();
            roleCheckList.Add(guild.GetRole(981575801214492752)); //⁣     ⁣  ⁣   Voice Channel Level             ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(949045273978630214)); //⁣Server Booster
            roleCheckList.Add(guild.GetRole(928955073285984268)); //⁣     ⁣  ⁣   Sympathie Rating             ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(949045281108922459)); //⁣Sympathie Rating 1
            roleCheckList.Add(guild.GetRole(949045284355342386)); //⁣Sympathie Rating 2
            roleCheckList.Add(guild.GetRole(949045286808981565)); //⁣Sympathie Rating 3
            roleCheckList.Add(guild.GetRole(949045289728233552)); //⁣Sympathie Rating 4
            roleCheckList.Add(guild.GetRole(949045292282544148)); //⁣Sympathie Rating 5
            roleCheckList.Add(guild.GetRole(928946804400193558)); //⁣     ⁣  ⁣   ⁣Über mich              ⁣   ⁣               ⁣
            roleCheckList.Add(guild.GetRole(945300974623391754)); //⁣     ⁣  ⁣   Geburtstag             ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(945300961679794226)); //⁣     ⁣  ⁣   Sternzeichen             ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(928947169719910421)); //⁣     ⁣  ⁣   Kanäle⁣               ⁣   ⁣               ⁣    ⁣
            roleCheckList.Add(guild.GetRole(928947030590644274)); //⁣     ⁣  ⁣   Benutzerdefiniert⁣⁣⁣              ⁣   ⁣
            roleCheckList.Add(guild.GetRole(928947119425982615)); //⁣     ⁣  ⁣   Spiele             ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(931950722256371752)); //⁣     ⁣  ⁣   Shooter             ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(931950729663492207)); //⁣     ⁣  ⁣   Survival             ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(931950732658237501)); //⁣     ⁣  ⁣   Andere Spiele              ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(939236265587515462)); //⁣     ⁣  ⁣   16P             ⁣   ⁣               ⁣      ⁣
            roleCheckList.Add(guild.GetRole(939216240512208956)); //⁣     ⁣  ⁣   BDSM             ⁣
            roleCheckList.Add(guild.GetRole(945007672577642537)); //⁣     ⁣  ⁣   Energy             ⁣   ⁣               ⁣      ⁣

            List<DiscordMember> discordMembers = guild.Members.Values.ToList();

            foreach (var discordMember in discordMembers)
            {
               if (discordMember.Id == 689780138157670420)
               {
                  var discordMemberRoles = discordMember.Roles;

                  List<DiscordRole> discordRoles = new();

                  foreach (DiscordRole role in discordMemberRoles)
                  {
                     if (!roleCheckList.Contains(role))
                        discordRoles.Add(role);
                  }

                  if (discordRoles.Contains(grey))
                  {
                     bool lever = true;

                     foreach (var role in discordRoles)
                     {
                        if (roleCheckListNegativ.Contains(role))
                           lever = false;
                     }
                     
                     if (discordRoles.Count > 2 && lever)
                     {
                        await discordMember.GrantRoleAsync(green);
                        await discordMember.RevokeRoleAsync(grey);
                     }
                  }
               }
            }

            await Task.Delay(1000);
         });
      }
   }
}
