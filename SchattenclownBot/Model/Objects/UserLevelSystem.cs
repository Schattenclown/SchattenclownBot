using DisCatSharp;
using DisCatSharp.Entities;
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
        public UserLevelSystem()
        {

        }
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
                        List<UserLevelSystem> userLevelSystemList = new();
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

                while (true && guildObj != null)
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

                    await Task.Delay(2000);
                }
            });
        }
    }
}
