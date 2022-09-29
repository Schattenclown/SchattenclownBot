using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
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
      private const string RoleChannelLevelString = "Voice Channel Level";

      public static List<UserLevelSystem> Read(ulong guildId)
      {
         return DbUserLevelSystem.Read(guildId);
      }
      public static void Add(ulong guildId, UserLevelSystem userLevelSystem)
      {
         DbUserLevelSystem.Add(guildId, userLevelSystem);
      }
      public static void Change(ulong guildId, UserLevelSystem userLevelSystem)
      {
         DbUserLevelSystem.Change(guildId, userLevelSystem);
      }
      public static void CreateTable_UserLevelSystem(ulong guildId)
      {
         DbUserLevelSystem.CreateTable_UserLevelSystem(guildId);
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
      public static async Task LevelSystemRunAsync(int executeSecond)
      {
         bool levelSystemVirgin = true;

         await Task.Run(async () =>
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
            }
            // ReSharper disable once FunctionNeverReturns
         });
      }
      public static async Task LevelSystemRoleDistributionRunAsync(int executeSecond)
      {
         while (DateTime.Now.Second != executeSecond)
         {
            await Task.Delay(1000);
         }

         bool levelSystemRoleDistributionVirgin = true;
         DiscordGuild guildObj = null;

         await Task.Run(async () =>
         {
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
                  List<DiscordRole> zehnerRolesOrg = new();
                  List<DiscordRole> einerRolesOrg = new();

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

                  zehnerRolesOrg.Add(guildObj.GetRole(1010251754270642218)); //zehner 1
                  zehnerRolesOrg.Add(guildObj.GetRole(1001177749207126106)); //zehner 2
                  zehnerRolesOrg.Add(guildObj.GetRole(995805285383938098)); //zehner 3
                  zehnerRolesOrg.Add(guildObj.GetRole(993902906417889432)); //zehner 4
                  zehnerRolesOrg.Add(guildObj.GetRole(986332993528426546)); //zehner 5
                  zehnerRolesOrg.Add(guildObj.GetRole(983134660169195600)); //zehner 6
                  zehnerRolesOrg.Add(guildObj.GetRole(981715147263467622)); //zehner 7
                  zehnerRolesOrg.Add(guildObj.GetRole(1015272139051507805)); //zehner 8
                  zehnerRolesOrg.Add(guildObj.GetRole(1009772791563825183)); //zehner 9

                  einerRolesOrg.Add(guildObj.GetRole(981695815053631558)); //zehner  1
                  einerRolesOrg.Add(guildObj.GetRole(981715121866960917)); //zehner  2
                  einerRolesOrg.Add(guildObj.GetRole(1020780813282975816)); //zehner  3
                  einerRolesOrg.Add(guildObj.GetRole(1016418457597784196)); //zehner  4
                  einerRolesOrg.Add(guildObj.GetRole(1012411021262073949)); //zehner  5
                  einerRolesOrg.Add(guildObj.GetRole(1004817444604498020)); //zehner  6
                  einerRolesOrg.Add(guildObj.GetRole(1001555701308604536)); //zehner  7
                  einerRolesOrg.Add(guildObj.GetRole(981630890876764291)); //zehner  8
                  einerRolesOrg.Add(guildObj.GetRole(993902853959712769)); //zehner  9
                  einerRolesOrg.Add(guildObj.GetRole(981626330007347220)); //zehner  0
                  string all = "";
                  foreach (UserLevelSystem userLevelSystemItem in userLevelSystemListSortedOut)
                  {
                     if (userLevelSystemItem.MemberId is not 304366130238193664 and not 523765246104567808)
                     {
                        List<DiscordRole> einerRoles = new(einerRolesOrg);
                        List<DiscordRole> zehnerRoles = new(zehnerRolesOrg);
                        DiscordMember discordMember = guildObj.GetMemberAsync(userLevelSystemItem.MemberId).Result;

                        int totalLevel = CalculateLevel(userLevelSystemItem.OnlineTicks);
                        all += discordMember.DisplayName + " " + totalLevel + " ; ";
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

                              CWLogger.Write($"Granted {discordMember.DisplayName} MemberID Level {totalLevel} --- {discordMember.Id} Role {zehnerRole.Id} {zehnerRole.Name}", "INFO", "UserLevelSystem.cs", ConsoleColor.Yellow);
                           }
                        }

                        if (einerString != "")
                        {
                           DiscordRole einerRole = einerRoles.Find(x => x.Name == einerString);
                           einerRoles.Remove(einerRole);

                           if (!discordMember.Roles.Contains(einerRole))
                           {
                              await discordMember.GrantRoleAsync(einerRole);

                              CWLogger.Write($"Granted {discordMember.DisplayName} MemberID Level {totalLevel} --- {discordMember.Id} Role {einerRole.Id} {einerRole.Name}", "INFO", "UserLevelSystem.cs", ConsoleColor.Yellow);
                           }
                        }

                        foreach (DiscordRole revokeRoleItem in discordMember.Roles.ToList().Where(x => einerRoles.Contains(x) || zehnerRoles.Contains(x)))
                        {
                           await discordMember.RevokeRoleAsync(revokeRoleItem);

                           CWLogger.Write($"Removed {discordMember.DisplayName} MemberID {discordMember.Id} Role {revokeRoleItem.Id} {revokeRoleItem.Name}", "INFO", "UserLevelSystem.cs", ConsoleColor.Yellow);
                        }

                        DiscordRole discordLevelRole = guildObj.GetRole(1017937277307064340);
                        if (!discordMember.Roles.Contains(discordLevelRole))
                        {
                           await discordMember.GrantRoleAsync(discordLevelRole);
                        }
                     }
                  }
                  CWLogger.Write("Finished UserLevelSystem", "INFO", "UserLevelSystem.cs", ConsoleColor.Cyan);
               }
               catch (Exception e)
               {
                  CWLogger.Write(e.Message, "EXCEPTION", "UserLevelSystem.cs", ConsoleColor.Red);
               }
               await Task.Delay(2000);
            }
         });
      }

      public static async Task LevelSystemRoleDistributionRunAsyncOLD(int executeSecond)
      {
         while (DateTime.Now.Second != executeSecond)
         {
            await Task.Delay(1000);
         }

         bool levelSystemRoleDistributionVirgin = true;
         DiscordGuild guildObj = null;
         bool sortLevelSystemRolesBool = false;
         const int delayInMs = 200;

         await Task.Run(async () =>
         {
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

               //Create List where all users are listed.
               List<UserLevelSystem> userLevelSystemList = UserLevelSystem.Read(guildObj.Id);
               //Order the list by online ticks.
               List<UserLevelSystem> userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
               userLevelSystemListSorted.Reverse();

               List<DiscordMember> guildMemberList = guildObj.Members.Values.ToList();

               List<UserLevelSystem> userLevelSystemListSortedOut = guildMemberList.SelectMany(guildMemberItem => userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.MemberId == guildMemberItem.Id)).ToList();
               userLevelSystemListSortedOut = userLevelSystemListSortedOut.OrderBy(x => x.OnlineTicks).ToList();
               List<DiscordRole> discordRoleList = guildObj.Roles.Values.ToList();

               foreach (UserLevelSystem userLevelSystemItem in userLevelSystemListSortedOut)
               {
                  if (userLevelSystemItem.MemberId is not 304366130238193664 and not 523765246104567808)
                  {
                     //Get the discord user by ID.
                     DiscordMember discordMember = guildObj.GetMemberAsync(userLevelSystemItem.MemberId).Result;
                     await Task.Delay(delayInMs);

                     DiscordRole discordRoleObj = null;
                     int roleIndex = guildObj.GetRole(981575801214492752).Position - 1;
                     await Task.Delay(delayInMs);

                     int totalLevel = CalculateLevel(userLevelSystemItem.OnlineTicks);

                     string voiceChannelLevelString = $"{RoleChannelLevelString} {totalLevel}";
                     bool roleExists = false;


                     foreach (DiscordRole discordRoleItem in discordRoleList.Where(role => role.Name == voiceChannelLevelString))
                     {
                        discordRoleObj = discordRoleItem;
                        roleExists = true;
                        break;
                     }

                     if (!roleExists)
                     {
                        discordRoleObj = await guildObj.CreateRoleAsync(voiceChannelLevelString, permissions: Permissions.None, DiscordColor.None);
                        await Task.Delay(delayInMs);
                        await discordRoleObj.ModifyPositionAsync(roleIndex);
                        await Task.Delay(delayInMs);
                        discordRoleList = guildObj.Roles.Values.ToList();
                        await Task.Delay(delayInMs);
                     }

                     List<DiscordRole> discordMemberRoleList = discordMember.Roles.ToList();
                     await Task.Delay(delayInMs);

                     foreach (DiscordRole revokeRoleItem in discordMemberRoleList.Where(revokeRoleItem => revokeRoleItem.Name.Contains(RoleChannelLevelString) && revokeRoleItem.Name != voiceChannelLevelString))
                     {
                        if (revokeRoleItem.Id != 981575801214492752)
                        {
                           await discordMember.RevokeRoleAsync(revokeRoleItem);
                           await Task.Delay(delayInMs);
                        }
                     }

                     if (!discordMember.Roles.Contains(discordRoleObj))
                     {
                        await discordMember.GrantRoleAsync(discordRoleObj);
                        await Task.Delay(delayInMs);
                     }

                     await Task.Delay(2000);
                  }

                  if (sortLevelSystemRolesBool == false)
                  {
#pragma warning disable CS4014
                     UserLevelSystem.SortLevelSystemRolesRunAsync(49);
#pragma warning restore CS4014
                     sortLevelSystemRolesBool = true;
                  }

                  await Task.Delay(2000);
               }
            }
         });
      }
      public static async Task SortLevelSystemRolesRunAsync(int executeSecond)
      {
         bool levelSystemRoleDistributionVirgin = true;
         DiscordGuild guildObj = null;

         await Task.Run(async () =>
         {
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

            while (true)
            {
               while (DateTime.Now.Second != executeSecond)
               {
                  await Task.Delay(1000);
               }

               if (guildObj != null)
               {
                  List<DiscordRole> discordRoleList = guildObj.Roles.Values.ToList();
                  List<KeyValuePair<int, DiscordRole>> discordRoleListSortedOut = new();

                  foreach (DiscordRole discordRoleItem in discordRoleList.Where(discordRoleItem => discordRoleItem.Name.Contains(RoleChannelLevelString)))
                  {
                     if (discordRoleItem.Id != 981575801214492752)
                     {
                        string roleLevelString = discordRoleItem.Name.Substring(RoleChannelLevelString.Length + 1);
                        int roleLevel = Convert.ToInt32(roleLevelString);
                        KeyValuePair<int, DiscordRole> keyValuePair = new(roleLevel, discordRoleItem);
                        discordRoleListSortedOut.Add(keyValuePair);
                     }
                  }

                  List<KeyValuePair<int, DiscordRole>> discordRoleListSortedOutOrdered = discordRoleListSortedOut.OrderBy(x => x.Key).ToList();
                  discordRoleListSortedOutOrdered.Reverse();

                  int discordRolesInt = guildObj.Roles.Count;
                  int roleIndex = guildObj.GetRole(981575801214492752).Position - 1;
                  int index = 0;

                  foreach (KeyValuePair<int, DiscordRole> discordRoleItem in discordRoleListSortedOutOrdered)
                  {
                     if (discordRolesInt != guildObj.Roles.Count)
                        break;

                     int newRoleIndex = roleIndex - index;
                     if (discordRoleItem.Value.Position != newRoleIndex)
                        await discordRoleItem.Value.ModifyPositionAsync(newRoleIndex);
                     await Task.Delay(2000);
                     index++;
                  }
               }

               await Task.Delay(2000);
            }
            // ReSharper disable once FunctionNeverReturns
         });
      }
   }
}
