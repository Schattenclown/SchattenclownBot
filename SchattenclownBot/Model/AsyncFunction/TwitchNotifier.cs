using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class TwitchNotifier
   {
      public ulong DiscordGuildId { get; set; }
      public ulong DiscordMemberId { get; set; }
      public ulong DiscordChannelId { get; set; }
      public ulong DiscordRoleId { get; set; }
      public ulong TwitchUserId { get; set; }
      public string TwitchChannelUrl { get; set; }
      public DiscordMessage DiscordMessage { get; set; }
      public override string ToString()
      {
         return $"DiscordGuildId: {DiscordGuildId}" +
                $"\nDiscordMemberId: <@{DiscordMemberId}>" +
                $"\nDiscordChannelId: <#{DiscordChannelId}>" +
                $"\nDiscordRoleId: <@{DiscordRoleId}>" +
                $"\nTwitchUserId: {TwitchUserId}" +
                $"\nTwitchChannelUrl: {TwitchChannelUrl}";
      }

      public static List<TwitchNotifier> Read(ulong guildId)
      {
         return DB_TwitchNotifier.Read(guildId);
      }
      public static void Add(TwitchNotifier twitchNotifier)
      {
         DB_TwitchNotifier.Add(twitchNotifier);
      }
      public static async Task CreateTable_TwitchNotifier()
      {
         bool levelSystemVirgin = true;
         do
         {
            if (Bot.DiscordClient.Guilds.ToList().Count != 0)
            {
               List<KeyValuePair<ulong, DiscordGuild>> guildsList = Bot.DiscordClient.Guilds.ToList();
               foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
               {
                  await DB_TwitchNotifier.CreateTable_TwitchNotifier(guildItem.Value.Id);
               }

               levelSystemVirgin = false;
            }

            await Task.Delay(1000);
         } while (levelSystemVirgin);

      }

      internal static LiveStreamMonitorService Monitor;
      internal static TwitchAPI API;
      internal static List<TwitchNotifier> twitchNotifiers = new();

      internal static async Task Run()
      {
         //https://twitchtokengenerator.com/
         API = new TwitchAPI
         {
            Settings =
            {
               ClientId = Bot.Connections.TwitchToken.ClientId,
               AccessToken = Bot.Connections.TwitchToken.ClientSecret
            }
         };

         bool levelSystemVirgin = true;
         do
         {
            if (Bot.DiscordClient.Guilds.ToList().Count != 0)
            {
               List<KeyValuePair<ulong, DiscordGuild>> guildsList = Bot.DiscordClient.Guilds.ToList();
               foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
               {
                  twitchNotifiers.AddRange(TwitchNotifier.Read(guildItem.Value.Id));
               }
               levelSystemVirgin = false;
            }

            await Task.Delay(1000);
         } while (levelSystemVirgin);

         SetMonitoring();
      }

      internal static void SetMonitoring()
      {
         twitchNotifiers.Clear();

         List<KeyValuePair<ulong, DiscordGuild>> guildsList = Bot.DiscordClient.Guilds.ToList();
         foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
         {
            twitchNotifiers.AddRange(TwitchNotifier.Read(guildItem.Value.Id));
         }


         List<string> list = new();

         foreach (TwitchNotifier twitchNotifierItem in twitchNotifiers)
         {
            /*if (twitchNotifierItem.TwitchUserId != 0)
            {
               list.Add(twitchNotifierItem.TwitchUserId.ToString());
            }
            else */

            if (twitchNotifierItem.TwitchChannelUrl != "")
            {
               list.Add(twitchNotifierItem.TwitchChannelUrl);
            }
         }

         Monitor = new LiveStreamMonitorService(API, 1);

         Monitor.SetChannelsByName(list);
         Monitor.OnStreamOnline += Monitor_OnStreamOnline;
         Monitor.OnStreamOffline += Monitor_OnStreamOffline;
         Monitor.OnStreamUpdate += Monitor_OnStreamUpdate;
         Monitor.OnServiceStarted += Monitor_OnServiceStarted;
         Monitor.OnChannelsSet += Monitor_OnChannelsSet;
         Monitor.OnServiceTick += MonitorOnOnServiceTick;

         Monitor.Start();
      }

      internal static void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
      {
         foreach (var twitchNotifierItem in twitchNotifiers)
         {
            if (twitchNotifierItem.TwitchChannelUrl.ToLower() == e.Channel.ToLower())
            {
               DiscordGuild discordGuild = Bot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildId).Result;
               DiscordChannel discordChannel = Bot.DiscordClient.GetChannelAsync(twitchNotifierItem.DiscordChannelId).Result;
               DiscordRole discordRole = discordGuild.GetRole(twitchNotifierItem.DiscordRoleId);

               TimeSpan upTimeSpan = DateTime.Now.AddHours(-1) - e.Stream.StartedAt;

               DiscordEmbedBuilder discordEmbedBuilder = new();
               discordEmbedBuilder.Color = DiscordColor.Purple;
               discordEmbedBuilder.WithDescription($"{discordRole.Mention}");

               discordEmbedBuilder.AddField(new DiscordEmbedField("Game", e.Stream.GameName, true));
               discordEmbedBuilder.AddField(new DiscordEmbedField("Stream title", e.Stream.Title, true));
               discordEmbedBuilder.AddField(new DiscordEmbedField("Stream", e.Stream.UserName, true));
               discordEmbedBuilder.AddField(new DiscordEmbedField("ViewerCount", e.Stream.ViewerCount.ToString(), true));
               discordEmbedBuilder.AddField(new DiscordEmbedField("18+", e.Stream.IsMature.ToString(), true));
               discordEmbedBuilder.AddField(new DiscordEmbedField("UpTime:", $"{upTimeSpan:hh\\:mm\\:ss}", true));

               discordEmbedBuilder.WithImageUrl(e.Stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080"));
               discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{e.Channel}");


               if (e.Stream.GameId != null)
               {
                  GetGamesResponse getGamesResponse = API.Helix.Games.GetGamesAsync(new List<string> { e.Stream.GameId }).Result;
                  string gameIconUrl = getGamesResponse.Games[0].BoxArtUrl.Replace("{width}", "285").Replace("{height}", "380");
                  discordEmbedBuilder.WithThumbnail(gameIconUrl);
               }

               GetUsersResponse getUsersResponse = API.Helix.Users.GetUsersAsync(new List<string> { e.Stream.UserId }).Result;
               string userIconUrl = getUsersResponse.Users[0].ProfileImageUrl.Replace("{width}", "400").Replace("{height}", "400");
               discordEmbedBuilder.WithAuthor($"{e.Stream.UserName} is Live", $"https://www.twitch.tv/{e.Channel}", userIconUrl);
               discordEmbedBuilder.WithFooter(e.Stream.Title, userIconUrl);
               discordEmbedBuilder.WithTimestamp(DateTime.Now);


               DiscordComponentEmoji discordComponentEmoji = new(Bot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560));
               DiscordLinkButtonComponent discordLinkButtonComponent = new($"https://www.twitch.tv/{e.Channel}", "Open stream", false, discordComponentEmoji);

               twitchNotifierItem.DiscordMessage = discordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordLinkButtonComponent).WithContent($"https://www.twitch.tv/{e.Channel}")).Result;
            }
         }

         Console.WriteLine("Monitor_OnStreamOnline");
      }

      internal static void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
      {
         foreach (TwitchNotifier twitchNotifierItem in twitchNotifiers)
         {
            if (twitchNotifierItem.DiscordMessage == null)
               continue;

            TimeSpan upTimeSpan = DateTime.Now.AddHours(-1) - e.Stream.StartedAt;

            DiscordEmbedBuilder discordEmbedBuilder = new();
            discordEmbedBuilder.Color = DiscordColor.Gray;


            GetUsersResponse getUsersResponse = API.Helix.Users.GetUsersAsync(new List<string> { e.Stream.UserId }).Result;
            string userIconUrl = getUsersResponse.Users[0].ProfileImageUrl.Replace("{width}", "400").Replace("{height}", "400");

            discordEmbedBuilder.WithAuthor($"{e.Stream.UserName} is Offline", $"https://www.twitch.tv/{e.Channel}", userIconUrl);
            discordEmbedBuilder.WithFooter(e.Stream.Title, userIconUrl);

            string userOfflineImageUrl = getUsersResponse.Users[0].OfflineImageUrl.Replace("{width}", "1920").Replace("{height}", "1080");
            discordEmbedBuilder.WithImageUrl(userOfflineImageUrl);

            discordEmbedBuilder.AddField(new DiscordEmbedField("Stream", e.Stream.UserName, true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("18+", e.Stream.IsMature.ToString(), true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("UpTime:", $"{upTimeSpan:hh\\:mm\\:ss}", true));

            discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{e.Channel}");

            discordEmbedBuilder.WithTimestamp(DateTime.Now);

            DiscordComponentEmoji discordComponentEmoji = new(Bot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560));
            DiscordLinkButtonComponent discordLinkButtonComponent = new($"https://www.twitch.tv/{e.Channel}", "Open Twitch", false, discordComponentEmoji);

            try
            {
               _ = twitchNotifierItem.DiscordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordLinkButtonComponent));
            }
            catch
            {
               //ignore
            }
         }
      }

      internal static void MonitorOnOnServiceTick(object sender, OnServiceTickArgs e)
      {
         //Console.WriteLine("Checked");
      }

      internal static void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
      {
         foreach (TwitchNotifier twitchNotifierItem in twitchNotifiers)
         {
            if (twitchNotifierItem.DiscordMessage == null)
               continue;

            DiscordGuild discordGuild = Bot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildId).Result;
            DiscordRole discordRole = discordGuild.GetRole(twitchNotifierItem.DiscordRoleId);

            TimeSpan upTimeSpan = DateTime.Now.AddHours(-1) - e.Stream.StartedAt;

            DiscordEmbedBuilder discordEmbedBuilder = new();
            discordEmbedBuilder.Color = DiscordColor.Purple;
            discordEmbedBuilder.WithDescription($"{discordRole.Mention}");

            discordEmbedBuilder.AddField(new DiscordEmbedField("Game", e.Stream.GameName, true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("Stream title", e.Stream.Title, true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("Stream", e.Stream.UserName, true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("ViewerCount", e.Stream.ViewerCount.ToString(), true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("18+", e.Stream.IsMature.ToString(), true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("UpTime:", $"{upTimeSpan:hh\\:mm\\:ss}", true));

            discordEmbedBuilder.WithImageUrl(e.Stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080"));
            discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{e.Channel}");


            if (e.Stream.GameId != null)
            {
               GetGamesResponse getGamesResponse = API.Helix.Games.GetGamesAsync(new List<string> { e.Stream.GameId }).Result;
               string gameIconUrl = getGamesResponse.Games[0].BoxArtUrl.Replace("{width}", "285").Replace("{height}", "380");
               discordEmbedBuilder.WithThumbnail(gameIconUrl);
            }

            GetUsersResponse getUsersResponse = API.Helix.Users.GetUsersAsync(new List<string> { e.Stream.UserId }).Result;
            string userIconUrl = getUsersResponse.Users[0].ProfileImageUrl.Replace("{width}", "400").Replace("{height}", "400");
            discordEmbedBuilder.WithAuthor($"{e.Stream.UserName} is Live", $"https://www.twitch.tv/{e.Channel}", userIconUrl);
            discordEmbedBuilder.WithFooter(e.Stream.Title, userIconUrl);
            discordEmbedBuilder.WithTimestamp(DateTime.Now);


            DiscordComponentEmoji discordComponentEmoji = new(Bot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560));
            DiscordLinkButtonComponent discordLinkButtonComponent = new($"https://www.twitch.tv/{e.Channel}", "Open stream", false, discordComponentEmoji);

            try
            {
               _ = twitchNotifierItem.DiscordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordLinkButtonComponent));
            }
            catch
            {
               //ignore
            }
         }
      }

      internal static void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e)
      {
         Console.WriteLine("Monitor_OnChannelsSet");
      }

      internal static void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e)
      {
         Console.WriteLine("Monitor_OnServiceStarted");
      }
   }
}