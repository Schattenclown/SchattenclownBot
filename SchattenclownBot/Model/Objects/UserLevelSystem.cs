using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using MySql.Data.MySqlClient.Memcached;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;

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
            var returnInt = 0.69 * Math.Pow(onlineTicks, 0.38);
            var returnIntRounded = Math.Round(returnInt, MidpointRounding.ToNegativeInfinity);
            return Convert.ToInt32(returnIntRounded);
        }
        public static int CalculateXpOverCurrentLevel(int onlineTicks)
        {
            var level = CalculateLevel(onlineTicks);

            var xpToReachThisLevel = Math.Pow(level / 0.69, 1 / 0.38);
            var calculatedXpOverCurrentLevel = onlineTicks - Convert.ToInt32(xpToReachThisLevel);

            return calculatedXpOverCurrentLevel;
        }
        public static int CalculateXpSpanToReachNextLevel(int onlineTicks)
        {
            var level = CalculateLevel(onlineTicks);
            
            var xpToReachThisLevel = Math.Pow(level / 0.69, 1 / 0.38);
            var xpToReachNextLevel = Math.Pow((level + 1) / 0.69, 1 / 0.38);
            var xpSpanToReachNextLevel = Convert.ToInt32(xpToReachNextLevel) - Convert.ToInt32(xpToReachThisLevel);

            return xpSpanToReachNextLevel;
        }
        public static async Task LevelSystemRunAsync(int executeSecond)
        {
            var levelSystemVirgin = true;

            await Task.Run(async () =>
            {
                do
                {
                    if (Bot.Client.Guilds.ToList().Count != 0)
                    {
                        if (levelSystemVirgin)
                        {
                            var guildsList = Bot.Client.Guilds.ToList();
                            foreach (var guildItem in guildsList)
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

                    var guildsList = Bot.Client.Guilds.ToList();
                    foreach (var guildItem in guildsList)
                    {
                        var userLevelSystemList = new List<UserLevelSystem>();
                        userLevelSystemList = UserLevelSystem.Read(guildItem.Value.Id);

                        var guildMembers = guildItem.Value.Members;
                        foreach (var memberItem in guildMembers)
                        {
                            if (memberItem.Value.VoiceState != null && !memberItem.Value.VoiceState.IsSelfDeafened && !memberItem.Value.VoiceState.IsSuppressed && !memberItem.Value.IsBot)
                            {
                                var userLevelSystemObj = new UserLevelSystem();
                                userLevelSystemObj.MemberId = memberItem.Value.Id;
                                userLevelSystemObj.OnlineTicks = 0;
                                var found = false;

                                foreach (var userLevelSystemItem in userLevelSystemList)
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
                                    var date1 = new DateTime(1969, 4, 20, 4, 20, 0);
                                    var date2 = new DateTime(1969, 4, 20, 4, 21, 0);
                                    var timeSpan = date2 - date1;
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
            var levelSystemRoleDistributionVirgin = true;
            DiscordGuild guildObj = null;
            var sortLevelSystemRolesBool = false;
            const int delayInMs = 200;

            await Task.Run(async () =>
            {
                do
                {
                    if (Bot.Client.Guilds.ToList().Count != 0)
                    {
                        if (levelSystemRoleDistributionVirgin)
                        {
                            var guildsList = Bot.Client.Guilds.ToList();
                            foreach (var guildItem in guildsList.Where(guiltItem => guiltItem.Value.Id == 928930967140331590))
                            {
                                guildObj = Bot.Client.GetGuildAsync(guildItem.Value.Id).Result;
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

                    //Create List where all users are listed.
                    var userLevelSystemList = UserLevelSystem.Read(guildObj.Id);
                    //Order the list by online ticks.
                    var userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
                    userLevelSystemListSorted.Reverse();

                    var guildMemberList = guildObj.Members.Values.ToList();

                    var userLevelSystemListSortedOut = guildMemberList.SelectMany(guildMemberItem => userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.MemberId == guildMemberItem.Id)).ToList();
                    userLevelSystemListSortedOut = userLevelSystemListSortedOut.OrderBy(x => x.OnlineTicks).ToList();
                    var discordRoleList = guildObj.Roles.Values.ToList();

                    foreach (var userLevelSystemItem in userLevelSystemListSortedOut)
                    {
                        //Get the discord user by ID.
                        var discordMember = guildObj.GetMemberAsync(userLevelSystemItem.MemberId).Result;
                        await Task.Delay(delayInMs);

                        DiscordRole discordRoleObj = null;
                        var roleIndex = guildObj.GetRole(981575801214492752).Position - 1;
                        await Task.Delay(delayInMs);

                        var totalLevel = CalculateLevel(userLevelSystemItem.OnlineTicks);

                        var voiceChannelLevelString = $"{RoleChannelLevelString} {totalLevel}";
                        var roleExists = false;


                        foreach (var discordRoleItem in discordRoleList.Where(role => role.Name == voiceChannelLevelString))
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

                        var discordMemberRoleList = discordMember.Roles.ToList();
                        Task.Delay(delayInMs);

                        foreach (var revokeRoleItem in discordMemberRoleList.Where(revokeRoleItem => revokeRoleItem.Name.Contains(RoleChannelLevelString) && revokeRoleItem.Name != voiceChannelLevelString))
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
            });
        }
        public static async Task SortLevelSystemRolesRunAsync(int executeSecond)
        {
            var levelSystemRoleDistributionVirgin = true;
            DiscordGuild guildObj = null;

            await Task.Run(async () =>
            {
                do
                {
                    if (Bot.Client.Guilds.ToList().Count != 0)
                    {
                        if (levelSystemRoleDistributionVirgin)
                        {
                            var guildsList = Bot.Client.Guilds.ToList();
                            foreach (var guildItem in guildsList.Where(guiltItem => guiltItem.Value.Id == 928930967140331590))
                            {
                                guildObj = Bot.Client.GetGuildAsync(guildItem.Value.Id).Result;
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

                    var discordRoleList = guildObj.Roles.Values.ToList();
                    var discordRoleListSortedOut = new List<KeyValuePair<int, DiscordRole>>();

                    foreach (var discordRoleItem in discordRoleList.Where(discordRoleItem => discordRoleItem.Name.Contains(RoleChannelLevelString)))
                    {
                        if (discordRoleItem.Id != 981575801214492752)
                        {
                            var roleLevelString = discordRoleItem.Name.Substring(RoleChannelLevelString.Length + 1);
                            var roleLevel = Convert.ToInt32(roleLevelString);
                            var keyValuePair = new KeyValuePair<int, DiscordRole>(roleLevel, discordRoleItem);
                            discordRoleListSortedOut.Add(keyValuePair);
                        }
                    }

                    var discordRoleListSortedOutOrdered = discordRoleListSortedOut.OrderBy(x => x.Key).ToList();
                    discordRoleListSortedOutOrdered.Reverse();

                    var discordRolesInt = guildObj.Roles.Count;
                    var roleIndex = guildObj.GetRole(981575801214492752).Position - 1;
                    var index = 0;

                    foreach (var discordRoleItem in discordRoleListSortedOutOrdered)
                    {
                        if (discordRolesInt != guildObj.Roles.Count)
                            break;

                        var newRoleIndex = roleIndex - index;
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
