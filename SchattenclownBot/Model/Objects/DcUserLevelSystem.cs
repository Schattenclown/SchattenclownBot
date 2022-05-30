﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Objects
{
    public class DcUserLevelSystem
    {
        public ulong MemberId { get; set; }
        public ulong GuildId { get; set; }
        public int OnlineTicks { get; set; }
        public TimeSpan OnlineTime { get; set; }
        public double VoteRatingAvg { get; set; }
        public DcUserLevelSystem()
        {

        }
        public static List<DcUserLevelSystem> Read(ulong guildsId)
        {
            return DB_DcUserLevelSystem.Read(guildsId);
        }
        public static void Add(ulong guildsId, DcUserLevelSystem dcLevelSystem)
        {
            DB_DcUserLevelSystem.Add(guildsId, dcLevelSystem);
        }
        public static void Change(ulong guildsId, DcUserLevelSystem dcLevelSystem)
        {
            DB_DcUserLevelSystem.Change(guildsId, dcLevelSystem);
        }
        public static void CreateTable_DcUserLevelSystem(ulong guildsId)
        {
            DB_DcUserLevelSystem.CreateTable_DcUserLevelSystem(guildsId);
        }
        public static async Task LevelSystem()
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
                                DcUserLevelSystem.CreateTable_DcUserLevelSystem(guildItem.Value.Id);
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
                        List<DcUserLevelSystem> dcUserLevelSystemList = new List<DcUserLevelSystem>();
                        dcUserLevelSystemList = DcUserLevelSystem.Read(guildItem.Value.Id);

                        var guildMembers = guildItem.Value.Members;
                        foreach (var memberItem in guildMembers)
                        {
                            if (memberItem.Value.VoiceState != null && !memberItem.Value.VoiceState.IsSelfDeafened && !memberItem.Value.VoiceState.IsSuppressed && !memberItem.Value.IsBot)
                            {
                                DcUserLevelSystem dcUserLevelSystemObj = new DcUserLevelSystem();
                                dcUserLevelSystemObj.MemberId = memberItem.Value.Id;
                                dcUserLevelSystemObj.OnlineTicks = 0;
                                bool found = false;

                                foreach (DcUserLevelSystem dcUserLevelSystemItem in dcUserLevelSystemList)
                                {
                                    if (memberItem.Value.Id == dcUserLevelSystemItem.MemberId)
                                    {
                                        dcUserLevelSystemObj.OnlineTicks = dcUserLevelSystemItem.OnlineTicks;
                                        found = true;
                                        break;
                                    }
                                }

                                if (found)
                                {
                                    dcUserLevelSystemObj.OnlineTicks++;
                                    DcUserLevelSystem.Change(guildItem.Value.Id, dcUserLevelSystemObj);
                                }

                                if (!found)
                                {
                                    DateTime date1 = new DateTime(1969, 4, 20, 4, 20, 0);
                                    DateTime date2 = new DateTime(1969, 4, 20, 4, 21, 0);
                                    TimeSpan timeSpan = date2 - date1;
                                    dcUserLevelSystemObj.OnlineTime = timeSpan;
                                    dcUserLevelSystemObj.OnlineTicks = 1;
                                    DcUserLevelSystem.Add(guildItem.Value.Id, dcUserLevelSystemObj);
                                }
                            }
                        }
                    }
                    await Task.Delay(2000);
                }
            });
        }
    }
}
