using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Persistence.DatabaseAccess;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Models
{
    public class UserLevelSystem
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public ulong DiscordMemberID { get; set; }

        [Required]
        public ulong DiscordGuildID { get; set; }

        [Required]
        public int OnlineTicks { get; set; }

        public override string ToString()
        {
            return $"ID: {ID} DiscordMemberID: {DiscordMemberID} DiscordGuildID: {DiscordGuildID} OnlineTicks: {OnlineTicks}";
        }

        public void AddOrUpdate(UserLevelSystem userLevelSystem)
        {
            new UserLevelSystemDBA().AddOrUpdate(userLevelSystem);
        }

        public List<UserLevelSystem> GetByGuildId(ulong guildId)
        {
            return new UserLevelSystemDBA().GetByGuildId(guildId);
        }

        public int CalculateLevel(int onlineTicks)
        {
            double returnInt = 0.69 * Math.Pow(onlineTicks, 0.38);
            double returnIntRounded = Math.Round(returnInt, MidpointRounding.ToNegativeInfinity);
            return Convert.ToInt32(returnIntRounded);
        }

        public int CalculateXpOverCurrentLevel(int onlineTicks)
        {
            int level = CalculateLevel(onlineTicks);

            double xpToReachThisLevel = Math.Pow(level / 0.69, 1 / 0.38);
            int calculatedXpOverCurrentLevel = onlineTicks - Convert.ToInt32(xpToReachThisLevel);

            return calculatedXpOverCurrentLevel;
        }

        public int CalculateXpSpanToReachNextLevel(int onlineTicks)
        {
            int level = CalculateLevel(onlineTicks);

            double xpToReachThisLevel = Math.Pow(level / 0.69, 1 / 0.38);
            double xpToReachNextLevel = Math.Pow((level + 1) / 0.69, 1 / 0.38);
            int xpSpanToReachNextLevel = Convert.ToInt32(xpToReachNextLevel) - Convert.ToInt32(xpToReachThisLevel);

            return xpSpanToReachNextLevel;
        }

        public void LevelSystemRunAsync(int executeSecond)
        {
            new CustomLogger().Information("Starting LevelSystem...", ConsoleColor.Green);

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(executeSecond - DateTime.Now.Second % executeSecond));

                    List<KeyValuePair<ulong, DiscordGuild>> guildsList = DiscordBot.DiscordClient.Guilds.ToList();
                    foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
                    {
                        List<UserLevelSystem> userLevelSystemList = new UserLevelSystem().GetByGuildId(guildItem.Value.Id);

                        IReadOnlyDictionary<ulong, DiscordMember> guildMembers = guildItem.Value.Members;
                        foreach (KeyValuePair<ulong, DiscordMember> memberItem in guildMembers)
                        {
                            if (memberItem.Value.VoiceState is { IsSelfDeafened: false, IsSuppressed: false } && !memberItem.Value.IsBot)
                            {
                                UserLevelSystem userLevelSystemObj = new()
                                {
                                            DiscordGuildID = guildItem.Value.Id,
                                            DiscordMemberID = memberItem.Value.Id,
                                            OnlineTicks = 0
                                };
                                bool found = false;

                                foreach (UserLevelSystem userLevelSystemItem in userLevelSystemList)
                                {
                                    if (memberItem.Value.Id == userLevelSystemItem.DiscordMemberID)
                                    {
                                        userLevelSystemObj.OnlineTicks = userLevelSystemItem.OnlineTicks;
                                        found = true;
                                        break;
                                    }
                                }

                                if (found)
                                {
                                    userLevelSystemObj.OnlineTicks++;
                                    new UserLevelSystem().AddOrUpdate(userLevelSystemObj);
                                }

                                if (!found)
                                {
                                    userLevelSystemObj.OnlineTicks = 1;
                                    new UserLevelSystem().AddOrUpdate(userLevelSystemObj);
                                }
                            }
                        }
                    }

                    await Task.Delay(2000);
                    if (!LastMinuteCheck.LevelSystemRunAsync)
                    {
                        LastMinuteCheck.LevelSystemRunAsync = true;
                    }
                }
                // ReSharper disable once FunctionNeverReturns
            });
        }

        public void LevelSystemRoleDistributionRunAsync(int executeSecond)
        {
            new CustomLogger().Information("Starting LevelSystemRoleDistribution...", ConsoleColor.Green);
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(executeSecond - DateTime.Now.Second % executeSecond));

                bool levelSystemRoleDistributionVirgin = true;
                DiscordGuild guildObj = null;

                await Task.Delay(TimeSpan.FromSeconds(executeSecond - DateTime.Now.Second % executeSecond));

                do
                {
                    if (DiscordBot.DiscordClient.Guilds.ToList().Count != 0)
                    {
                        List<KeyValuePair<ulong, DiscordGuild>> guildsList = DiscordBot.DiscordClient.Guilds.ToList();
                        foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList.Where(guiltItem => guiltItem.Value.Id == 928930967140331590))
                        {
                            guildObj = DiscordBot.DiscordClient.GetGuildAsync(guildItem.Value.Id).Result;
                        }

                        levelSystemRoleDistributionVirgin = false;
                    }

                    await Task.Delay(1000);
                } while (levelSystemRoleDistributionVirgin);

                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                while (guildObj != null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(executeSecond - DateTime.Now.Second % executeSecond));

                    try
                    {
                        //Create List where all users are listed.
                        List<UserLevelSystem> userLevelSystemList = new UserLevelSystem().GetByGuildId(guildObj.Id);
                        //Order the list by online ticks.
                        List<UserLevelSystem> userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
                        userLevelSystemListSorted.Reverse();

                        List<DiscordMember> guildMemberList = guildObj.Members.Values.ToList();

                        List<UserLevelSystem> userLevelSystemListSortedOut = guildMemberList.SelectMany(guildMemberItem => userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.DiscordMemberID == guildMemberItem.Id)).ToList();
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
                            if (userLevelSystemItem.DiscordMemberID is not 304366130238193664 and not 523765246104567808)
                            {
                                List<DiscordRole> einerRoles = new(einerRolesOrg);
                                List<DiscordRole> zehnerRoles = new(zehnerRolesOrg);
                                DiscordMember discordMember = guildObj.GetMemberAsync(userLevelSystemItem.DiscordMemberID).Result;

                                int totalLevel = CalculateLevel(userLevelSystemItem.OnlineTicks);
                                string zehnerString = "";
                                string einerString = "";

                                if (totalLevel >= 10)
                                {
                                    zehnerString = Convert.ToString(totalLevel / 10);
                                }

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

                                        new CustomLogger().Information($"Granted {discordMember.DisplayName} DiscordMemberID Level {totalLevel} --- {discordMember.Id} Role {zehnerRole.Id} {zehnerRole.Name}", ConsoleColor.Green);
                                    }
                                }

                                if (einerString != "")
                                {
                                    DiscordRole einerRole = einerRoles.Find(x => x.Name == einerString);
                                    einerRoles.Remove(einerRole);

                                    if (!discordMember.Roles.Contains(einerRole))
                                    {
                                        await discordMember.GrantRoleAsync(einerRole);

                                        new CustomLogger().Information($"Granted {discordMember.DisplayName} DiscordMemberID Level {totalLevel} --- {discordMember.Id} Role {einerRole.Id} {einerRole.Name}", ConsoleColor.Green);
                                    }
                                }

                                foreach (DiscordRole revokeRoleItem in discordMember.Roles.ToList().Where(x => einerRoles.Contains(x) || zehnerRoles.Contains(x)))
                                {
                                    await discordMember.RevokeRoleAsync(revokeRoleItem);

                                    new CustomLogger().Information($"Removed {discordMember.DisplayName} DiscordMemberID {discordMember.Id} Role {revokeRoleItem.Id} {revokeRoleItem.Name}", ConsoleColor.Green);
                                }

                                DiscordRole discordLevelRole = guildObj.GetRole(1017937277307064340);
                                if (!discordMember.Roles.Contains(discordLevelRole))
                                {
                                    await discordMember.GrantRoleAsync(discordLevelRole);
                                }
                            }
                        }

                        new CustomLogger().Information("Finished", ConsoleColor.Green);
                    }
                    catch (Exception ex)
                    {
                        new CustomLogger().Error(ex);
                    }

                    await Task.Delay(2000);
                    if (!LastMinuteCheck.LevelSystemRoleDistributionRunAsync)
                    {
                        LastMinuteCheck.LevelSystemRoleDistributionRunAsync = true;
                    }
                }
            });
        }

        public void BrixLevelSystemRoleDistributionRunAsync(int executeSecond)
        {
            new CustomLogger().Information("Starting BrixLevelSystemRoleDistribution...", ConsoleColor.Green);
            Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(executeSecond - DateTime.Now.Second % executeSecond));

                DiscordGuild guildObj = await DiscordBot.DiscordClient.GetGuildAsync(914177901178544148);

                // ReSharper disable once LoopVariableIsNeverChangedInsideLoop
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                while (guildObj != null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(executeSecond - DateTime.Now.Second % executeSecond));

                    try
                    {
                        //Create List where all users are listed.
                        List<UserLevelSystem> userLevelSystemList = new UserLevelSystem().GetByGuildId(guildObj.Id);
                        //Order the list by online ticks.
                        List<UserLevelSystem> userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
                        userLevelSystemListSorted.Reverse();

                        List<DiscordMember> guildMemberList = guildObj.Members.Values.ToList();

                        List<UserLevelSystem> userLevelSystemListSortedOut = guildMemberList.SelectMany(guildMemberItem => userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.DiscordMemberID == guildMemberItem.Id)).ToList();
                        userLevelSystemListSortedOut = userLevelSystemListSortedOut.OrderBy(x => x.OnlineTicks).ToList();

                        List<KeyValuePair<string, DiscordRole>> levelRoles = new()
                        {
                                    new KeyValuePair<string, DiscordRole>("bronze", guildObj.GetRole(1176101767168139304)),
                                    new KeyValuePair<string, DiscordRole>("silber", guildObj.GetRole(1176101889469853757)),
                                    new KeyValuePair<string, DiscordRole>("gold", guildObj.GetRole(1176101930905374729)),
                                    new KeyValuePair<string, DiscordRole>("platin", guildObj.GetRole(1176101978158403667)),
                                    new KeyValuePair<string, DiscordRole>("diamant", guildObj.GetRole(1176102030167789588)),
                                    new KeyValuePair<string, DiscordRole>("master", guildObj.GetRole(1176102031870656522))
                        };

                        foreach (UserLevelSystem userLevelSystemItem in userLevelSystemListSortedOut)
                        {
                            DiscordMember discordMember = guildObj.GetMemberAsync(userLevelSystemItem.DiscordMemberID).Result;

                            int totalLevel = CalculateLevel(userLevelSystemItem.OnlineTicks);

                            switch (totalLevel)
                            {
                                case >= 100:
                                {
                                    const string targetRoleName = "master";
                                    await SetRank(levelRoles, targetRoleName, discordMember, totalLevel);
                                    break;
                                }
                                case >= 80:
                                {
                                    const string targetRoleName = "diamant";
                                    await SetRank(levelRoles, targetRoleName, discordMember, totalLevel);
                                    break;
                                }
                                case >= 50:
                                {
                                    const string targetRoleName = "platin";
                                    await SetRank(levelRoles, targetRoleName, discordMember, totalLevel);
                                    break;
                                }
                                case >= 15:
                                {
                                    const string targetRoleName = "gold";
                                    await SetRank(levelRoles, targetRoleName, discordMember, totalLevel);
                                    break;
                                }
                                case >= 10:
                                {
                                    const string targetRoleName = "silber";
                                    await SetRank(levelRoles, targetRoleName, discordMember, totalLevel);
                                    break;
                                }
                                case >= 5:
                                {
                                    const string targetRoleName = "bronze";
                                    await SetRank(levelRoles, targetRoleName, discordMember, totalLevel);
                                    break;
                                }
                            }
                        }

                        new CustomLogger().Information("Finished", ConsoleColor.Green);
                    }
                    catch (Exception exception)
                    {
                        new CustomLogger().Error(exception);
                    }

                    await Task.Delay(2000);
                    if (!LastMinuteCheck.BrixLevelSystemRoleDistributionRunAsync)
                    {
                        LastMinuteCheck.BrixLevelSystemRoleDistributionRunAsync = true;
                    }
                }
            });
        }

        private async Task SetRank(List<KeyValuePair<string, DiscordRole>> levelRoles, string targetRoleName, DiscordMember discordMember, int totalLevel)
        {
            KeyValuePair<string, DiscordRole> targetRole = levelRoles.FirstOrDefault(x => x.Key == targetRoleName);
            foreach (KeyValuePair<string, DiscordRole> levelRole in levelRoles.Where(x => x.Key != targetRole.Key))
            {
                if (discordMember.Roles.Contains(levelRole.Value))
                {
                    await discordMember.RevokeRoleAsync(levelRole.Value);
                    new CustomLogger().Information($"Revoked {discordMember.DisplayName} DiscordMemberID Level {totalLevel} --- {discordMember.Id} Role {targetRole.Value.Id} {targetRole.Value.Name}", ConsoleColor.Green);
                }
            }

            if (!discordMember.Roles.Contains(targetRole.Value))
            {
                await discordMember.GrantRoleAsync(targetRole.Value);
                new CustomLogger().Information($"Granted {discordMember.DisplayName} DiscordMemberID Level {totalLevel} --- {discordMember.Id} Role {targetRole.Value.Id} {targetRole.Value.Name}", ConsoleColor.Green);
            }
        }
    }
}