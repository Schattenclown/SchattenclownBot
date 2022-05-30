using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;
using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.Objects
{
    public class DcSympathieSystem
    {
        public int VoteTableID { get; set; }
        public ulong VotingUserID { get; set; }
        public ulong VotedUserID { get; set; }
        public ulong GuildID { get; set; }
        public int VoteRating { get; set; }
        public int VotedRating { get; set; }
        public DcSymSysRoleInfo RoleInfo { get; set; }
        public DcSympathieSystem()
        {

        }
        public static List<DcSympathieSystem> ReadAll(ulong guildsId)
        {
            return DB_DcSympathieSystem.ReadAll(guildsId);
        }
        public static void Add(DcSympathieSystem dcSympathieSystem)
        {
            DB_DcSympathieSystem.Add(dcSympathieSystem);
        }
        public static void Change(DcSympathieSystem dcSympathieSystem)
        {
            DB_DcSympathieSystem.Change(dcSympathieSystem);
        }
        public static void CreateTable_DcSympathieSystem(ulong guildsId)
        {
            DB_DcSympathieSystem.CreateTable_DcSympathieSystem(guildsId);
        }
        public static List<DcSymSysRoleInfo> ReadAllRoleInfo(ulong guildsId)
        {
            return DB_DcSympathieSystem.ReadAllRoleInfo(guildsId);
        }
        public static void AddRoleInfo(DcSympathieSystem dcSympathieSystem)
        {
            DB_DcSympathieSystem.AddRoleInfo(dcSympathieSystem);
        }
        public static void ChangeRoleInfo(DcSympathieSystem dcSympathieSystem)
        {
            DB_DcSympathieSystem.ChangeRoleInfo(dcSympathieSystem);
        }
        public static bool CheckRoleInfoExists(ulong guildId, int ratingValue)
        {
            return DB_DcSympathieSystem.CheckRoleInfoExists(guildId, ratingValue);
        }
        public static void CreateTable_DcSympathieSystemRoleInfo(ulong guildsId)
        {
            DB_DcSympathieSystem.CreateTable_DcSympathieSystemRoleInfo(guildsId);
        }
        public static int GetUserRatings(ulong guildsId, ulong votedUserID, int voteRating)
        {
            return DB_DcSympathieSystem.GetUserRatings(guildsId, votedUserID, voteRating);
        }
        public static async Task SympathieSystem()
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
                                DcSympathieSystem.CreateTable_DcSympathieSystem(guildItem.Value.Id);
                                DcSympathieSystem.CreateTable_DcSympathieSystemRoleInfo(guildItem.Value.Id);
                            }
                            levelSystemVirign = false;
                        }
                    }
                    await Task.Delay(1000);
                } while (levelSystemVirign);

                while (true)
                {
                    while (DateTime.Now.Second != 29)
                    {
                        await Task.Delay(1000);
                    }

                    var guildsList = Bot.Client.Guilds.ToList();
                    foreach (var guildItem in guildsList)
                    {
                        DiscordGuild discordGuildObj = Bot.Client.GetGuildAsync(guildItem.Value.Id).Result;
                        var discordMembers = discordGuildObj.Members;

                        List<DcSympathieSystem> dcSympathieSystemsList = DcSympathieSystem.ReadAll(guildItem.Value.Id);
                        List<DcSympathieSystem> dcSympathieSystemsFinishedList = new();
                        List<DcSymSysRoleInfo> dcSymSysRoleInfosList = DcSympathieSystem.ReadAllRoleInfo(guildItem.Value.Id);
                        List<DiscordRole> discordRoleList = new();

                        foreach (var discordMemberItem in discordMembers)
                        {
                            discordRoleList.Clear();
                            
                            foreach (var item in dcSymSysRoleInfosList)
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

                            if (discordRoleList.Count == 5)
                            {
                                int counts = 1;
                                int ratingsadded = 0;
                                double rating = 0.0;
                                DcSympathieSystem dcSympathieSystemObj = new();

                                foreach (DcSympathieSystem dcSympathieSystemItem in dcSympathieSystemsList)
                                {
                                    if (discordMemberItem.Value.Id == dcSympathieSystemItem.VotedUserID)
                                    {
                                        dcSympathieSystemObj = dcSympathieSystemItem;

                                        ratingsadded += dcSympathieSystemItem.VoteRating;
                                        rating = Convert.ToDouble(ratingsadded) / Convert.ToDouble(counts);

                                        dcSympathieSystemObj.VotedRating = Convert.ToInt32(Math.Round(rating));

                                        if (rating == 1.5 || rating == 2.5 || rating == 3.5 || rating == 4.5)
                                        {
                                            dcSympathieSystemObj.VotedRating = Convert.ToInt32(Math.Round(rating, 0, MidpointRounding.ToPositiveInfinity));
                                        }

                                        counts++;
                                    }
                                }

                                if (dcSympathieSystemObj.VotedRating == 1)
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
                                else if (dcSympathieSystemObj.VotedRating == 2)
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
                                else if (dcSympathieSystemObj.VotedRating == 3)
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
                                else if (dcSympathieSystemObj.VotedRating == 4)
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
                                else if (dcSympathieSystemObj.VotedRating == 5)
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
