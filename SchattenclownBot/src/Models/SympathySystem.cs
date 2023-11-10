using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.DataAccess.MySQL.Services;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Models
{
    public class SympathySystem
    {
        public int VoteTableId { get; set; }
        public ulong VotingUserId { get; set; }
        public ulong VotedUserId { get; set; }
        public ulong GuildId { get; set; }
        public int VoteRating { get; set; }
        public int VotedRating { get; set; }
        public RoleInfoSympathySystem RoleInfo { get; set; }

        public List<SympathySystem> ReadAll(ulong guildId)
        {
            return new DbSympathySystem().ReadAll(guildId);
        }

        public void Add(SympathySystem sympathySystem)
        {
            new DbSympathySystem().Add(sympathySystem);
        }

        public void Change(SympathySystem sympathySystem)
        {
            new DbSympathySystem().Change(sympathySystem);
        }

        public void CreateTable_SympathySystem(ulong guildId)
        {
            new DbSympathySystem().CreateTable(guildId);
        }

        public List<RoleInfoSympathySystem> ReadAllRoleInfo(ulong guildId)
        {
            return new DbSympathySystem().ReadAllRoleInfo(guildId);
        }

        public void AddRoleInfo(SympathySystem sympathySystem)
        {
            new DbSympathySystem().AddRoleInfo(sympathySystem);
        }

        public void ChangeRoleInfo(SympathySystem sympathySystem)
        {
            new DbSympathySystem().ChangeRoleInfo(sympathySystem);
        }

        public bool CheckRoleInfoExists(ulong guildId, int ratingValue)
        {
            return new DbSympathySystem().CheckRoleInfoExists(guildId, ratingValue);
        }

        public void CreateTable(ulong guildId)
        {
            new DbSympathySystem().CreateTable_RoleInfoSympathySystem(guildId);
        }

        public int GetUserRatings(ulong guildId, ulong votedUserId, int voteRating)
        {
            return new DbSympathySystem().GetUserRatings(guildId, votedUserId, voteRating);
        }

        public void RunAsync(int executeSecond)
        {
            new CustomLogger().Information("Starting SympathySystem...", ConsoleColor.Green);
            bool levelSystemVirgin = true;

            Task.Run(async () =>
            {
                while (DateTime.Now.Second != executeSecond)
                {
                    await Task.Delay(1000);
                }

                do
                {
                    if (DiscordBot.DiscordClient.Guilds.ToList().Count != 0)
                    {
                        if (levelSystemVirgin)
                        {
                            List<KeyValuePair<ulong, DiscordGuild>> guildsList = DiscordBot.DiscordClient.Guilds.ToList();
                            foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
                            {
                                CreateTable_SympathySystem(guildItem.Value.Id);
                                CreateTable(guildItem.Value.Id);
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

                    List<KeyValuePair<ulong, DiscordGuild>> guildsList = DiscordBot.DiscordClient.Guilds.ToList();
                    foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
                    {
                        DiscordGuild discordGuildObj = DiscordBot.DiscordClient.GetGuildAsync(guildItem.Value.Id).Result;
                        IReadOnlyDictionary<ulong, DiscordMember> discordMembers = discordGuildObj.Members;

                        List<SympathySystem> sympathySystemsList = ReadAll(guildItem.Value.Id);
                        List<RoleInfoSympathySystem> roleInfoSympathySystemsList = ReadAllRoleInfo(guildItem.Value.Id);
                        List<DiscordRole> discordRoleList = new();

                        foreach (KeyValuePair<ulong, DiscordMember> discordMemberItem in discordMembers)
                        {
                            discordRoleList.Clear();

                            foreach (RoleInfoSympathySystem item in roleInfoSympathySystemsList)
                            {
                                if (item.RatingOne != 0)
                                {
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingOne));
                                }
                                else if (item.RatingTwo != 0)
                                {
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingTwo));
                                }
                                else if (item.RatingThree != 0)
                                {
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingThree));
                                }
                                else if (item.RatingFour != 0)
                                {
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingFour));
                                }
                                else if (item.RatingFive != 0)
                                {
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingFive));
                                }
                            }

                            if (discordRoleList.Count == 5 && discordMemberItem.Value.Id != 523765246104567808)
                            {
                                int counts = 1;
                                int ratingsadded = 0;
                                SympathySystem sympathySystemObj = new();

                                foreach (SympathySystem sympathySystemItem in sympathySystemsList)
                                {
                                    if (discordMemberItem.Value.Id == sympathySystemItem.VotedUserId)
                                    {
                                        sympathySystemObj = sympathySystemItem;

                                        ratingsadded += sympathySystemItem.VoteRating;
                                        double rating = Convert.ToDouble(ratingsadded) / Convert.ToDouble(counts);

                                        sympathySystemObj.VotedRating = Convert.ToInt32(Math.Round(rating));

                                        if (rating == 1.5 || rating == 2.5 || rating == 3.5 || rating == 4.5)
                                        {
                                            sympathySystemObj.VotedRating = Convert.ToInt32(Math.Round(rating, 0, MidpointRounding.ToPositiveInfinity));
                                        }

                                        counts++;
                                    }
                                }

                                if (sympathySystemObj.VotedRating == 1)
                                {
                                    if (!discordMemberItem.Value.Roles.Contains(discordRoleList[0]))
                                    {
                                        await discordMemberItem.Value.GrantRoleAsync(discordRoleList[0]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[1]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[2]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[3]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[4]);
                                    }
                                }
                                else if (sympathySystemObj.VotedRating == 2)
                                {
                                    if (!discordMemberItem.Value.Roles.Contains(discordRoleList[1]))
                                    {
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[0]);
                                        await discordMemberItem.Value.GrantRoleAsync(discordRoleList[1]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[2]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[3]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[4]);
                                    }
                                }
                                else if (sympathySystemObj.VotedRating == 3)
                                {
                                    if (!discordMemberItem.Value.Roles.Contains(discordRoleList[2]))
                                    {
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[0]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[1]);
                                        await discordMemberItem.Value.GrantRoleAsync(discordRoleList[2]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[3]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[4]);
                                    }
                                }
                                else if (sympathySystemObj.VotedRating == 4)
                                {
                                    if (!discordMemberItem.Value.Roles.Contains(discordRoleList[3]))
                                    {
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[0]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[1]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[2]);
                                        await discordMemberItem.Value.GrantRoleAsync(discordRoleList[3]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[4]);
                                    }
                                }
                                else if (sympathySystemObj.VotedRating == 5)
                                {
                                    if (!discordMemberItem.Value.Roles.Contains(discordRoleList[4]))
                                    {
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[0]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[1]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[2]);
                                        await discordMemberItem.Value.RevokeRoleAsync(discordRoleList[3]);
                                        await discordMemberItem.Value.GrantRoleAsync(discordRoleList[4]);
                                    }
                                }
                            }
                        }
                    }

                    await Task.Delay(2000);
                    new CustomLogger().Information("Checked", ConsoleColor.Green);
                    if (!LastMinuteCheck.SympathySystemRunAsync)
                    {
                        LastMinuteCheck.SympathySystemRunAsync = true;
                    }
                }
                // ReSharper disable once FunctionNeverReturns
            });
        }
    }
}