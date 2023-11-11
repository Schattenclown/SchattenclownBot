using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.DataAccess.MySQL.Services;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Helix.Models.Users.GetUsers;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace SchattenclownBot.Integrations.Discord.Services
{
    public class TwitchNotifier
    {
        public TwitchAPI Api;
        public LiveStreamMonitorService Monitor;
        public List<TwitchNotifier> TwitchNotifiers = new();
        public ulong DiscordGuildId { get; set; }
        public ulong DiscordMemberId { get; set; }
        public ulong DiscordChannelId { get; set; }
        public ulong DiscordRoleId { get; set; }
        public ulong TwitchUserId { get; set; }
        public string TwitchChannelUrl { get; set; }
        public DiscordMessage DiscordMessage { get; set; }

        public override string ToString()
        {
            DiscordGuild discordGuild = DiscordBot.DiscordClient.GetGuildAsync(DiscordGuildId).Result;
            DiscordChannel discordChannel = DiscordBot.DiscordClient.GetChannelAsync(DiscordChannelId).Result;
            DiscordRole discordRole = discordGuild.GetRole(DiscordRoleId);

            return $"``{"DiscordGuild:",-18}``{discordGuild.Name}" + $"\n``{"DiscordMember:",-18}``<@{DiscordMemberId}>" + $"\n``{"DiscordChannel:",-18}``{discordChannel.Mention}" + $"\n``{"DiscordRole:",-18}``{discordRole.Mention}" + $"\n``{"TwitchUserId:",-18}``{TwitchUserId}" + $"\n``{"TwitchChannelUrl:",-18}``{TwitchChannelUrl}";
        }

        public List<TwitchNotifier> Read(ulong guildId)
        {
            return new DbTwitchNotifier().Read(guildId);
        }

        public void Add(TwitchNotifier twitchNotifier)
        {
            new DbTwitchNotifier().Add(twitchNotifier);
        }

        public async Task CreateTable()
        {
            bool levelSystemVirgin = true;
            do
            {
                if (DiscordBot.DiscordClient.Guilds.ToList().Count != 0)
                {
                    List<KeyValuePair<ulong, DiscordGuild>> guildsList = DiscordBot.DiscordClient.Guilds.ToList();
                    foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
                    {
                        new CustomLogger().Information($"Creating TwitchNotifier table for {guildItem.Value.Name}", ConsoleColor.Green);
                        await new DbTwitchNotifier().CreateTable(guildItem.Value.Id);
                    }

                    levelSystemVirgin = false;
                }

                await Task.Delay(1000);
            } while (levelSystemVirgin);
        }

        public void RunAsync()
        {
            new CustomLogger().Information("Starting TwitchNotifier...", ConsoleColor.Green);
            Task.Run(async () =>
            {
                await CreateTable();

                //https://twitchtokengenerator.com/
                Api = new TwitchAPI
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
                        List<KeyValuePair<ulong, DiscordGuild>> guildsList = DiscordBot.DiscordClient.Guilds.ToList();
                        foreach (KeyValuePair<ulong, DiscordGuild> guildItem in guildsList)
                        {
                            TwitchNotifiers.AddRange(Read(guildItem.Value.Id));
                        }

                        levelSystemVirgin = false;
                    }

                    await Task.Delay(1000);
                } while (levelSystemVirgin);

                SetMonitoring();
            });
        }

        public void SetMonitoring()
        {
            List<KeyValuePair<ulong, DiscordGuild>> guildsList = DiscordBot.DiscordClient.Guilds.ToList();
            foreach (TwitchNotifier y in guildsList.SelectMany(guildItem => Read(guildItem.Value.Id)))
            {
                if (TwitchNotifiers.Any(x => x.TwitchChannelUrl == y.TwitchChannelUrl && x.DiscordChannelId == y.DiscordChannelId && x.DiscordGuildId == y.DiscordGuildId && x.DiscordMemberId == y.DiscordMemberId && x.DiscordMemberId == y.DiscordMemberId))
                {
                }
                else
                {
                    TwitchNotifiers.Add(y);
                }
            }

            List<string> list = (from twitchNotifierItem in TwitchNotifiers where twitchNotifierItem.TwitchChannelUrl != "" select twitchNotifierItem.TwitchChannelUrl).ToList();

            Monitor = new LiveStreamMonitorService(Api);

            Monitor.SetChannelsByName(list);
            Monitor.OnStreamOnline += Monitor_OnStreamOnline;
            Monitor.OnStreamOffline += Monitor_OnStreamOffline;
            Monitor.OnStreamUpdate += Monitor_OnStreamUpdate;
            Monitor.OnServiceStarted += Monitor_OnServiceStarted;
            Monitor.OnChannelsSet += Monitor_OnChannelsSet;
            Monitor.OnServiceTick += MonitorOnOnServiceTick;

            Monitor.Start();
        }

        public void Monitor_OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            foreach (TwitchNotifier twitchNotifierItem in TwitchNotifiers)
            {
                if (twitchNotifierItem.TwitchChannelUrl != e.Channel)
                {
                    continue;
                }

                if (twitchNotifierItem.DiscordMessage != null)
                {
                    continue;
                }

                DiscordGuild discordGuild = DiscordBot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildId).Result;
                DiscordChannel discordChannel = DiscordBot.DiscordClient.GetChannelAsync(twitchNotifierItem.DiscordChannelId).Result;
                DiscordRole discordRole = discordGuild.GetRole(twitchNotifierItem.DiscordRoleId);

                TimeSpan upTimeSpan = DateTime.UtcNow - e.Stream.StartedAt;

                DiscordEmbedBuilder discordEmbedBuilder = new()
                {
                            Color = DiscordColor.Purple
                };
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
                    GetGamesResponse getGamesResponse = Api.Helix.Games.GetGamesAsync(new List<string>
                                {
                                            e.Stream.GameId
                                })
                                .Result;
                    string gameIconUrl = getGamesResponse.Games[0].BoxArtUrl.Replace("{width}", "285").Replace("{height}", "380");
                    discordEmbedBuilder.WithThumbnail(gameIconUrl);
                }

                GetUsersResponse getUsersResponse = Api.Helix.Users.GetUsersAsync(new List<string>
                            {
                                        e.Stream.UserId
                            })
                            .Result;
                string userIconUrl = getUsersResponse.Users[0].ProfileImageUrl.Replace("{width}", "400").Replace("{height}", "400");
                discordEmbedBuilder.WithAuthor($"{e.Stream.UserName} is Live", $"https://www.twitch.tv/{e.Channel}", userIconUrl);
                discordEmbedBuilder.WithFooter(e.Stream.Title, userIconUrl);
                discordEmbedBuilder.WithTimestamp(DateTime.Now);


                DiscordComponentEmoji discordComponentEmoji = new(DiscordBot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560));
                DiscordLinkButtonComponent discordLinkButtonComponent = new($"https://www.twitch.tv/{e.Channel}", "Open stream", false, discordComponentEmoji);

                twitchNotifierItem.DiscordMessage = discordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder.Build()).AddComponents(discordLinkButtonComponent).WithContent($"https://www.twitch.tv/{e.Channel}")).Result;
            }

            new CustomLogger().Information("Monitor_OnStreamOnline", ConsoleColor.Green);
        }

        public void Monitor_OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            foreach (TwitchNotifier twitchNotifierItem in TwitchNotifiers)
            {
                if (twitchNotifierItem.TwitchChannelUrl != e.Channel)
                {
                    continue;
                }

                if (twitchNotifierItem.DiscordMessage == null)
                {
                    continue;
                }

                TimeSpan upTimeSpan = DateTime.UtcNow - e.Stream.StartedAt;

                DiscordEmbedBuilder discordEmbedBuilder = new()
                {
                            Color = DiscordColor.Gray
                };

                GetUsersResponse getUsersResponse = Api.Helix.Users.GetUsersAsync(new List<string>
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
                discordEmbedBuilder.AddField(new DiscordEmbedField("18+", e.Stream.IsMature.ToString(), true));
                discordEmbedBuilder.AddField(new DiscordEmbedField("UpTime:", $"{upTimeSpan:hh\\:mm\\:ss}", true));

                discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{e.Channel}");

                discordEmbedBuilder.WithTimestamp(DateTime.Now);

                DiscordComponentEmoji discordComponentEmoji = new(DiscordBot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560));
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

        public void MonitorOnOnServiceTick(object sender, OnServiceTickArgs e)
        {
            Dictionary<string, Stream>.ValueCollection liveStreamsValues = Monitor.LiveStreams.Values;

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

                        if (twitchNotifierItem.DiscordMessage == null)
                        {
                            continue;
                        }

                        DiscordGuild discordGuild = DiscordBot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildId).Result;
                        DiscordRole discordRole = discordGuild.GetRole(twitchNotifierItem.DiscordRoleId);

                        TimeSpan upTimeSpan = DateTime.UtcNow - stream.StartedAt;

                        DiscordEmbedBuilder discordEmbedBuilder = new()
                        {
                                    Color = DiscordColor.Purple
                        };
                        discordEmbedBuilder.WithDescription($"{discordRole.Mention}");

                        discordEmbedBuilder.AddField(new DiscordEmbedField("Game", stream.GameName, true));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("Stream title", stream.Title, true));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("Stream", stream.UserName, true));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("ViewerCount", stream.ViewerCount.ToString(), true));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("18+", stream.IsMature.ToString(), true));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("UpTime:", $"{upTimeSpan:hh\\:mm\\:ss}", true));

                        discordEmbedBuilder.WithImageUrl(stream.ThumbnailUrl.Replace("{width}", "1920").Replace("{height}", "1080"));
                        discordEmbedBuilder.WithUrl($"https://www.twitch.tv/{stream.UserLogin}");


                        if (stream.GameId != null)
                        {
                            GetGamesResponse getGamesResponse = Api.Helix.Games.GetGamesAsync(new List<string>
                                        {
                                                    stream.GameId
                                        })
                                        .Result;
                            string gameIconUrl = getGamesResponse.Games[0].BoxArtUrl.Replace("{width}", "285").Replace("{height}", "380");
                            discordEmbedBuilder.WithThumbnail(gameIconUrl);
                        }

                        GetUsersResponse getUsersResponse = Api.Helix.Users.GetUsersAsync(new List<string>
                                    {
                                                stream.UserId
                                    })
                                    .Result;
                        string userIconUrl = getUsersResponse.Users[0].ProfileImageUrl.Replace("{width}", "400").Replace("{height}", "400");
                        discordEmbedBuilder.WithAuthor($"{stream.UserName} is Live", $"https://www.twitch.tv/{stream.UserLogin}", userIconUrl);
                        discordEmbedBuilder.WithFooter(stream.Title, userIconUrl);
                        discordEmbedBuilder.WithTimestamp(DateTime.Now);


                        DiscordComponentEmoji discordComponentEmoji = new(DiscordBot.EmojiDiscordGuild.GetEmojisAsync().Result.FirstOrDefault(x => x.Id == 1050340762459586560));
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

        public void Monitor_OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            /*try
            {
               foreach (TwitchNotifier twitchNotifierItem in twitchNotifiers)
               {
                  if (twitchNotifierItem.TwitchChannelUrl != e.Channel)
                     continue;

                  if (twitchNotifierItem.DiscordMessage == null)
                     continue;

                  DiscordGuild discordGuild = DiscordBot.DiscordClient.GetGuildAsync(twitchNotifierItem.DiscordGuildId).Result;
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

        public void Monitor_OnChannelsSet(object sender, OnChannelsSetArgs e)
        {
            new CustomLogger().Information("Monitor_OnChannelsSet", ConsoleColor.Green);
        }

        public void Monitor_OnServiceStarted(object sender, OnServiceStartedArgs e)
        {
            new CustomLogger().Information("Monitor_OnServiceStarted", ConsoleColor.Green);
        }
    }
}