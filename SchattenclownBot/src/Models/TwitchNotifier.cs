#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Persistence.DatabaseAccess;
using SchattenclownBot.Utils;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using Stream = TwitchLib.Api.Helix.Models.Streams.GetStreams.Stream;

#pragma warning disable CA1822

namespace SchattenclownBot.Models
{
    public class TwitchNotifier
    {
        [NotMapped]
        public static TwitchAPI TwitchAPI { get; set; } = null!;

        [NotMapped]
        public static LiveStreamMonitorService LiveStreamMonitorService { get; set; } = null!;

        [NotMapped]
        public static List<TwitchNotifier> TwitchNotifiers { get; set; } = new();

        [Key]
        public int ID { get; set; }

        [Required]
        public ulong DiscordGuildID { get; set; }

        [Required]
        public ulong DiscordMemberID { get; set; }

        [Required]
        public ulong DiscordChannelID { get; set; }

        [Required]
        public ulong DiscordRoleID { get; set; }

        [Required]
        public ulong TwitchUserID { get; set; }

        [Required]
        public string TwitchChannelUrl { get; set; } = null!;

        [NotMapped]
        public DiscordMessage? DiscordMessage { get; set; }

        public override string ToString()
        {
            DiscordGuild discordGuild = DiscordBot.DiscordClient.GetGuildAsync(DiscordGuildID).Result;
            DiscordChannel discordChannel = DiscordBot.DiscordClient.GetChannelAsync(DiscordChannelID).Result;
            DiscordRole discordRole = discordGuild.GetRole(DiscordRoleID);

            return $"``{"DiscordGuild:",-18}``{discordGuild.Name}" + $"\n``{"DiscordMember:",-18}``<@{DiscordMemberID}>" + $"\n``{"DiscordChannel:",-18}``{discordChannel.Mention}" + $"\n``{"DiscordRole:",-18}``{discordRole.Mention}" + $"\n``{"TwitchUserID:",-18}``{TwitchUserID}" + $"\n``{"TwitchChannelUrl:",-18}``{TwitchChannelUrl}";
        }

        public void Add(TwitchNotifier twitchNotifier)
        {
            new TwitchNotifierDBA().Add(twitchNotifier);
        }

        public List<TwitchNotifier> Read()
        {
            return new TwitchNotifierDBA().Read();
        }

        public List<TwitchNotifier> ReadBasedOnGuild(ulong guildId)
        {
            return new TwitchNotifierDBA().ReadBasedOnGuild(guildId);
        }

        public void RunAsync()
        {
            new CustomLogger().Information("Starting TwitchNotifier...", ConsoleColor.Green);
            Task.Run(async () =>
            {
                //https://twitchtokengenerator.com/
                TwitchAPI = new TwitchAPI
                {
                            Settings =
                            {
                                        ClientId = Program.Config["APIKeys:TwitchOAuth2ClientId"],
                                        AccessToken = Program.Config["APIKeys:TwitchOAuth2ClientSecret"]
                            }
                };

                bool levelSystemVirgin = true;
                do
                {
                    if (DiscordBot.DiscordClient.Guilds.ToList().Count != 0)
                    {
                        TwitchNotifiers.AddRange(new TwitchNotifier().Read());

                        levelSystemVirgin = false;
                    }

                    await Task.Delay(1000);
                } while (levelSystemVirgin);

                _ = Task.Run(async () =>
                {
                    while (true)
                    {
                        DiscordGuild discordGuild = await DiscordBot.DiscordClient.GetGuildAsync(807254423592108032);
                        foreach (DiscordMember discordMember in discordGuild.Members.Values)
                        {
                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            if (discordMember.Presence is { Activities: not null } && discordMember.Presence.Activities.Any(x => x.StreamUrl != null! && x.StreamUrl.Contains("twitch")))
                            {
                                TwitchNotifier twitchNotifierObj = new()
                                {
                                            DiscordGuildID = 807254423592108032,
                                            DiscordMemberID = discordMember.Id,
                                            DiscordChannelID = 1050190278838997032,
                                            DiscordRoleID = 1052993779570839632
                                };

                                string twitchUrl = discordMember.Presence.Activities.FirstOrDefault(x => x.StreamUrl != null! && x.StreamUrl.Contains("twitch"))?.StreamUrl.ToString()!;

                                twitchNotifierObj.TwitchChannelUrl = new StringCutter().RemoveUntil(twitchUrl, "https://www.twitch.tv/", "https://www.twitch.tv/".Length);

                                List<TwitchNotifier> twitchNotifiers = new TwitchNotifier().ReadBasedOnGuild(discordMember.Presence.Guild.Id);

                                if (twitchNotifiers.All(x => x.TwitchChannelUrl != twitchNotifierObj.TwitchChannelUrl))
                                {
                                    new TwitchNotifier().Add(twitchNotifierObj);
                                    new TwitchNotifier().SetMonitoring();
                                }
                            }
                            // ReSharper restore ConditionIsAlwaysTrueOrFalse
                        }

                        await Task.Delay(60000);
                    }
                });

                new TwitchNotifier().SetMonitoring();
            });
        }

        /*public Task OnPresenceUpdated(object sender, PresenceUpdateEventArgs eventArgs)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (eventArgs.PresenceAfter.Activity?.StreamUrl == null! || eventArgs.PresenceAfter.Guild.Id != 807254423592108032)
            {
                return Task.CompletedTask;
            }

            TwitchNotifier twitchNotifierObj = new()
            {
                        DiscordGuildID = eventArgs.PresenceAfter.Guild.Id,
                        DiscordMemberID = eventArgs.PresenceAfter.User.Id,
                        DiscordChannelID = 1050190278838997032
            };

            if ((bool)eventArgs.PresenceAfter.Activity?.StreamUrl.Contains("twitch"))
            {
                twitchNotifierObj.TwitchChannelUrl = eventArgs.PresenceAfter.Activity.StreamUrl;

                if (twitchNotifierObj.TwitchChannelUrl.Contains("https://"))
                {
                    twitchNotifierObj.TwitchChannelUrl = new StringCutter().RemoveUntil(twitchNotifierObj.TwitchChannelUrl, "https://www.twitch.tv/", "https://www.twitch.tv/".Length);
                }

                List<TwitchNotifier> twitchNotifiers = new TwitchNotifierDBA().ReadBasedOnGuild(eventArgs.PresenceAfter.Guild.Id);

                if (twitchNotifiers.All(x => x.TwitchChannelUrl != twitchNotifierObj.TwitchChannelUrl))
                {
                    new TwitchNotifier().Add(twitchNotifierObj);
                    new TwitchNotifier().SetMonitoring();
                }
            }

            return Task.CompletedTask;
        }*/

        public void SetMonitoring()
        {
            foreach (TwitchNotifier twitchNotifier in new TwitchNotifier().Read())
            {
                if (TwitchNotifiers.Any(x => x.TwitchChannelUrl == twitchNotifier.TwitchChannelUrl && x.DiscordChannelID == twitchNotifier.DiscordChannelID && x.DiscordGuildID == twitchNotifier.DiscordGuildID && x.DiscordMemberID == twitchNotifier.DiscordMemberID))
                {
                }
                else
                {
                    TwitchNotifiers.Add(twitchNotifier);
                }
            }

            List<string> list = (from twitchNotifierItem in TwitchNotifiers where twitchNotifierItem.TwitchChannelUrl != "" select twitchNotifierItem.TwitchChannelUrl).ToList();

            LiveStreamMonitorService = new LiveStreamMonitorService(TwitchAPI);

            LiveStreamMonitorService.SetChannelsByName(list);
            LiveStreamMonitorService.OnStreamOnline += new TwitchNotifier().Monitor_OnStreamOnline;
            LiveStreamMonitorService.OnStreamOffline += new TwitchNotifier().Monitor_OnStreamOffline;
            LiveStreamMonitorService.OnStreamUpdate += new TwitchNotifier().Monitor_OnStreamUpdate;
            LiveStreamMonitorService.OnServiceStarted += new TwitchNotifier().Monitor_OnServiceStarted;
            LiveStreamMonitorService.OnChannelsSet += new TwitchNotifier().Monitor_OnChannelsSet;
            LiveStreamMonitorService.OnServiceTick += new TwitchNotifier().MonitorOnServiceTick;

            LiveStreamMonitorService.Start();
        }

        public void Monitor_OnStreamOnline(object? sender, OnStreamOnlineArgs e)
        {
            foreach (TwitchNotifier twitchNotifierItem in TwitchNotifiers)
            {
                if (twitchNotifierItem.TwitchChannelUrl != e.Channel)
                {
                    continue;
                }

                if (twitchNotifierItem.DiscordMessage! != null!)
                {
                    continue;
                }

                DiscordGuild discordGuild = DiscordBot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildID).Result;
                DiscordChannel discordChannel = DiscordBot.DiscordClient.GetChannelAsync(twitchNotifierItem.DiscordChannelID).Result;
                DiscordRole discordRole = discordGuild.GetRole(twitchNotifierItem.DiscordRoleID);

                TimeSpan upTimeSpan = DateTime.UtcNow - e.Stream.StartedAt;

                DiscordEmbedBuilder discordEmbedBuilder = new()
                {
                            Color = DiscordColor.Purple
                };
                discordEmbedBuilder.WithDescription($"{discordRole.Mention}");

                //discordEmbedBuilder.AddField(new DiscordEmbedField("Game", e.Stream.GameName, true));
                //discordEmbedBuilder.AddField(new DiscordEmbedField("Stream title", e.Stream.Title, true));
                discordEmbedBuilder.AddField(new DiscordEmbedField("Stream", e.Stream.UserName, true));
                discordEmbedBuilder.AddField(new DiscordEmbedField("ViewerCount", e.Stream.ViewerCount.ToString(), true));
                //discordEmbedBuilder.AddField(new DiscordEmbedField("18+", e.Stream.IsMature.ToString(), true));
                discordEmbedBuilder.AddField(new DiscordEmbedField("UpTime:", $"{upTimeSpan:hh\\:mm\\:ss}", true));


                string[] fileEntries = Directory.GetFiles(@"A:\binhex-nginx\nginx\html\TwitchNotifier\Images");
                foreach (string fileName in fileEntries.Where(x => x.Contains(@$"A:\binhex-nginx\nginx\html\TwitchNotifier\Images\{e.Stream.UserName.ToLower()}-")))
                {
                    File.Delete(fileName);
                }

                using (HttpClient client = new())
                {
                    byte[] imageBytes = client.GetByteArrayAsync(e.Stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080")).Result;
                    using MemoryStream memoryStream = new(imageBytes);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        Bitmap image = new(memoryStream);
                        image.Save(@$"A:\binhex-nginx\nginx\html\TwitchNotifier\Images\{e.Stream.UserName.ToLower()}-{upTimeSpan.Hours}-{upTimeSpan.Minutes}.jpg");
                    }
                }

                discordEmbedBuilder.WithImageUrl($"https://app.0x360x39.de/TwitchNotifier/Images/{e.Stream.UserName.ToLower()}-{upTimeSpan.Hours}-{upTimeSpan.Minutes}.jpg");

                discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{e.Channel.ToLower()}");


                if (e.Stream.GameId != null)
                {
                    GetGamesResponse getGamesResponse = TwitchAPI.Helix.Games.GetGamesAsync(new List<string>
                                {
                                            e.Stream.GameId
                                })
                                .Result;
                    string gameIconUrl = getGamesResponse.Games[0].BoxArtUrl.Replace("{width}", "285").Replace("{height}", "380");
                    discordEmbedBuilder.WithThumbnail(gameIconUrl);
                }

                GetUsersResponse getUsersResponse = TwitchAPI.Helix.Users.GetUsersAsync(new List<string>
                            {
                                        e.Stream.UserId
                            })
                            .Result;
                string userIconUrl = getUsersResponse.Users[0].ProfileImageUrl.Replace("{width}", "400").Replace("{height}", "400");
                discordEmbedBuilder.WithAuthor($"{e.Stream.UserName} is Live", $"https://www.twitch.tv/{e.Channel}", userIconUrl);
                discordEmbedBuilder.WithFooter(e.Stream.Title, userIconUrl);
                discordEmbedBuilder.WithTimestamp(DateTime.Now);


                DiscordComponentEmoji discordComponentEmoji = new(DiscordBot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560)!);
                DiscordLinkButtonComponent discordLinkButtonComponent = new($"https://www.twitch.tv/{e.Channel}", "Open stream", false, discordComponentEmoji);

                twitchNotifierItem.DiscordMessage = discordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordLinkButtonComponent).WithContent($"https://www.twitch.tv/{e.Channel}")).Result;
            }

            new CustomLogger().Information("Monitor_OnStreamOnline", ConsoleColor.Green);
        }

        public void Monitor_OnStreamOffline(object? sender, OnStreamOfflineArgs e)
        {
            foreach (TwitchNotifier twitchNotifierItem in TwitchNotifiers)
            {
                if (twitchNotifierItem.TwitchChannelUrl != e.Channel)
                {
                    continue;
                }

                if (twitchNotifierItem.DiscordMessage! == null!)
                {
                    continue;
                }

                TimeSpan upTimeSpan = DateTime.UtcNow - e.Stream.StartedAt;

                DiscordEmbedBuilder discordEmbedBuilder = new()
                {
                            Color = DiscordColor.Gray
                };

                GetUsersResponse getUsersResponse = TwitchAPI.Helix.Users.GetUsersAsync(new List<string>
                            {
                                        e.Stream.UserId
                            })
                            .Result;
                string userIconUrl = getUsersResponse.Users[0].ProfileImageUrl.Replace("{width}", "400").Replace("{height}", "400");

                discordEmbedBuilder.WithAuthor($"{e.Stream.UserName} is Offline", $"https://www.twitch.tv/{e.Channel}", userIconUrl);
                discordEmbedBuilder.WithFooter(e.Stream.Title, userIconUrl);

                string userOfflineImageUrl = getUsersResponse.Users[0].OfflineImageUrl.Replace("{width}", "1920").Replace("{height}", "1080");
                discordEmbedBuilder.WithImageUrl(userOfflineImageUrl);

                discordEmbedBuilder.AddField(new DiscordEmbedField("Stream", e.Stream.UserName, true));
                //discordEmbedBuilder.AddField(new DiscordEmbedField("18+", e.Stream.IsMature.ToString(), true));
                discordEmbedBuilder.AddField(new DiscordEmbedField("UpTime:", $"{upTimeSpan:hh\\:mm\\:ss}", true));

                discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{e.Channel}");

                discordEmbedBuilder.WithTimestamp(DateTime.Now);

                DiscordComponentEmoji discordComponentEmoji = new(DiscordBot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560)!);
                DiscordLinkButtonComponent discordLinkButtonComponent = new($"https://www.twitch.tv/{e.Channel}", "Open Twitch", false, discordComponentEmoji);

                try
                {
                    _ = twitchNotifierItem.DiscordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordLinkButtonComponent));
                }
                catch
                {
                    //ignore
                }

                twitchNotifierItem.DiscordMessage = null;
            }

            new CustomLogger().Information("Monitor_OnStreamOffline", ConsoleColor.Green);
        }

        public void MonitorOnServiceTick(object? sender, OnServiceTickArgs e)
        {
            Dictionary<string, Stream>.ValueCollection liveStreamsValues = LiveStreamMonitorService.LiveStreams.Values;

            try
            {
                foreach (Stream stream in liveStreamsValues)
                {
                    foreach (TwitchNotifier twitchNotifierItem in TwitchNotifiers)
                    {
                        if (twitchNotifierItem.TwitchChannelUrl != stream.UserLogin)
                        {
                            continue;
                        }

                        if (twitchNotifierItem.DiscordMessage! == null!)
                        {
                            continue;
                        }

                        DiscordGuild discordGuild = DiscordBot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildID).Result;
                        DiscordRole discordRole = discordGuild.GetRole(twitchNotifierItem.DiscordRoleID);

                        TimeSpan upTimeSpan = DateTime.UtcNow - stream.StartedAt;

                        DiscordEmbedBuilder discordEmbedBuilder = new()
                        {
                                    Color = DiscordColor.Purple
                        };
                        discordEmbedBuilder.WithDescription($"{discordRole.Mention}");

                        discordEmbedBuilder.AddField(new DiscordEmbedField("Game", stream.GameName, true));
                        //discordEmbedBuilder.AddField(new DiscordEmbedField("Stream title", stream.Title, true));
                        //discordEmbedBuilder.AddField(new DiscordEmbedField("Stream", stream.UserName, true));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("ViewerCount", stream.ViewerCount.ToString(), true));
                        //discordEmbedBuilder.AddField(new DiscordEmbedField("18+", stream.IsMature.ToString(), true));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("UpTime:", $"{upTimeSpan:hh\\:mm\\:ss}", true));

                        string[] fileEntries = Directory.GetFiles(@"A:\binhex-nginx\nginx\html\TwitchNotifier\Images");
                        foreach (string fileName in fileEntries.Where(x => x.Contains(@$"A:\binhex-nginx\nginx\html\TwitchNotifier\Images\{stream.UserLogin.ToLower()}-")))
                        {
                            File.Delete(fileName);
                        }

                        using (HttpClient client = new())
                        {
                            byte[] imageBytes = client.GetByteArrayAsync(stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080")).Result;
                            using MemoryStream memoryStream = new(imageBytes);
                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                Bitmap image = new(memoryStream);
                                image.Save(@$"A:\binhex-nginx\nginx\html\TwitchNotifier\Images\{stream.UserLogin.ToLower()}-{upTimeSpan.Hours}-{upTimeSpan.Minutes}.jpg");
                            }
                        }

                        discordEmbedBuilder.WithImageUrl($"https://app.0x360x39.de/TwitchNotifier/Images/{stream.UserLogin.ToLower()}-{upTimeSpan.Hours}-{upTimeSpan.Minutes}.jpg");

                        discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{stream.UserLogin.ToLower()}");


                        if (stream.GameId != null)
                        {
                            GetGamesResponse getGamesResponse = TwitchAPI.Helix.Games.GetGamesAsync(new List<string>
                                        {
                                                    stream.GameId
                                        })
                                        .Result;
                            string gameIconUrl = getGamesResponse.Games[0].BoxArtUrl.Replace("{width}", "285").Replace("{height}", "380");
                            discordEmbedBuilder.WithThumbnail(gameIconUrl);
                        }

                        GetUsersResponse getUsersResponse = TwitchAPI.Helix.Users.GetUsersAsync(new List<string>
                                    {
                                                stream.UserId
                                    })
                                    .Result;
                        string userIconUrl = getUsersResponse.Users[0].ProfileImageUrl.Replace("{width}", "400").Replace("{height}", "400");
                        discordEmbedBuilder.WithAuthor($"{stream.UserName} is Live", $"https://www.twitch.tv/{stream.UserLogin}", userIconUrl);
                        discordEmbedBuilder.WithFooter(stream.Title, userIconUrl);
                        discordEmbedBuilder.WithTimestamp(DateTime.Now);


                        DiscordComponentEmoji discordComponentEmoji = new(DiscordBot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560)!);
                        DiscordLinkButtonComponent discordLinkButtonComponent = new($"https://www.twitch.tv/{stream.UserLogin}", "Open stream", false, discordComponentEmoji);

                        try
                        {
                            _ = twitchNotifierItem.DiscordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordLinkButtonComponent));
                        }
                        catch
                        {
                            //ignore
                        }
                    }

                    new CustomLogger().Information(stream.UserLogin, ConsoleColor.Green);
                    _ = Task.Delay(1000);
                }
            }
            catch
            {
                //ignore
            }

            new CustomLogger().Information("MonitorOnOnServiceTickChecked", ConsoleColor.Green);
        }

        public void Monitor_OnStreamUpdate(object? sender, OnStreamUpdateArgs e)
        {
            /*try
            {
               foreach (TwitchNotifier twitchNotifierItem in twitchNotifiers)
               {
                  if (twitchNotifierItem.TwitchChannelUrl != e.Channel)
                     continue;

                  if (twitchNotifierItem.DiscordMessage == null)
                     continue;

                  DiscordGuild discordGuild = DiscordBot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildID).Result;
                  DiscordRole discordRole = discordGuild.GetRole(twitchNotifierItem.DiscordRoleID);

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


                  DiscordComponentEmoji discordComponentEmoji = new(DiscordBot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560));
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
            catch
            {
               //ignore
            }*/
        }

        public void Monitor_OnChannelsSet(object? sender, OnChannelsSetArgs e)
        {
            new CustomLogger().Information("Monitor_OnChannelsSet", ConsoleColor.Green);
        }

        public void Monitor_OnServiceStarted(object? sender, OnServiceStartedArgs e)
        {
            new CustomLogger().Information("Monitor_OnServiceStarted", ConsoleColor.Green);
        }
    }
}