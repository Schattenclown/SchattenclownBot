using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Persistence.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Objects
{
    public class UserLevelSystem
   {
      public ulong MemberId { get; set; }
      public ulong GuildId { get; set; }
      public int OnlineTicks { get; set; }
      public TimeSpan OnlineTime { get; set; }
      public double VoteRatingAvg { get; set; }

      public static List<UserLevelSystem> Read(ulong guildId)
      {
         return DB_UserLevelSystem.Read(guildId);
      }
      public static void Add(ulong guildId, UserLevelSystem userLevelSystem)
      {
         DB_UserLevelSystem.Add(guildId, userLevelSystem);
      }
      public static void Change(ulong guildId, UserLevelSystem userLevelSystem)
      {
         DB_UserLevelSystem.Change(guildId, userLevelSystem);
      }
      public static void CreateTable_UserLevelSystem(ulong guildId)
      {
         DB_UserLevelSystem.CreateTable_UserLevelSystem(guildId);
      }
      public static int CalculateLevel(int onlineTicks)
      {
         double returnInt = 0.69 * Math.Pow(onlineTicks, 0.38);
         double returnIntRounded = Math.Round(returnInt, MidpointRounding.ToNegativeInfinity);
         return Convert.ToInt32(returnIntRounded);
      }
      public static int CalculateXpOverCurrentLevel(int onlineTicks)
      {
         int level = CalculateLevel(onlineTicks);

         double xpToReachThisLevel = Math.Pow(level / 0.69, 1 / 0.38);
         int calculatedXpOverCurrentLevel = onlineTicks - Convert.ToInt32(xpToReachThisLevel);

         return calculatedXpOverCurrentLevel;
      }
      public static int CalculateXpSpanToReachNextLevel(int onlineTicks)
      {
         int level = CalculateLevel(onlineTicks);

         double xpToReachThisLevel = Math.Pow(level / 0.69, 1 / 0.38);
         double xpToReachNextLevel = Math.Pow((level + 1) / 0.69, 1 / 0.38);
         int xpSpanToReachNextLevel = Convert.ToInt32(xpToReachNextLevel) - Convert.ToInt32(xpToReachThisLevel);

         return xpSpanToReachNextLevel;
      }
      public static void LevelSystemRunAsync(int executeSecond)
      {
         bool levelSystemVirgin = true;

         Task.Run(async () =>
         {
            while (DateTime.Now.Second != executeSecond)
            {
               await Task.Delay(1000);
            }

            do
            {
               if (Bot.DiscordClient.Guilds.ToList().Count != 0)
               {
                  if (levelSystemVirgin)
                  {
                     List<KeyValuePair<ulong, DiscordGuild>> guildsList = Bot.DiscordClient.Guilds.ToList();
                     foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
                     {
                        UserLevelSystem.CreateTable_UserLevelSystem(guildItem.Value.Id);
                     }
                     levelSystemVirgin = false;
                  }
               }
               await Task.Delay(1000);
            } while (levelSystemVirgin);

            while (true)
            {
               while (DateTime.Now.Second != executeSecond)
               {
                  await Task.Delay(1000);
               }

               List<KeyValuePair<ulong, DiscordGuild>> guildsList = Bot.DiscordClient.Guilds.ToList();
               foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
               {
                  List<UserLevelSystem> userLevelSystemList;
                  userLevelSystemList = UserLevelSystem.Read(guildItem.Value.Id);

                  IReadOnlyDictionary<ulong, DiscordMember> guildMembers = guildItem.Value.Members;
                  foreach (KeyValuePair<ulong, DiscordMember> memberItem in guildMembers)
                  {
                     if (memberItem.Value.VoiceState != null && !memberItem.Value.VoiceState.IsSelfDeafened && !memberItem.Value.VoiceState.IsSuppressed && !memberItem.Value.IsBot)
                     {
                        UserLevelSystem userLevelSystemObj = new();
                        userLevelSystemObj.MemberId = memberItem.Value.Id;
                        userLevelSystemObj.OnlineTicks = 0;
                        bool found = false;

                        foreach (UserLevelSystem userLevelSystemItem in userLevelSystemList)
                        {
                           if (memberItem.Value.Id == userLevelSystemItem.MemberId)
                           {
                              userLevelSystemObj.OnlineTicks = userLevelSystemItem.OnlineTicks;
                              found = true;
                              break;
                           }
                        }

                        if (found)
                        {
                           userLevelSystemObj.OnlineTicks++;
                           UserLevelSystem.Change(guildItem.Value.Id, userLevelSystemObj);
                        }

                        if (!found)
                        {
                           DateTime date1 = new(1969, 4, 20, 4, 20, 0);
                           DateTime date2 = new(1969, 4, 20, 4, 21, 0);
                           TimeSpan timeSpan = date2 - date1;
                           userLevelSystemObj.OnlineTime = timeSpan;
                           userLevelSystemObj.OnlineTicks = 1;
                           UserLevelSystem.Add(guildItem.Value.Id, userLevelSystemObj);
                        }
                     }
                  }
               }
               await Task.Delay(2000);
               if (!AsyncFunction.LastMinuteCheck.LevelSystemRunAsync)
                  AsyncFunction.LastMinuteCheck.LevelSystemRunAsync = true;
            }
            // ReSharper disable once FunctionNeverReturns
         });
      }
      public static void LevelSystemRoleDistributionRunAsync(int executeSecond)
      {
         Task.Run(async () =>
         {
            while (DateTime.Now.Second != executeSecond)
            {
               await Task.Delay(1000);
            }

            bool levelSystemRoleDistributionVirgin = true;
            DiscordGuild guildObj = null;

            while (DateTime.Now.Second != executeSecond)
            {
               await Task.Delay(1000);
            }

            do
            {
               if (Bot.DiscordClient.Guilds.ToList().Count != 0)
               {
                  if (levelSystemRoleDistributionVirgin)
                  {
                     List<KeyValuePair<ulong, DiscordGuild>> guildsList = Bot.DiscordClient.Guilds.ToList();
                     foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList.Where(guiltItem => guiltItem.Value.Id == 928930967140331590))
                     {
                        guildObj = Bot.DiscordClient.GetGuildAsync(guildItem.Value.Id).Result;
                     }
                     levelSystemRoleDistributionVirgin = false;
                  }
               }
               await Task.Delay(1000);
            } while (levelSystemRoleDistributionVirgin);

            // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
            while (guildObj != null)
            {
               while (DateTime.Now.Second != executeSecond)
               {
                  await Task.Delay(1000);
               }

               try
               {
                  //Create List where all users are listed.
                  List<UserLevelSystem> userLevelSystemList = UserLevelSystem.Read(guildObj.Id);
                  //Order the list by online ticks.
                  List<UserLevelSystem> userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
                  userLevelSystemListSorted.Reverse();

                  List<DiscordMember> guildMemberList = guildObj.Members.Values.ToList();

                  List<UserLevelSystem> userLevelSystemListSortedOut = guildMemberList.SelectMany(guildMemberItem => userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.MemberId == guildMemberItem.Id)).ToList();
                  userLevelSystemListSortedOut = userLevelSystemListSortedOut.OrderBy(x => x.OnlineTicks).ToList();

                  /*zehnerRoles.Add(guildObj.GetRole(1023523454105952347)); //zehner 1
                  zehnerRoles.Add(guildObj.GetRole(1023523457704665098)); //zehner 2
                  zehnerRoles.Add(guildObj.GetRole(1023523458870681660)); //zehner 3
                  zehnerRoles.Add(guildObj.GetRole(1023523459504025660)); //zehner 4
                  zehnerRoles.Add(guildObj.GetRole(1023523460120576041)); //zehner 5
                  zehnerRoles.Add(guildObj.GetRole(1023523460816830534)); //zehner 6
                  zehnerRoles.Add(guildObj.GetRole(1023523461504708648)); //zehner 7
                  zehnerRoles.Add(guildObj.GetRole(1023523461848649789)); //zehner 8
                  zehnerRoles.Add(guildObj.GetRole(1023523462817517658)); //zehner 9

                  einerRoles.Add(guildObj.GetRole(1023523571890397254)); //zehner  1
                  einerRoles.Add(guildObj.GetRole(1023523571252875355)); //zehner  2
                  einerRoles.Add(guildObj.GetRole(1023523570766331904)); //zehner  3
                  einerRoles.Add(guildObj.GetRole(1023523569533206569)); //zehner  4
                  einerRoles.Add(guildObj.GetRole(1023523568937607280)); //zehner  5
                  einerRoles.Add(guildObj.GetRole(1023523568690147388)); //zehner  6
                  einerRoles.Add(guildObj.GetRole(1023523568002285668)); //zehner  7
                  einerRoles.Add(guildObj.GetRole(1023523566911766618)); //zehner  8
                  einerRoles.Add(guildObj.GetRole(1023523464834994216)); //zehner  9
                  einerRoles.Add(guildObj.GetRole(1023523464017096716)); //zehner  0*/
                  List<DiscordRole> zehnerRolesOrg = new()
                  {
                     guildObj.GetRole(1010251754270642218), //zehner 1
                     guildObj.GetRole(1001177749207126106), //zehner 2
                     guildObj.GetRole(995805285383938098), //zehner 3
                     guildObj.GetRole(993902906417889432), //zehner 4
                     guildObj.GetRole(986332993528426546), //zehner 5
                     guildObj.GetRole(983134660169195600), //zehner 6
                     guildObj.GetRole(981715147263467622), //zehner 7
                     guildObj.GetRole(1015272139051507805), //zehner 8
                     guildObj.GetRole(1009772791563825183) //zehner 9
                  };

                  List<DiscordRole> einerRolesOrg = new()
                  {
                     guildObj.GetRole(981695815053631558), //zehner  1
                     guildObj.GetRole(981715121866960917), //zehner  2
                     guildObj.GetRole(1020780813282975816), //zehner  3
                     guildObj.GetRole(1016418457597784196), //zehner  4
                     guildObj.GetRole(1012411021262073949), //zehner  5
                     guildObj.GetRole(1004817444604498020), //zehner  6
                     guildObj.GetRole(1001555701308604536), //zehner  7
                     guildObj.GetRole(981630890876764291), //zehner  8
                     guildObj.GetRole(993902853959712769), //zehner  9
                     guildObj.GetRole(981626330007347220) //zehner  0
                  };

                  foreach (UserLevelSystem userLevelSystemItem in userLevelSystemListSortedOut)
                  {
                     if (userLevelSystemItem.MemberId is not 304366130238193664 and not 523765246104567808)
                     {
                        List<DiscordRole> einerRoles = new(einerRolesOrg);
                        List<DiscordRole> zehnerRoles = new(zehnerRolesOrg);
                        DiscordMember discordMember = guildObj.GetMemberAsync(userLevelSystemItem.MemberId).Result;

                        int totalLevel = CalculateLevel(userLevelSystemItem.OnlineTicks);
                        string zehnerString = "";
                        string einerString = "";

                        if (totalLevel >= 10)
                           zehnerString = Convert.ToString(totalLevel / 10);

                        einerString = (totalLevel % 10) switch
                        {
                           0 => "0",
                           1 => "1",
                           2 => "2",
                           3 => "3",
                           4 => "4",
                           5 => "5",
                           6 => "6",
                           7 => "7",
                           8 => "8",
                           9 => "9",
                           _ => einerString
                        };

                        if (zehnerString != "")
                        {
                           DiscordRole zehnerRole = zehnerRoles.Find(x => x.Name == zehnerString);
                           zehnerRoles.Remove(zehnerRole);

                           if (!discordMember.Roles.Contains(zehnerRole))
                           {
                              await discordMember.GrantRoleAsync(zehnerRole);

                              CwLogger.Write($"Granted {discordMember.DisplayName} MemberID Level {totalLevel} --- {discordMember.Id} Role {zehnerRole.Id} {zehnerRole.Name}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
                           }
                        }

                        if (einerString != "")
                        {
                           DiscordRole einerRole = einerRoles.Find(x => x.Name == einerString);
                           einerRoles.Remove(einerRole);

                           if (!discordMember.Roles.Contains(einerRole))
                           {
                              await discordMember.GrantRoleAsync(einerRole);

                              CwLogger.Write($"Granted {discordMember.DisplayName} MemberID Level {totalLevel} --- {discordMember.Id} Role {einerRole.Id} {einerRole.Name}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
                           }
                        }

                        foreach (DiscordRole revokeRoleItem in discordMember.Roles.ToList().Where(x => einerRoles.Contains(x) || zehnerRoles.Contains(x)))
                        {
                           await discordMember.RevokeRoleAsync(revokeRoleItem);

                           CwLogger.Write($"Removed {discordMember.DisplayName} MemberID {discordMember.Id} Role {revokeRoleItem.Id} {revokeRoleItem.Name}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
                        }

                        DiscordRole discordLevelRole = guildObj.GetRole(1017937277307064340);
                        if (!discordMember.Roles.Contains(discordLevelRole))
                        {
                           await discordMember.GrantRoleAsync(discordLevelRole);
                        }
                     }
                  }
                  CwLogger.Write("Finished", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
               }
               catch (Exception ex)
               {
                  CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
               }
               await Task.Delay(2000);
               if (!AsyncFunction.LastMinuteCheck.LevelSystemRoleDistributionRunAsync)
                  AsyncFunction.LastMinuteCheck.LevelSystemRoleDistributionRunAsync = true;
            }
         });
      }
   }
}
