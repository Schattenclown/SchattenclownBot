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

#pragma warning disable CA1822
#pragma warning disable CA1822

namespace SchattenclownBot.Models
{
    public class SympathySystem
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public ulong VotingMemberID { get; set; }

        [Required]
        public ulong TargetMemberID { get; set; }

        [Required]
        public ulong GuildID { get; set; }

        [Required]
        public int Rating { get; set; }

        public void AddOrUpdate(SympathySystem sympathySystem)
        {
            new SympathySystemDBA().AddOrUpdate(sympathySystem);
        }

        public List<SympathySystem> ReadBasedOnTargetMemberID(ulong memberID)
        {
            return new SympathySystemDBA().ReadBasedOnTargetMemberID(memberID);
        }

        public int GetMemberRatingsByRatingValue(ulong memberID, int ratingValue)
        {
            return new SympathySystemDBA().GetMemberRatingsByRatingValue(memberID, ratingValue);
        }

        public void RunAsync(int executeSecond)
        {
            new CustomLogger().Information("Starting SympathySystemAC...", ConsoleColor.Green);

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(executeSecond - DateTime.Now.Second % executeSecond));

                    DiscordGuild discordGuild = await DiscordBot.DiscordClient.GetGuildAsync(928930967140331590);
                    IEnumerable<DiscordMember> discordMembers = discordGuild.Members.Values;

                    List<DiscordRole> discordRoleList = new();
                    discordRoleList.Clear();
                    discordRoleList.Add(discordGuild.GetRole(949045281108922459));
                    discordRoleList.Add(discordGuild.GetRole(949045284355342386));
                    discordRoleList.Add(discordGuild.GetRole(949045286808981565));
                    discordRoleList.Add(discordGuild.GetRole(949045289728233552));
                    discordRoleList.Add(discordGuild.GetRole(949045292282544148));

                    foreach (DiscordMember discordMember in discordMembers)
                    {
                        if (discordRoleList.Count != 5 || discordMember.Id == 523765246104567808)
                        {
                            continue;
                        }

                        List<SympathySystem> sympathySystemsTarget = new SympathySystem().ReadBasedOnTargetMemberID(discordMember.Id);
                        if (sympathySystemsTarget.Count == 0)
                        {
                            continue;
                        }

                        int count = sympathySystemsTarget.Count;
                        int ratingsAdded = sympathySystemsTarget.Sum(sympathySystem => sympathySystem.Rating);
                        double ratingDouble = Convert.ToDouble(ratingsAdded) / Convert.ToDouble(count);
                        int rating = Convert.ToInt32(Math.Round(ratingDouble));

                        switch (rating)
                        {
                            case 1:
                                if (!discordMember.Roles.Contains(discordRoleList[0]))
                                {
                                    await discordMember.GrantRoleAsync(discordRoleList[0]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[1]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[2]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[3]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[4]);
                                }

                                break;
                            case 2:
                                if (!discordMember.Roles.Contains(discordRoleList[1]))
                                {
                                    await discordMember.RevokeRoleAsync(discordRoleList[0]);
                                    await discordMember.GrantRoleAsync(discordRoleList[1]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[2]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[3]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[4]);
                                }

                                break;
                            case 3:
                                if (!discordMember.Roles.Contains(discordRoleList[2]))
                                {
                                    await discordMember.RevokeRoleAsync(discordRoleList[0]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[1]);
                                    await discordMember.GrantRoleAsync(discordRoleList[2]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[3]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[4]);
                                }

                                break;
                            case 4:
                                if (!discordMember.Roles.Contains(discordRoleList[3]))
                                {
                                    await discordMember.RevokeRoleAsync(discordRoleList[0]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[1]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[2]);
                                    await discordMember.GrantRoleAsync(discordRoleList[3]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[4]);
                                }

                                break;
                            case 5:
                                if (!discordMember.Roles.Contains(discordRoleList[4]))
                                {
                                    await discordMember.RevokeRoleAsync(discordRoleList[0]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[1]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[2]);
                                    await discordMember.RevokeRoleAsync(discordRoleList[3]);
                                    await discordMember.GrantRoleAsync(discordRoleList[4]);
                                }

                                break;
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