using DisCatSharp.Entities;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Persistence.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
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
      public override string ToString()
      {
         return $"DiscordGuildId:{DiscordGuildId}" +
                $"\nDiscordMemberId:{DiscordMemberId}" +
                $"\nDiscordChannelId:{DiscordChannelId}" +
                $"\nDiscordRoleId:{DiscordRoleId}" +
                $"\nTwitchUserId:{TwitchUserId}" +
                $"\nTwitchChannelUrl:{TwitchChannelUrl}";
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
               var discordGuild = Bot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildId).Result;
               var discordChannel = Bot.DiscordClient.GetChannelAsync(twitchNotifierItem.DiscordChannelId).Result;
               var discordRole = discordGuild.GetRole(twitchNotifierItem.DiscordRoleId);

               TimeSpan upTimeSpan = DateTime.Now - e.Stream.StartedAt;


               DiscordEmbedBuilder discordEmbedBuilder = new();
               discordEmbedBuilder.Color = DiscordColor.Purple;
               discordEmbedBuilder.Title = $"{e.Channel} went Live";
               discordEmbedBuilder.WithDescription($"{discordRole.Mention}" +
                                                   $"\n Stream title: {e.Stream.Title}" +
                                                   $"\n Streamer: {e.Channel}" +
                                                   $"\n Link: https://www.twitch.tv/{e.Channel}" +
                                                   $"\n Viewer: {e.Stream.ViewerCount}" +
                                                   $"\n Game: {e.Stream.GameName}" +
                                                   $"\n 18+: {e.Stream.IsMature}" +
                                                   $"\n UpTime: {upTimeSpan}");


               discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{e.Channel}");

               DiscordLinkButtonComponent discordLinkButtonComponent = new($"https://www.twitch.tv/{e.Channel}", "Open stream");

               _ = discordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordLinkButtonComponent)).Result;
            }
         }

         Console.WriteLine("Monitor_OnStreamOnline");
      }

      internal static void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
      {
         Console.WriteLine("Monitor_OnStreamOffline");
      }

      internal static void MonitorOnOnServiceTick(object sender, OnServiceTickArgs e)
      {
         //Console.WriteLine("Checked");
      }

      internal static void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
      {
         //Console.WriteLine("Monitor_OnStreamUpdate");
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