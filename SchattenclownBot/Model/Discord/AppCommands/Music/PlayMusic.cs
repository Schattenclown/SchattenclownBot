using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SpotifyAPI.Web;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using File = TagLib.File;

// ReSharper disable UnusedMember.Local
#pragma warning disable CS4014

// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands.Music;

internal class PlayMusic : ApplicationCommandsModule
{
   private static readonly List<DcCancellationTokenItem> CancellationTokenItemList = new();
   public static readonly List<QueueTrack> QueueTracks = new();
   public static readonly List<QueueTrack> CompareQueueTracks = new();

   public static async void NextTrackRequestApi(API aPI)
   {
      CwLogger.Write(aPI.RequestTimeStamp + " " + aPI.RequesterIp + " " + aPI.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__5", "").Replace("<", ""), ConsoleColor.DarkYellow);
      API.DELETE(aPI.CommandRequestId);

      GCM gCM = new();

      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
      {
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPI.RequestDiscordUserId))
         {
            gCM.DiscordGuild = guildItem;
            gCM.DiscordMember = memberItem;
            gCM.DiscordChannel = memberItem.VoiceState.Channel;
            break;
         }
      }

      if (gCM.DiscordMember == null)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         {
            foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == aPI.RequestDiscordUserId))
            {
               await memberItem.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return;
            }
         }
      }

      PlayNextTrackFromQueue(gCM);
   }

   public static async void PreviousTrackRequestApi(API aPI)
   {
      CwLogger.Write(aPI.RequestTimeStamp + " " + aPI.RequesterIp + " " + aPI.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__6", "").Replace("<", ""), ConsoleColor.DarkYellow);
      API.DELETE(aPI.CommandRequestId);

      GCM gCM = new();

      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
      foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPI.RequestDiscordUserId))
      {
         gCM.DiscordGuild = guildItem;
         gCM.DiscordMember = memberItem;
         gCM.DiscordChannel = memberItem.VoiceState.Channel;
         break;
      }

      if (gCM.DiscordMember == null)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == aPI.RequestDiscordUserId))
         {
            await memberItem.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return;
         }
      }

      PlayPreviousTrackFromQueue(gCM);
   }

   private static bool NoMusicPlaying(DiscordGuild discordGuild)
   {
      return CancellationTokenItemList.All(cancellationTokenItem => cancellationTokenItem.DiscordGuild != discordGuild);
   }

   public static SpotifyClient GetSpotifyClientConfig()
   {
      SpotifyClientConfig spotifyClientConfig = SpotifyClientConfig.CreateDefault();
      ClientCredentialsRequest clientCredentialsRequest = new(Bot.Connections.Token.ClientId, Bot.Connections.Token.ClientSecret);
      ClientCredentialsTokenResponse clientCredentialsTokenResponse = new OAuthClient(spotifyClientConfig).RequestToken(clientCredentialsRequest).Result;
      SpotifyClient spotifyClient = new(clientCredentialsTokenResponse.AccessToken);
      return spotifyClient;
   }

   [SlashCommand("Play" + Bot.isDevBot, "Play Spotify or YouTube links!")]
   private async Task PlayCommand(InteractionContext interactionContext, [Option("Link", "Link!")] string webLink)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
      await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Working on it."));
      AddTracksToQueueAsyncTask(interactionContext.Member.Id, webLink, false);
   }

   public static void API_PlayRequest(API api)
   {
      AddTracksToQueueAsyncTask(api.RequestDiscordUserId, api.Data, false);
   }

   public static void API_ShufflePlayRequest(API api)
   {
      AddTracksToQueueAsyncTask(api.RequestDiscordUserId, api.Data, true);
   }

   private static async Task AddTracksToQueueAsyncTask(ulong requestDiscordUserId, string webLink, bool isShufflePlay)
   {
      GCM gCM = new();

      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
      foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == requestDiscordUserId && x.VoiceState != null))
      {
         gCM.DiscordGuild = guildItem;
         gCM.DiscordMember = memberItem;
         gCM.DiscordChannel = memberItem.VoiceState.Channel;
         break;
      }

      if (gCM.DiscordMember == null)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == requestDiscordUserId))
         {
            await memberItem.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return;
         }
      }

      if (gCM.DiscordGuild == null || gCM.DiscordChannel == null)
      {
         return;
      }

      if (QueueTracks.Any(x => x.GCM.DiscordChannel == gCM.DiscordChannel && !x.IsAdded))
      {
         int addedCount = QueueTracks.Count(x => x.GCM.DiscordChannel == gCM.DiscordChannel && x.IsAdded);
         int notAddedCount = QueueTracks.Count(x => x.GCM.DiscordChannel == gCM.DiscordChannel && !x.IsAdded);
         await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("The queue is already being generated. Please wait until the first queue is generated! " + $"{addedCount}/ " + $"{notAddedCount} Please wait!")));
         return;
      }

      if (!NoMusicPlaying(gCM.DiscordGuild))
      {
         await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Queue is being created! Please be patient!")));
      }

      Uri webLinkUri;
      try
      {
         webLinkUri = new Uri(webLink);
      }
      catch
      {
         await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Please check your link, something is wrong! The https:// tag may be missing")));
         return;
      }

      int tracksAdded = 0;

      DeterminedStreamingService determinedStreamingService = new DeterminedStreamingService().IdentifyStreamingService(webLink);
      if (determinedStreamingService.Nothing)
      {
         await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Sag ICH!!!")));
         return;
      }

      if (determinedStreamingService.IsYouTube)
      {
         YoutubeClient youtubeClient = new();

         try
         {
            if (determinedStreamingService.IsYouTubePlaylist)
            {
               int playlistSelectedVideoIndex = 1;
               string playlistId = StringCutter.RmAfter(StringCutter.RmAfter(StringCutter.RmAfter(StringCutter.RmUntil(webLink, "&list=", "&list=".Length), "&index=", 0), "&ab_channel=", 0), "&start_radio=", 0);
               bool isYouTubeMix = webLinkUri.AbsoluteUri.Contains("&ab_channel=") || webLinkUri.AbsoluteUri.Contains("&start_radio=");

               if (determinedStreamingService.IsYouTubePlaylistWithIndex)
               {
                  playlistSelectedVideoIndex = Convert.ToInt32(StringCutter.RmAfter(StringCutter.RmAfter(StringCutter.RmUntil(webLink, "&index=", "&index=".Length), "&ab_channel=", 0), "&start_radio=", 0));
               }

               List<PlaylistVideo> playlistVideos = new(await youtubeClient.Playlists.GetVideosAsync(playlistId));

               if (isShufflePlay)
               {
                  playlistVideos = ShufflePlayListForYouTube(playlistVideos);
               }

               if (playlistSelectedVideoIndex != 1 && !isYouTubeMix)
               {
                  playlistVideos.RemoveRange(0, playlistSelectedVideoIndex);
               }

               foreach (PlaylistVideo item in playlistVideos)
               {
                  QueueTracks.Add(new QueueTrack(gCM, new Uri(item.Url), item.Title, item.Author.ChannelTitle));
                  tracksAdded++;
               }
            }
            else
            {
               string selectedVideoId;
               if (webLink.Contains("youtu.be"))
               {
                  selectedVideoId = StringCutter.RmAfter(StringCutter.RmUntil(webLink, "youtu.be/", "youtu.be/".Length), "&list=", 0);
               }
               else
               {
                  selectedVideoId = StringCutter.RmAfter(StringCutter.RmUntil(webLink, "watch?v=", "watch?v=".Length), "&list=", 0);
               }

               Video videoData = await youtubeClient.Videos.GetAsync("https://www.youtube.com/watch?v=" + selectedVideoId);

               QueueTracks.Add(new QueueTrack(gCM, new Uri(videoData.Url), videoData.Title, videoData.Author.ChannelTitle));
               tracksAdded++;
            }
         }
         catch (Exception ex)
         {
            await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error!\n" + ex.Message)));
         }
      }
      else if (determinedStreamingService.IsSpotify)
      {
         SpotifyClient spotifyClient = GetSpotifyClientConfig();
         List<FullTrack> fullTracks = new();

         if (determinedStreamingService.IsSpotifyPlaylist)
         {
            string playlistId = StringCutter.RmAfter(StringCutter.RmUntil(webLink, "/playlist/", "/playlist/".Length), "?si", 0);

            PlaylistGetItemsRequest playlistGetItemsRequest = new()
            {
               Offset = 0
            };

            List<PlaylistTrack<IPlayableItem>> iPlayableItems = spotifyClient.Playlists.GetItems(playlistId, playlistGetItemsRequest).Result.Items;

            if (iPlayableItems is { Count: >= 100 })
            {
               try
               {
                  while (true)
                  {
                     playlistGetItemsRequest.Offset += 100;

                     List<PlaylistTrack<IPlayableItem>> playlistTrackListSecond = spotifyClient.Playlists.GetItems(playlistId, playlistGetItemsRequest).Result.Items;

                     if (playlistTrackListSecond != null)
                     {
                        iPlayableItems.AddRange(playlistTrackListSecond);

                        if (playlistTrackListSecond.Count < 100)
                        {
                           break;
                        }
                     }
                  }
               }
               catch
               {
                  // ignored
               }
            }

            foreach (PlaylistTrack<IPlayableItem> iPlayableItem in iPlayableItems)
            {
               fullTracks.Add(iPlayableItem.Track as FullTrack);
            }
         }
         else if (determinedStreamingService.IsSpotifyAlbum)
         {
            string albumId = StringCutter.RmAfter(StringCutter.RmUntil(StringCutter.RmUntil(webLink, "/album/", "/album/".Length), ":album:", ":album:".Length), "?si", 0);
            Paging<SimpleTrack> simpleTracks = await spotifyClient.Albums.GetTracks(albumId);

            foreach (SimpleTrack simpleTrack in simpleTracks.Items)
            {
               FullTrack fullTrack = await spotifyClient.Tracks.Get(simpleTrack.Id);
               fullTracks.Add(fullTrack);
            }
         }
         else
         {
            string trackId = StringCutter.RmAfter(StringCutter.RmUntil(webLink, "/track/", "/track/".Length), "?si", 0);
            FullTrack fullTrack = spotifyClient.Tracks.Get(trackId).Result;
            fullTracks.Add(fullTrack);
         }

         if (isShufflePlay)
         {
            fullTracks = ShufflePlayListForSpotify(fullTracks);
         }

         List<QueueTrack> queueTracks = new();

         foreach (FullTrack item in fullTracks)
         {
            QueueTrack queueTrack = new(gCM, item);
            queueTracks.Add(queueTrack);
            QueueTracks.Add(queueTrack);
         }

         foreach (QueueTrack item in queueTracks)
         {
            SpotifyQueueAddSearchAsync(item);
            tracksAdded++;
            await Task.Delay(100);
         }
      }

      if (NoMusicPlaying(gCM.DiscordGuild))
      {
         while (!QueueTracks.FirstOrDefault().IsAdded)
         {
            await Task.Delay(1000);
         }

         await PlayFromQueueTask(gCM, QueueTracks.FirstOrDefault());
      }

      if (tracksAdded == 1)
      {
         await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Generating queue for {tracksAdded} track!")));
      }
      else
      {
         await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Generating queue for {tracksAdded} tracks!")));
      }

      if (NoMusicPlaying(gCM.DiscordGuild))
      {
         if (tracksAdded == 1)
         {
            await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"{tracksAdded} track is now added to the queue!")));
         }
         else
         {
            await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"{tracksAdded} tracks are now added to the queue!")));
         }
      }
      else
      {
         if (tracksAdded == 1)
         {
            await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Music is already playing or will at any moment! {tracksAdded} track is now added to the queue!")));
         }
         else
         {
            await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Music is already playing or will at any moment! {tracksAdded} tracks are now added to the queue!")));
         }
      }
   }

   private static List<PlaylistVideo> ShufflePlayListForYouTube(List<PlaylistVideo> playlistVideos)
   {
      List<PlaylistVideo> playlistVideosMixed = new();

      int queueLength = playlistVideos.Count;
      List<int> intListMixed = new();

      for (int i = 0; i < queueLength; i++)
      {
         bool foundNumber = false;

         do
         {
            int randomInt = new Random().Next(0, queueLength);
            if (!intListMixed.Contains(randomInt))
            {
               intListMixed.Add(randomInt);
               foundNumber = true;
            }
         } while (!foundNumber);
      }

      foreach (int randomInt in intListMixed)
      {
         playlistVideosMixed.Add(playlistVideos[randomInt]);
      }

      return playlistVideosMixed;
   }

   private static List<FullTrack> ShufflePlayListForSpotify(List<FullTrack> fullTracks)
   {
      List<FullTrack> fullTracksMixed = new();

      int queueLength = fullTracks.Count;
      List<int> intListMixed = new();

      for (int i = 0; i < queueLength; i++)
      {
         bool foundNumber = false;

         do
         {
            int randomInt = new Random().Next(0, queueLength);
            if (!intListMixed.Contains(randomInt))
            {
               intListMixed.Add(randomInt);
               foundNumber = true;
            }
         } while (!foundNumber);
      }

      foreach (int randomInt in intListMixed)
      {
         fullTracksMixed.Add(fullTracks[randomInt]);
      }

      return fullTracksMixed;
   }

   public static void SpotifyQueueAddSearchAsync(QueueTrack queueTrack)
   {
      Task.Run(async () =>
      {
         QueueTrack editQueueTrack = QueueTracks.Find(x => x.FullTrack == queueTrack.FullTrack);

         try
         {
            if (editQueueTrack != null)
            {
               editQueueTrack.YouTubeUri = SearchYoutubeFromSpotify(queueTrack.FullTrack);
               editQueueTrack.IsAdded = true;
               Console.WriteLine("added");
            }
            else
            {
               Console.WriteLine("editQueueTrack was null");
            }
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
         }

         return Task.CompletedTask;
      });
   }

   private static Uri SearchYoutubeFromSpotify(FullTrack fullTrack)
   {
      List<VideoResultFromYTSearch> results = new();
      YoutubeClient youtubeClient = new();

      string artists = fullTrack.Artists.Aggregate("", (current, artist) => current + " " + artist.Name);
      string trackName = fullTrack.Name;
      string externalIds = fullTrack.ExternalIds.Values.FirstOrDefault();
      int durationMs = fullTrack.DurationMs;
      List<SimpleArtist> artistsArray = fullTrack.Artists;

      try
      {
         results.Add(new VideoResultFromYTSearch(youtubeClient.Search.GetVideosAsync($"{artists} {trackName}").CollectAsync(1).Result[0], new TimeSpan(0), 0));
      }
      catch
      {
      }

      try
      {
         results.Add(new VideoResultFromYTSearch(youtubeClient.Search.GetVideosAsync($"{externalIds}").CollectAsync(1).Result[0], new TimeSpan(0), 0));
      }
      catch
      {
      }

      TimeSpan t1 = TimeSpan.FromMilliseconds(durationMs);
      foreach (VideoResultFromYTSearch result in results)
      {
         if (result.VideoSearchResult.Title.ToLower().Contains(trackName.ToLower()))
         {
            result.Hits++;
         }

         foreach (SimpleArtist artist in artistsArray)
         {
            if (result.VideoSearchResult.Author.ChannelTitle.ToLower().Contains(artist.Name.ToLower()))
            {
               result.Hits++;
            }

            if (result.VideoSearchResult.Title.ToLower().Contains(artist.Name.ToLower()))
            {
               result.Hits++;
            }
         }

         TimeSpan t2 = TimeSpan.FromMilliseconds(result.VideoSearchResult.Duration.Value.TotalMilliseconds);
         result.OffsetTimeSpan = t2 - t1;
      }

      results.Sort((ps1, ps2) => TimeSpan.Compare(ps1.OffsetTimeSpan, ps2.OffsetTimeSpan));

      results.FirstOrDefault().Hits++;

      results.OrderBy(search => search.Hits);

      DiscordEmbedBuilder discordEmbedBuilder = new()
      {
         Title = $"Spotify <{externalIds}>"
      };
      discordEmbedBuilder.AddField(new DiscordEmbedField($"{t1:mm\\:ss}   |   {artists}   -   {trackName}", externalIds));

      int i = 1;
      foreach (VideoResultFromYTSearch result in results)
      {
         discordEmbedBuilder.AddField(new DiscordEmbedField($"Result number {i}", $"{result.Hits} hints   |   {result.OffsetTimeSpan:mm\\:ss} TimeSpanOffset"));

         discordEmbedBuilder.AddField(new DiscordEmbedField("" + $"{TimeSpan.FromMilliseconds(result.VideoSearchResult.Duration.Value.TotalMilliseconds):mm\\:ss}   |   " + $"{result.VideoSearchResult.Author.ChannelTitle}   -   " + $"{result.VideoSearchResult.Title}", result.VideoSearchResult.Url));
         i++;
      }

      return new Uri(results.FirstOrDefault().VideoSearchResult.Url);
   }

   private static Task PlayFromQueueTask(GCM gCM, QueueTrack queueTrack)
   {
      CancellationTokenSource tokenSource = new();
      CancellationToken cancellationToken = tokenSource.Token;
      DcCancellationTokenItem dcCancellationTokenKeyPair = new(gCM.DiscordGuild, tokenSource);
      CancellationTokenItemList.Add(dcCancellationTokenKeyPair);

      try
      {
         Task.Run(() => PlayFromQueueAsyncTask(gCM, queueTrack, cancellationToken), cancellationToken);
      }
      catch
      {
         CancellationTokenItemList.Remove(dcCancellationTokenKeyPair);
      }

      return Task.CompletedTask;
   }

   private static async Task PlayFromQueueAsyncTask(GCM gCM, QueueTrack queueTrack, CancellationToken cancellationToken)
   {
      VoiceNextExtension voiceNext = Bot.DiscordClient.GetVoiceNext();
      if (voiceNext == null)
      {
         return;
      }

      VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(gCM.DiscordGuild);
      DiscordVoiceState voiceState = gCM.DiscordMember?.VoiceState;
      if (voiceState?.Channel == null)
      {
         return;
      }

      voiceNextConnection ??= await voiceNext.ConnectAsync(voiceState.Channel);

      VoiceTransmitSink voiceTransmitSink = default;

      try
      {
         QueueTracks.Find(x => x == queueTrack).HasBeenPlayed = true;

         Uri networkDriveUri = new(@"N:\");
         YoutubeDL youtubeDl = new()
         {
            YoutubeDLPath = "..\\..\\..\\Model\\Executables\\youtube-dl\\yt-dlp.exe",
            FFmpegPath = "..\\..\\..\\Model\\Executables\\ffmpeg\\ffmpeg.exe",
            OutputFolder = networkDriveUri.AbsolutePath,
            RestrictFilenames = true,
            OverwriteFiles = false,
            IgnoreDownloadErrors = false
         };

         OptionSet optionSet = new()
         {
            AddMetadata = true,
            AudioQuality = 0
         };

         optionSet.AddCustomOption("--output", networkDriveUri.AbsolutePath + "%(title)s-%(id)s-%(release_date)s.%(ext)s");
         RunResult<string> audioDownload = await youtubeDl.RunAudioDownload(queueTrack.YouTubeUri.AbsoluteUri, AudioConversionFormat.Mp3, new CancellationToken(), null, null, optionSet);
         VideoData audioDownloadMetaData = youtubeDl.RunVideoDataFetch(queueTrack.YouTubeUri.AbsoluteUri).Result.Data;
         TimeSpan audioDownloadTimeSpan = default;
         if (audioDownloadMetaData?.Duration != null)
         {
            audioDownloadTimeSpan = new TimeSpan(0, 0, 0, (int)audioDownloadMetaData.Duration.Value);
         }

         DiscordEmbedBuilder discordEmbedBuilder = new();

         if (queueTrack.SpotifyUri == null)
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField("YouTube", $"[[-🔗-]({queueTrack.YouTubeUri.AbsoluteUri})]", true));
         }
         else
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField("Spotify", $"[[-🔗-]({queueTrack.SpotifyUri.AbsoluteUri})]", true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("YouTube", $"[[-🔗-]({queueTrack.YouTubeUri.AbsoluteUri})]", true));
         }

         DiscordComponentEmoji discordComponentEmojisPrevious = new("⏮️");
         DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
         DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
         DiscordComponentEmoji discordComponentEmojisShuffle = new("🔀");
         DiscordComponentEmoji discordComponentEmojisQueue = new("⏬");
         DiscordComponent[] discordComponent = new DiscordComponent[5];
         discordComponent[0] = new DiscordButtonComponent(ButtonStyle.Primary, "PreviousTrackStream", "Back!", false, discordComponentEmojisPrevious);
         discordComponent[1] = new DiscordButtonComponent(ButtonStyle.Primary, "NextTrackStream", "Next!", false, discordComponentEmojisNext);
         discordComponent[2] = new DiscordButtonComponent(ButtonStyle.Danger, "StopTrackStream", "Stop!", false, discordComponentEmojisStop);
         discordComponent[3] = new DiscordButtonComponent(ButtonStyle.Success, "ShuffleStream", "Shuffle!", false, discordComponentEmojisShuffle);
         discordComponent[4] = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowQueueStream", "Show queue!", false, discordComponentEmojisQueue);

         if (audioDownload.ErrorOutput.Length > 1)
         {
            await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().WithContent($"{audioDownload.ErrorOutput[1]} `{queueTrack.YouTubeUri.AbsoluteUri}`"));
         }
         else
         {
            discordEmbedBuilder = CustomDiscordEmbedBuilder(discordEmbedBuilder, queueTrack, new Uri(audioDownload.Data), audioDownloadMetaData, null);
            DiscordMessage discordMessage = await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponent).AddEmbed(discordEmbedBuilder.Build()));

            ProcessStartInfo ffmpegProcessStartInfo = new()
            {
               FileName = "..\\..\\..\\Model\\Executables\\ffmpeg\\ffmpeg.exe",
               Arguments = $@"-i ""{audioDownload.Data}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
               RedirectStandardOutput = true,
               UseShellExecute = false
            };
            Process ffmpegProcess = Process.Start(ffmpegProcessStartInfo);
            if (ffmpegProcess != null)
            {
               voiceTransmitSink = voiceNextConnection.GetTransmitSink();
               voiceTransmitSink.VolumeModifier = 0.2;
               Stream ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;
               Task ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink);

               int timeSpanAdvanceInt = 0;
               while (!ffmpegCopyTask.IsCompleted)
               {
                  await Task.Delay(1000);

                  try
                  {
                     if (timeSpanAdvanceInt % 10 == 0)
                     {
                        discordEmbedBuilder.Description = TimeLineStringBuilderWhilePlaying(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);
                        await discordMessage.ModifyAsync(x => x.AddComponents(discordMessage.Components).AddEmbed(discordEmbedBuilder.Build()));
                     }
                  }
                  catch (Exception ex)
                  {
                     CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__12", "").Replace("<", ""), ConsoleColor.Red);
                  }

                  if (cancellationToken.IsCancellationRequested)
                  {
                     ffmpegStream.Close();
                     break;
                  }

                  timeSpanAdvanceInt++;
               }

               discordComponent[0] = new DiscordButtonComponent(ButtonStyle.Primary, "PreviousTrackStream", "Back!", true, discordComponentEmojisPrevious);
               discordComponent[1] = new DiscordButtonComponent(ButtonStyle.Primary, "NextTrackStream", "Next!", true, discordComponentEmojisNext);
               discordComponent[2] = new DiscordButtonComponent(ButtonStyle.Danger, "StopTrackStream", "Stop!", true, discordComponentEmojisStop);
               discordComponent[3] = new DiscordButtonComponent(ButtonStyle.Success, "ShuffleStream", "Shuffle!", true, discordComponentEmojisShuffle);
               discordComponent[4] = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowQueueStream", "Show queue!", true, discordComponentEmojisQueue);

               discordEmbedBuilder.Description = TimeLineStringBuilderAfterTrack(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);
               await discordMessage.ModifyAsync(x => x.AddComponents(discordComponent).AddEmbed(discordEmbedBuilder.Build()));
            }

            if (!cancellationToken.IsCancellationRequested)
            {
               CancellationTokenItemList.RemoveAll(x => x.CancellationTokenSource.Token == cancellationToken && x.DiscordGuild == gCM.DiscordGuild);
            }
         }
      }
      catch (Exception ex)
      {
         await Bot.DebugDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(ex.ToString())));
         await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Something went wrong!\n")));
      }
      finally
      {
         await voiceTransmitSink.FlushAsync();

         if (!cancellationToken.IsCancellationRequested)
         {
            if (QueueTracks.All(x => x.GCM.DiscordGuild == gCM.DiscordGuild && x.HasBeenPlayed))
            {
               await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Queue is empty!")));

               List<CancellationTokenSource> cancellationTokenSourceList = new();

               foreach (DcCancellationTokenItem item in CancellationTokenItemList.Where(x => x.DiscordGuild == gCM.DiscordGuild))
               {
                  cancellationTokenSourceList.Add(item.CancellationTokenSource);
               }

               CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == gCM.DiscordGuild);

               foreach (CancellationTokenSource item in cancellationTokenSourceList)
               {
                  item.Cancel();
                  item.Dispose();
               }

               voiceNextConnection.Disconnect();
            }

            foreach (QueueTrack queueTrackItem in QueueTracks)
            {
               if (queueTrackItem.GCM.DiscordGuild == gCM.DiscordGuild && queueTrackItem.HasBeenPlayed == false)
               {
                  CancellationTokenSource cancellationTokenSource = new();
                  CancellationToken token = cancellationTokenSource.Token;
                  CancellationTokenItemList.Add(new DcCancellationTokenItem(gCM.DiscordGuild, cancellationTokenSource));

                  Task.Run(() => PlayFromQueueAsyncTask(gCM, queueTrackItem, token));
                  break;
               }
            }
         }
      }
   }

   public static string TimeLineStringBuilderWhilePlaying(int timeSpanAdvanceInt, TimeSpan totalTimeSpan, CancellationToken cancellationToken)
   {
      TimeSpan playerAdvanceTimeSpan = TimeSpan.FromSeconds(timeSpanAdvanceInt);

      string playerAdvanceString = PlayerAdvance(timeSpanAdvanceInt, totalTimeSpan);

      string descriptionString = "⏹️";
      if (cancellationToken.IsCancellationRequested)
      {
         descriptionString = "▶️";
      }

      if (playerAdvanceTimeSpan.Hours != 0)
      {
         descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Hours:#00}:{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
      }
      else
      {
         descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
      }

      return descriptionString;
   }

   public static string TimeLineStringBuilderAfterTrack(int timeSpanAdvanceInt, TimeSpan totalTimeSpan, CancellationToken cancellationToken)
   {
      TimeSpan playerAdvanceTimeSpan = TimeSpan.FromSeconds(timeSpanAdvanceInt);

      string durationString = playerAdvanceTimeSpan.Hours != 0 ? $"{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}" : $"{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}";

      if (!cancellationToken.IsCancellationRequested)
      {
         return $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
      }

      string descriptionString = "⏹️";
      if (cancellationToken.IsCancellationRequested)
      {
         descriptionString = "▶️";
      }

      string playerAdvanceString = PlayerAdvance(timeSpanAdvanceInt, totalTimeSpan);

      if (playerAdvanceTimeSpan.Hours != 0)
      {
         descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Hours:#00}:{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
      }
      else
      {
         descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
      }

      return descriptionString;
   }

   private static string PlayerAdvance(int timeSpanAdvanceInt, TimeSpan totalTimeSpan)
   {
      string[] strings = new string[15];
      string playerAdvanceString = "";

      double thisIsOneHundredPercent = totalTimeSpan.TotalSeconds;
      double dotPositionInPercent = 100.0 / thisIsOneHundredPercent * timeSpanAdvanceInt;
      double dotPositionInInt = 15.0 / 100.0 * dotPositionInPercent;

      for (int i = 0; i < strings.Length; i++)
      {
         if (Convert.ToInt32(dotPositionInInt) == i)
         {
            strings[i] = "🔘";
         }
         else
         {
            strings[i] = "▬";
         }
      }

      foreach (string item in strings)
      {
         playerAdvanceString += item;
      }

      return playerAdvanceString;
   }

   public static AcoustId.Root AcoustIdFromFingerPrint(Uri filePathUri)
   {
      string[] fingerPrintDuration = default;
      string[] fingerPrintFingerprint = default;
      ProcessStartInfo fingerPrintCalculationProcessStartInfo = new()
      {
         FileName = "..\\..\\..\\Model\\Executables\\fpcalc\\fpcalc.exe",
         Arguments = filePathUri.AbsolutePath,
         RedirectStandardOutput = true,
         UseShellExecute = false
      };
      Process fingerPrintCalculationProcess = Process.Start(fingerPrintCalculationProcessStartInfo);
      if (fingerPrintCalculationProcess != null)
      {
         string fingerPrintCalculationOutput = fingerPrintCalculationProcess.StandardOutput.ReadToEndAsync().Result;
         string[] fingerPrintArgs = fingerPrintCalculationOutput.Split("\r\n");
         if (fingerPrintArgs.Length == 3)
         {
            fingerPrintDuration = fingerPrintArgs[0].Split('=');
            fingerPrintFingerprint = fingerPrintArgs[1].Split('=');
         }
      }

      AcoustId.Root acoustId = new();
      if (fingerPrintDuration != null)
      {
         string url = "http://aPI.acoustid.org/v2/lookup?client=" + Bot.Connections.AcoustIdApiKey + "&duration=" + fingerPrintDuration[1] + "&fingerprint=" + fingerPrintFingerprint[1] + "&meta=recordings+recordingIds+releases+releaseIds+ReleaseGroups+releaseGroupIds+tracks+compress+userMeta+sources";

         string httpClientContent = new HttpClient().GetStringAsync(url).Result;
         acoustId = AcoustId.CreateObj(httpClientContent);
      }

      return acoustId;
   }

   public static DiscordEmbedBuilder CustomDiscordEmbedBuilder(DiscordEmbedBuilder discordEmbedBuilder, QueueTrack queueTrack, Uri filePathUri, VideoData audioDownloadMetaData, File metaTagFileToPlay)
   {
      if (metaTagFileToPlay == null)
      {
         bool needThumbnail = true;
         bool needAlbum = true;
         string albumTitle = "";
         string recordingMbId = "";
         discordEmbedBuilder.Title = audioDownloadMetaData.Title;
         discordEmbedBuilder.WithAuthor(audioDownloadMetaData.Creator);
         discordEmbedBuilder.WithUrl(queueTrack.YouTubeUri.AbsoluteUri);

         if (queueTrack.SpotifyUri != null)
         {
            SpotifyClient spotifyClient = GetSpotifyClientConfig();
            string trackId = StringCutter.RmAfter(StringCutter.RmUntil(queueTrack.SpotifyUri.AbsoluteUri, "/track/", "/track/".Length), "?si", 0);
            FullTrack fullTrack = spotifyClient.Tracks.Get(trackId).Result;
            if (fullTrack.Album.Images.Count > 0)
            {
               discordEmbedBuilder.WithThumbnail(fullTrack.Album.Images[0].Url);

               Bitmap bitmapAlbumCover = new(new HttpClient().GetStreamAsync(fullTrack.Album.Images[0].Url).Result);
               Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
               discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
               needThumbnail = false;
            }

            if (fullTrack.Album.Name != "")
            {
               albumTitle = fullTrack.Album.Name;
               needAlbum = false;
            }
         }

         AcoustId.Root acoustIdRoot = AcoustIdFromFingerPrint(filePathUri);
         if (acoustIdRoot.Results?.Count > 0 && acoustIdRoot.Results[0].Recordings?[0].Releases != null)
         {
            DateTime rightAlbumDateTime = new();
            AcoustId.Release rightAlbum = new();

            if (needAlbum)
            {
               foreach (AcoustId.Release albumItem in acoustIdRoot.Results[0].Recordings[0].Releases)
               {
                  if (acoustIdRoot.Results[0].Recordings[0].Releases.Count == 1)
                  {
                     rightAlbum = albumItem;
                     break;
                  }

                  if (albumItem.Date == null || albumItem.Date.Year == 0)
                  {
                     continue;
                  }

                  if (albumItem.Date.Month == 0)
                  {
                     albumItem.Date.Month = 1;
                  }

                  if (albumItem.Date.Day == 0)
                  {
                     albumItem.Date.Day = 1;
                  }

                  if (rightAlbumDateTime.Equals(new DateTime()))
                  {
                     rightAlbumDateTime = new DateTime(albumItem.Date.Year, albumItem.Date.Month, albumItem.Date.Day);
                  }

                  DateTime albumItemDateTime = new(albumItem.Date.Year, albumItem.Date.Month, albumItem.Date.Day);
                  if (rightAlbumDateTime >= albumItemDateTime)
                  {
                     rightAlbum = albumItem;
                     rightAlbumDateTime = albumItemDateTime;
                  }
               }

               if (rightAlbum.Title == "")
               {
                  albumTitle = rightAlbum.Title;
               }
            }

            recordingMbId = acoustIdRoot.Results[0].Recordings[0].Id;
            IRecording iRecording = new Query().LookupRecordingAsync(new Guid(recordingMbId)).Result;

            string genres = "";
            if (iRecording.Genres != null)
            {
               foreach (char genre in genres)
               {
                  genres += genre;

                  if (genres.Last() != genre)
                  {
                     genres += ", ";
                  }
               }
            }

            if (genres != "")
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));
            }

            if (rightAlbum.Id != null && needThumbnail)
            {
               try
               {
                  discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release/{rightAlbum.Id}/front");

                  Bitmap bitmapAlbumCover0 = new(new HttpClient().GetStreamAsync($"https://coverartarchive.org/release/{rightAlbum.Id}/front").Result);
                  Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover0);
                  discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
                  needThumbnail = false;
               }
               catch
               {
                  //invalid url
               }
            }
         }

         if (needThumbnail)
         {
            discordEmbedBuilder.WithThumbnail(audioDownloadMetaData.Thumbnails[18].Url);

            Bitmap bitmapAlbumCover = new(new HttpClient().GetStreamAsync(audioDownloadMetaData.Thumbnails[18].Url).Result);
            Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
            discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
         }

         if (recordingMbId != "")
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainz", $"[[-🔗-](https://musicbrainz.org/recording/{recordingMbId})]", true));
         }

         if (albumTitle != "")
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField("Album", albumTitle, true));
         }

         discordEmbedBuilder.AddField(new DiscordEmbedField("Uploader", audioDownloadMetaData.Uploader, true));
      }
      else
      {
         discordEmbedBuilder.Title = metaTagFileToPlay.Tag.Title;
         discordEmbedBuilder.WithAuthor(metaTagFileToPlay.Tag.JoinedPerformers);
         if (metaTagFileToPlay.Tag.Album != null)
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField("Album", metaTagFileToPlay.Tag.Album, true));
         }

         if (metaTagFileToPlay.Tag.JoinedGenres != null)
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", metaTagFileToPlay.Tag.JoinedGenres, true));
         }

         if (metaTagFileToPlay.Tag.MusicBrainzReleaseId != null)
         {
            Stream bitmapStream = null;
            try
            {
               string httpClientContent = new HttpClient().GetStringAsync($"https://coverartarchive.org/release/{metaTagFileToPlay.Tag.MusicBrainzReleaseId}").Result;
               MusicBrainz.Root musicBrainzObj = MusicBrainz.CreateObj(httpClientContent);

               bitmapStream = new HttpClient().GetStreamAsync(musicBrainzObj.Images.FirstOrDefault()?.ImageString).Result;
               discordEmbedBuilder.WithThumbnail(musicBrainzObj.Images.FirstOrDefault()?.ImageString);
               discordEmbedBuilder.WithUrl(musicBrainzObj.Release);
            }
            catch
            {
               if (metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId != null)
               {
                  try
                  {
                     bitmapStream = new HttpClient().GetStreamAsync($"https://coverartarchive.org/release-group/{metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId}/front").Result;
                     discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release-group/{metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId}/front");
                  }
                  catch
                  {
                     //ignore
                  }
               }
            }
            finally
            {
               if (bitmapStream != null)
               {
                  Bitmap albumCoverBitmap = new(bitmapStream);
                  Color dominantColor = ColorMath.GetDominantColor(albumCoverBitmap);
                  discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
               }
            }
         }

         if (metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId == null && metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId == null)
         {
            if (metaTagFileToPlay.Tag.MusicBrainzArtistId != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzArtistId", metaTagFileToPlay.Tag.MusicBrainzArtistId));
            }

            if (metaTagFileToPlay.Tag.MusicBrainzDiscId != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzDiscId", metaTagFileToPlay.Tag.MusicBrainzDiscId));
            }

            if (metaTagFileToPlay.Tag.MusicBrainzReleaseArtistId != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseArtistId", metaTagFileToPlay.Tag.MusicBrainzReleaseArtistId));
            }

            if (metaTagFileToPlay.Tag.MusicBrainzReleaseCountry != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseCountry", metaTagFileToPlay.Tag.MusicBrainzReleaseCountry));
            }

            if (metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseGroupId", metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId));
            }

            if (metaTagFileToPlay.Tag.MusicBrainzReleaseId != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseId", metaTagFileToPlay.Tag.MusicBrainzReleaseId));
            }

            if (metaTagFileToPlay.Tag.MusicBrainzReleaseStatus != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseStatus", metaTagFileToPlay.Tag.MusicBrainzReleaseStatus));
            }

            if (metaTagFileToPlay.Tag.MusicBrainzReleaseType != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseType", metaTagFileToPlay.Tag.MusicBrainzReleaseType));
            }

            if (metaTagFileToPlay.Tag.MusicBrainzTrackId != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzTrackId", metaTagFileToPlay.Tag.MusicBrainzTrackId));
            }

            if (metaTagFileToPlay.Tag.MusicIpId != null)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicIpId", metaTagFileToPlay.Tag.MusicIpId));
            }
         }
      }

      return discordEmbedBuilder;
   }

   [SlashCommand("Stop" + Bot.isDevBot, "Stop the music!")]
   private async Task StopCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

      if (interactionContext.Member.VoiceState == null)
      {
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be connected!"));
         return;
      }

      await StopMusicTask(new GCM(interactionContext.Guild, interactionContext.Channel, interactionContext.Member), true);
   }

   [SlashCommand("Shuffle" + Bot.isDevBot, "Randomize the queue!")]
   private async Task Shuffle(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
      await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shuffle requested!"));

      ShuffleQueueTracksAsyncTask(new GCM(interactionContext.Guild, interactionContext.Channel, interactionContext.Member));
   }

   public static async Task API_Shuffle(API aPI)
   {
      GCM gCM = new();
      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
      foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPI.RequestDiscordUserId))
      {
         gCM.DiscordGuild = guildItem;
         gCM.DiscordMember = memberItem;
         gCM.DiscordChannel = memberItem.VoiceState.Channel;
         break;
      }

      if (gCM.DiscordMember == null)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == aPI.RequestDiscordUserId))
         {
            await memberItem.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return;
         }

         return;
      }

      ShuffleQueueTracksAsyncTask(gCM);
   }

   public static async Task ShuffleQueueTracksAsyncTask(GCM gCM)
   {
      if (QueueTracks.Any(x => x.GCM.DiscordGuild == gCM.DiscordGuild && !x.IsAdded))
      {
         int queueItemsInt = QueueTracks.Count(x => x.GCM.DiscordGuild == gCM.DiscordGuild && x.IsAdded);
         int queueItemsNotAddedInt = QueueTracks.Count(x => x.GCM.DiscordGuild == gCM.DiscordGuild && !x.IsAdded);
         await gCM.DiscordMember.VoiceState.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Queue is being created! " + $"{queueItemsInt}/" + $"{queueItemsNotAddedInt} Please wait!")));
      }
      else
      {
         List<QueueTrack> queueTracksMixed = new(ShuffleQueueTracks(QueueTracks.FindAll(x => x.GCM.DiscordGuild == gCM.DiscordGuild && !x.HasBeenPlayed)));

         QueueTracks.RemoveAll(x => x.GCM.DiscordGuild == gCM.DiscordGuild && !x.HasBeenPlayed);

         foreach (QueueTrack queueTrack in queueTracksMixed)
         {
            QueueTracks.Add(queueTrack);
         }

         if (queueTracksMixed.Count == 0)
         {
            await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Its late i have to leave!")));
         }
         else
         {
            await gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription("Queue has been altered!")));
         }
      }
   }

   public static List<QueueTrack> ShuffleQueueTracks(List<QueueTrack> queueTrack)
   {
      List<QueueTrack> queueTrackMixed = queueTrack;

      int queueLength = queueTrack.Count;
      List<int> intListMixed = new();

      for (int i = 0; i < queueLength; i++)
      {
         bool foundNumber = false;

         do
         {
            int randomInt = new Random().Next(0, queueLength);
            if (!intListMixed.Contains(randomInt))
            {
               intListMixed.Add(randomInt);
               foundNumber = true;
            }
         } while (!foundNumber);
      }

      foreach (int randomInt in intListMixed)
      {
         queueTrackMixed.Add(queueTrack[randomInt]);
      }

      return queueTrackMixed;
   }

   internal static Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
   {
      eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

      if (QueueTracks.Any(x => x.GCM.DiscordGuild == eventArgs.Guild))
      {
         DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
         GCM gCM = new(eventArgs.Guild, eventArgs.Channel, discordMember);

         if (gCM.DiscordMember.VoiceState == null)
         {
            eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return Task.CompletedTask;
         }

         switch (eventArgs.Id)
         {
            case "PreviousTrackStream":
            {
               PlayPreviousTrackFromQueue(gCM);
               break;
            }
            case "NextTrackStream":
            {
               PlayNextTrackFromQueue(gCM);
               break;
            }
            case "StopTrackStream":
            {
               StopMusicTask(new GCM(eventArgs.Guild, eventArgs.Channel, discordMember), false);
               break;
            }
            case "ShuffleStream":
            {
               gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Shuffle requested!")));
               ShuffleQueueTracksAsyncTask(gCM);
               break;
            }
            case "ShowQueueStream":
            {
               DiscordMessage discordMessage = eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Loading!"))).Result;

               if (QueueTracks.All(x => x.GCM.DiscordGuild != gCM.DiscordGuild && x.HasBeenPlayed))
               {
                  discordMessage.ModifyAsync("Queue is empty!");
               }
               else
               {
                  string descriptionString = "";
                  DiscordEmbedBuilder discordEmbedBuilder = new();

                  List<QueueTrack> queueTracks = QueueTracks.FindAll(x => x.GCM.DiscordChannel == gCM.DiscordChannel && !x.HasBeenPlayed);

                  for (int i = 0; i < 15; i++)
                  {
                     if (queueTracks.Count == i)
                     {
                        break;
                     }

                     if (queueTracks[i].FullTrack != null)
                     {
                        descriptionString += "[🔗[YouTube]" + $"({queueTracks[i].YouTubeUri.AbsoluteUri})] " + "[🔗[Spotify]" + $"({queueTracks[i].SpotifyUri.AbsoluteUri})]  " + queueTracks[i].Title + " - " + queueTracks[i].Artist + "\n";
                     }
                     else
                     {
                        descriptionString += "[🔗[YouTube]" + $"({queueTracks[i].YouTubeUri.AbsoluteUri})] " + queueTracks[i].Title + " - " + queueTracks[i].Artist + "\n";
                     }
                  }

                  discordEmbedBuilder.Title = $"{queueTracks.Count} Track/s in queue!";
                  discordEmbedBuilder.WithDescription(descriptionString);
                  discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder));
               }

               break;
            }
         }
      }

      return Task.CompletedTask;
   }

   private static void PlayPreviousTrackFromQueue(GCM gCM)
   {
      /*if (QueueTracks.Any(x => x.GCM.DiscordGuild == gCM.DiscordGuild && !x.HasBeenPlayed))
      {
         gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Wrong universe!")));
         return;
      }*/

      QueueTrack item = QueueTracks.First(x => x.GCM.DiscordGuild == gCM.DiscordGuild && !x.HasBeenPlayed);

      int indexOfPlaying = QueueTracks.FindIndex(x => x.GCM.DiscordGuild == item.GCM.DiscordGuild && ((x.SpotifyUri == item.SpotifyUri && x.SpotifyUri != null && item.SpotifyUri != null) || (x.YouTubeUri == item.YouTubeUri && x.YouTubeUri != null && item.YouTubeUri != null))) - 1;
      QueueTrack playingQueueTrack = QueueTracks[indexOfPlaying];
      playingQueueTrack.HasBeenPlayed = false;

      int indexOfLast = QueueTracks.FindIndex(x => x.GCM.DiscordGuild == item.GCM.DiscordGuild && ((x.SpotifyUri == item.SpotifyUri && x.SpotifyUri != null && item.SpotifyUri != null) || (x.YouTubeUri == item.YouTubeUri && x.YouTubeUri != null && item.YouTubeUri != null))) - 2;
      QueueTrack lastQueueTrack = QueueTracks[indexOfLast];
      lastQueueTrack.HasBeenPlayed = false;

      List<CancellationTokenSource> cancellationTokenSourceList = new();
      foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == gCM.DiscordGuild))
      {
         cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
      }

      CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == gCM.DiscordGuild);

      foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
      {
         cancellationToken.Cancel();
         cancellationToken.Dispose();
      }

      CancellationTokenSource newCancellationTokenSource = new();
      CancellationToken newCancellationToken = newCancellationTokenSource.Token;

      foreach (QueueTrack queueTrack in QueueTracks.Where(x => x.GCM.DiscordGuild == gCM.DiscordGuild && !x.HasBeenPlayed))
      {
         DcCancellationTokenItem newDcCancellationTokenItem = new(gCM.DiscordGuild, newCancellationTokenSource);
         CancellationTokenItemList.Add(newDcCancellationTokenItem);

         Task.Run(() => PlayFromQueueAsyncTask(gCM, queueTrack, newCancellationToken), newCancellationToken);
         break;
      }
   }

   private static void PlayNextTrackFromQueue(GCM gCM)
   {
      if (QueueTracks.All(x => x.GCM.DiscordGuild != gCM.DiscordGuild && x.HasBeenPlayed))
      {
         gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Nothing to skip!")));
         return;
      }

      List<CancellationTokenSource> cancellationTokenSourceList = new();
      foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == gCM.DiscordGuild))
      {
         cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
      }

      CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == gCM.DiscordGuild);

      foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
      {
         cancellationToken.Cancel();
         cancellationToken.Dispose();
      }

      CancellationTokenSource newCancellationTokenSource = new();
      CancellationToken newCancellationToken = newCancellationTokenSource.Token;

      foreach (QueueTrack queueTrack in QueueTracks.Where(x => x.GCM.DiscordGuild == gCM.DiscordGuild && !x.HasBeenPlayed))
      {
         DcCancellationTokenItem newDcCancellationTokenItem = new(gCM.DiscordGuild, newCancellationTokenSource);
         CancellationTokenItemList.Add(newDcCancellationTokenItem);

         Task.Run(() => PlayFromQueueAsyncTask(gCM, queueTrack, newCancellationToken), newCancellationToken);
         break;
      }
   }

   private static Task StopMusicTask(GCM gCM, bool sendStopped)
   {
      bool nothingToStop = true;
      List<CancellationTokenSource> cancellationTokenSourceList = new();
      foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == gCM.DiscordGuild))
      {
         nothingToStop = false;
         cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
      }

      CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == gCM.DiscordGuild);

      foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
      {
         cancellationToken.Cancel();
         cancellationToken.Dispose();
      }

      if (sendStopped)
      {
         gCM.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(nothingToStop ? "Nothing to stop!" : "Stopped the music!")));
      }

      QueueTracks.RemoveAll(x => x.GCM.DiscordGuild == gCM.DiscordGuild);

      try
      {
         VoiceNextExtension voiceNext = Bot.DiscordClient.GetVoiceNext();
         VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(gCM.DiscordGuild);
         if (voiceNextConnection != null)
         {
            voiceNextConnection.Disconnect();
         }
      }
      catch
      {
         //ignore
      }

      return Task.CompletedTask;
   }

   internal static async Task PanicLeaveEvent(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
   {
      try
      {
         DiscordMember discordMember = await client.CurrentUser.ConvertToMember(eventArgs.Guild);
         if (eventArgs.Before != null && eventArgs.After != null && discordMember.VoiceState != null)
         {
            if (eventArgs.User == client.CurrentUser && eventArgs.After != null && eventArgs.Before.Channel != eventArgs.After.Channel)
            {
               bool nothingToStop = true;
               List<CancellationTokenSource> cancellationTokenSourceList = new();
               foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
               {
                  nothingToStop = false;
                  {
                     cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
                  }
               }

               CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

               foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
               {
                  cancellationToken.Cancel();
                  cancellationToken.Dispose();
               }

               QueueTracks.RemoveAll(x => x.GCM.DiscordGuild == eventArgs.Guild);

               eventArgs.Channel.SendMessageAsync(nothingToStop ? "Queue void and Left!" : "Stopped the music!");
               VoiceNextExtension voiceNext = client.GetVoiceNext();
               VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(eventArgs.Guild);
               if (voiceNextConnection != null)
               {
                  voiceNextConnection.Disconnect();
               }
            }
         }
      }
      catch
      {
         // ignored
      }
   }

   internal static async Task GotKickedEvent(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
   {
      try
      {
         DiscordMember discordMember = await client.CurrentUser.ConvertToMember(eventArgs.Guild);
         if (discordMember.VoiceState == null)
         {
            if (eventArgs.User == client.CurrentUser)
            {
               await StopMusicTask(new GCM(eventArgs.Guild, eventArgs.Channel, eventArgs.User.ConvertToMember(eventArgs.Guild).Result), false);
            }
         }
      }
      catch
      {
         // ignored
      }
   }
}