using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Objects
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

      public static List<SympathySystem> ReadAll(ulong guildId)
      {
         return DbSympathySystem.ReadAll(guildId);
      }
      public static void Add(SympathySystem sympathySystem)
      {
         DbSympathySystem.Add(sympathySystem);
      }
      public static void Change(SympathySystem sympathySystem)
      {
         DbSympathySystem.Change(sympathySystem);
      }
      public static void CreateTable_SympathySystem(ulong guildId)
      {
         DbSympathySystem.CreateTable_SympathySystem(guildId);
      }
      public static List<RoleInfoSympathySystem> ReadAllRoleInfo(ulong guildId)
      {
         return DbSympathySystem.ReadAllRoleInfo(guildId);
      }
      public static void AddRoleInfo(SympathySystem sympathySystem)
      {
         DbSympathySystem.AddRoleInfo(sympathySystem);
      }
      public static void ChangeRoleInfo(SympathySystem sympathySystem)
      {
         DbSympathySystem.ChangeRoleInfo(sympathySystem);
      }
      public static bool CheckRoleInfoExists(ulong guildId, int ratingValue)
      {
         return DbSympathySystem.CheckRoleInfoExists(guildId, ratingValue);
      }
      public static void CreateTable_RoleInfoSympathySystem(ulong guildId)
      {
         DbSympathySystem.CreateTable_RoleInfoSympathySystem(guildId);
      }
      public static int GetUserRatings(ulong guildId, ulong votedUserId, int voteRating)
      {
         return DbSympathySystem.GetUserRatings(guildId, votedUserId, voteRating);
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
                        double rating;
                        SympathySystem sympathySystemObj = new();

                        foreach (SympathySystem sympathySystemItem in sympathySystemsList)
                        {
                           if (discordMemberItem.Value.Id == sympathySystemItem.VotedUserId)
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
               CWLogger.Write("Checked", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);

            }
            // ReSharper disable once FunctionNeverReturns
         });
      }
   }
}
