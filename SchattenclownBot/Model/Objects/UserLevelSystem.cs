using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public static async Task LevelSystemRunAsync()
        {
            bool levelSystemVirign = true;

            await Task.Run(async () =>
            {
                do
                {
                    if (Bot.Client.Guilds.ToList().Count != 0)
                    {
                        if (levelSystemVirign)
                        {
                            var guildsList = Bot.Client.Guilds.ToList();
                            foreach (var guildItem in guildsList)
                            {
                                UserLevelSystem.CreateTable_UserLevelSystem(guildItem.Value.Id);
                            }
                            levelSystemVirign = false;
                        }
                    }
                    await Task.Delay(1000);
                } while (levelSystemVirign);

                while (DateTime.Now.Second != 59)
                {
                    await Task.Delay(1000);
                }

                while (true)
                {
                    while (DateTime.Now.Second != 59)
                    {
                        await Task.Delay(1000);
                    }

                    var guildsList = Bot.Client.Guilds.ToList();
                    foreach (var guildItem in guildsList)
                    {
                        List<UserLevelSystem> userLevelSystemList = new List<UserLevelSystem>();
                        userLevelSystemList = UserLevelSystem.Read(guildItem.Value.Id);

                        var guildMembers = guildItem.Value.Members;
                        foreach (var memberItem in guildMembers)
                        {
                            if (memberItem.Value.VoiceState != null && !memberItem.Value.VoiceState.IsSelfDeafened && !memberItem.Value.VoiceState.IsSuppressed && !memberItem.Value.IsBot)
                            {
                                UserLevelSystem userLevelSystemObj = new UserLevelSystem();
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
                                    DateTime date1 = new DateTime(1969, 4, 20, 4, 20, 0);
                                    DateTime date2 = new DateTime(1969, 4, 20, 4, 21, 0);
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

        public static async Task LevelSystemRoleDistributionRunAsync()
        {
            bool LevelSystemRoleDistributionVirign = true;

            await Task.Run(async () =>
            {
                do
                {
                    if (Bot.Client.Guilds.ToList().Count != 0)
                    {
                        if (LevelSystemRoleDistributionVirign)
                        {
                            var guildsList = Bot.Client.Guilds.ToList();
                            foreach (var guildItem in guildsList)
                            {
                                //UserLevelSystem.CreateTable_UserLevelSystem(guildItem.Value.Id);
                            }
                            LevelSystemRoleDistributionVirign = false;
                        }
                    }
                    await Task.Delay(1000);
                } while (LevelSystemRoleDistributionVirign);

                while (DateTime.Now.Second != 59)
                {
                    await Task.Delay(1000);
                }

                while (true)
                {
                    while (DateTime.Now.Second != 59)
                    {
                        await Task.Delay(1000);
                    }



                }
                await Task.Delay(2000);
            

            });
        }
    }
}
