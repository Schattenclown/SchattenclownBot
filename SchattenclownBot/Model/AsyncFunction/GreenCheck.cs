using DisCatSharp.Entities;
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
            while (true)
            {
               while (DateTime.Now.Second != executeSecond)
               {
                  await Task.Delay(1000);
               }

               //hier muss noch

               DiscordGuild guild = Bot.DiscordClient.GetGuildAsync(928930967140331590).Result;

               /*List<DiscordRole> onetime = guild.Roles.Values.ToList();
               DiscordColor invis = new DiscordColor("#292b2f");
               DiscordColor newColor = new DiscordColor("#000000");
               foreach (var role in onetime)
               {
                  if(role.Color.Value == invis.Value)
                  {
                     role.ModifyAsync(x => x.Color = newColor);
                  }
               }*/

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
               roleCheckList.Add(guild.GetRole(945007672577642537)); //⁣     ⁣  ⁣   Energy⁣   ⁣               ⁣      ⁣

               roleCheckList.Add(guild.GetRole(11023549473768611861)); //zehner 0
               roleCheckList.Add(guild.GetRole(1010251754270642218)); //zehner 1
               roleCheckList.Add(guild.GetRole(1001177749207126106)); //zehner 2
               roleCheckList.Add(guild.GetRole(995805285383938098)); //zehner 3
               roleCheckList.Add(guild.GetRole(993902906417889432)); //zehner 4
               roleCheckList.Add(guild.GetRole(986332993528426546)); //zehner 5
               roleCheckList.Add(guild.GetRole(983134660169195600)); //zehner 6
               roleCheckList.Add(guild.GetRole(981715147263467622)); //zehner 7
               roleCheckList.Add(guild.GetRole(1015272139051507805)); //zehner 8
               roleCheckList.Add(guild.GetRole(1009772791563825183)); //zehner 9

               roleCheckList.Add(guild.GetRole(981695815053631558)); //zehner  1
               roleCheckList.Add(guild.GetRole(981715121866960917)); //zehner  2
               roleCheckList.Add(guild.GetRole(1020780813282975816)); //zehner  3
               roleCheckList.Add(guild.GetRole(1016418457597784196)); //zehner  4
               roleCheckList.Add(guild.GetRole(1012411021262073949)); //zehner  5
               roleCheckList.Add(guild.GetRole(1004817444604498020)); //zehner  6
               roleCheckList.Add(guild.GetRole(1001555701308604536)); //zehner  7
               roleCheckList.Add(guild.GetRole(981630890876764291)); //zehner  8
               roleCheckList.Add(guild.GetRole(993902853959712769)); //zehner  9
               roleCheckList.Add(guild.GetRole(981626330007347220)); //zehner  0

               roleCheckList.Add(guild.GetRole(1017937277307064340)); //level


               List<DiscordMember> discordMembers = guild.Members.Values.ToList();

               foreach (DiscordMember discordMember in discordMembers)
               {
                  IEnumerable<DiscordRole> discordMemberRoles = discordMember.Roles;

                  List<DiscordRole> discordRoles = new();

                  foreach (DiscordRole role in discordMemberRoles)
                  {
                     if (!roleCheckList.Contains(role))
                        discordRoles.Add(role);
                  }

                  if (discordRoles.Contains(grey))
                  {
                     bool lever = true;

                     foreach (DiscordRole role in discordRoles)
                     {
                        if (roleCheckListNegativ.Contains(role))
                           lever = false;
                     }

                     if (discordRoles.Count > 2 && lever)
                     {
                        await discordMember.GrantRoleAsync(green);
                        await discordMember.RevokeRoleAsync(grey);
                        CWLogger.Write(discordMember.DisplayName + " Granted Green", "INFO", "GreenCheck.cs", ConsoleColor.Cyan);
                     }
                  }
               }

               await Task.Delay(1000);
            }
         });
      }
   }
}
