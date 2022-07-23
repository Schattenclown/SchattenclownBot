﻿using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Objects
{
    public class SympathySystem
    {
        public int VoteTableID { get; set; }
        public ulong VotingUserID { get; set; }
        public ulong VotedUserID { get; set; }
        public ulong GuildID { get; set; }
        public int VoteRating { get; set; }
        public int VotedRating { get; set; }
        public RoleInfoSympathySystem RoleInfo { get; set; }
        public SympathySystem()
        {

        }
        public static List<SympathySystem> ReadAll(ulong guildId)
        {
            return DB_SympathySystem.ReadAll(guildId);
        }
        public static void Add(SympathySystem sympathySystem)
        {
            DB_SympathySystem.Add(sympathySystem);
        }
        public static void Change(SympathySystem sympathySystem)
        {
            DB_SympathySystem.Change(sympathySystem);
        }
        public static void CreateTable_SympathySystem(ulong guildId)
        {
            DB_SympathySystem.CreateTable_SympathySystem(guildId);
        }
        public static List<RoleInfoSympathySystem> ReadAllRoleInfo(ulong guildId)
        {
            return DB_SympathySystem.ReadAllRoleInfo(guildId);
        }
        public static void AddRoleInfo(SympathySystem sympathySystem)
        {
            DB_SympathySystem.AddRoleInfo(sympathySystem);
        }
        public static void ChangeRoleInfo(SympathySystem sympathySystem)
        {
            DB_SympathySystem.ChangeRoleInfo(sympathySystem);
        }
        public static bool CheckRoleInfoExists(ulong guildId, int ratingValue)
        {
            return DB_SympathySystem.CheckRoleInfoExists(guildId, ratingValue);
        }
        public static void CreateTable_RoleInfoSympathySystem(ulong guildId)
        {
            DB_SympathySystem.CreateTable_RoleInfoSympathySystem(guildId);
        }
        public static int GetUserRatings(ulong guildId, ulong votedUserID, int voteRating)
        {
            return DB_SympathySystem.GetUserRatings(guildId, votedUserID, voteRating);
        }
        public static async Task SympathySystemRunAsync(int executeSecond)
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
                                SympathySystem.CreateTable_SympathySystem(guildItem.Value.Id);
                                SympathySystem.CreateTable_RoleInfoSympathySystem(guildItem.Value.Id);
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
                        DiscordGuild discordGuildObj = Bot.DiscordClient.GetGuildAsync(guildItem.Value.Id).Result;
                        IReadOnlyDictionary<ulong, DiscordMember> discordMembers = discordGuildObj.Members;

                        List<SympathySystem> sympathySystemsList = SympathySystem.ReadAll(guildItem.Value.Id);
                        List<RoleInfoSympathySystem> roleInfoSympathySystemsList = SympathySystem.ReadAllRoleInfo(guildItem.Value.Id);
                        List<DiscordRole> discordRoleList = new();

                        foreach (KeyValuePair<ulong, DiscordMember> discordMemberItem in discordMembers)
                        {
                            discordRoleList.Clear();

                            foreach (RoleInfoSympathySystem item in roleInfoSympathySystemsList)
                            {
                                if (item.RatingOne != 0)
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingOne));
                                else if (item.RatingTwo != 0)
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingTwo));
                                else if (item.RatingThree != 0)
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingThree));
                                else if (item.RatingFour != 0)
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingFour));
                                else if (item.RatingFive != 0)
                                    discordRoleList.Add(discordGuildObj.GetRole(item.RatingFive));
                            }

                            if (discordRoleList.Count == 5 && discordMemberItem.Value.Id != 523765246104567808)
                            {
                                int counts = 1;
                                int ratingsadded = 0;
                                double rating = 0.0;
                                SympathySystem sympathySystemObj = new();

                                foreach (SympathySystem sympathySystemItem in sympathySystemsList)
                                {
                                    if (discordMemberItem.Value.Id == sympathySystemItem.VotedUserID)
                                    {
                                        sympathySystemObj = sympathySystemItem;

                                        ratingsadded += sympathySystemItem.VoteRating;
                                        rating = Convert.ToDouble(ratingsadded) / Convert.ToDouble(counts);

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
                }
            });
        }
    }
}
