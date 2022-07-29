using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using RuntimeInformation = System.Runtime.InteropServices.RuntimeInformation;

// ReSharper disable UnusedMember.Local
#pragma warning disable CS4014

// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands
{
   internal class QueueItem
   {
      public DiscordGuild DiscordGuild { get; set; }
      public Uri YouTubeUri { get; set; }
      public Uri SpotifyUri { get; set; }
      public bool IsYouTube { get; set; }
      public bool IsSpotify { get; set; }

      internal QueueItem(DiscordGuild discordGuild, Uri youTubeUri, Uri spotifyUri)
      {
         DiscordGuild = discordGuild;
         YouTubeUri = youTubeUri;
         SpotifyUri = spotifyUri;
         IsYouTube = YouTubeUri != null;
         IsSpotify = SpotifyUri != null;
      }

      public QueueItem()
      {

      }
   }

   internal class CancellationTokenItem
   {
      internal DiscordGuild DiscordGuild { get; set; }
      internal CancellationTokenSource CancellationTokenSource { get; set; }

      internal CancellationTokenItem(DiscordGuild discordGuild, CancellationTokenSource cancellationTokenSource)
      {
         DiscordGuild = discordGuild;
         CancellationTokenSource = cancellationTokenSource;
      }

      internal CancellationTokenItem()
      {

      }
   }

   internal class PlayMusic : ApplicationCommandsModule
   {
      private static readonly List<CancellationTokenItem> CancellationTokenItemList = new();
      private static List<QueueItem> _queueItemList = new();
      private static bool _queueCreating = false;

      private static bool MusicAlreadyPlaying(DiscordGuild discordGuild)
      {
         return CancellationTokenItemList.Any(cancellationTokenItem => cancellationTokenItem.DiscordGuild == discordGuild);
      }

      public static SpotifyClient GetSpotifyClientConfig()
      {
         SpotifyClientConfig spotifyClientConfig = SpotifyClientConfig.CreateDefault();
         ClientCredentialsRequest clientCredentialsRequest = new(Bot.Connections.Token.ClientId, Bot.Connections.Token.ClientSecret);
         ClientCredentialsTokenResponse clientCredentialsTokenResponse = new OAuthClient(spotifyClientConfig).RequestToken(clientCredentialsRequest).Result;
         SpotifyClient spotifyClient = new(clientCredentialsTokenResponse.AccessToken);
         return spotifyClient;
      }

      [SlashCommand("DrivePlay" + Bot.isDevBot, "Just plays some random music!")]
      private async Task DrivePlayCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

         if (interactionContext.Member.VoiceState == null)
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
            return;
         }

         if (_queueItemList.Any(x => x.DiscordGuild == interactionContext.Guild))
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing! This interaction is locked!"));
            return;
         }

         if (!MusicAlreadyPlaying(interactionContext.Guild))
         {
            CancellationTokenSource tokenSource = new();
            CancellationToken cancellationToken = tokenSource.Token;
            CancellationTokenItem cancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
            CancellationTokenItemList.Add(cancellationTokenKeyPair);

            try
            {
               Task.Run(() => DrivePlayTask(interactionContext, null, null, null, null, cancellationToken, true), cancellationToken);
            }
            catch
            {
               CancellationTokenItemList.Remove(cancellationTokenKeyPair);
            }
         }
         else
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing already!"));
      }

      private static async Task DrivePlayTask(InteractionContext interactionContext, DiscordClient discordClient, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionDiscordChannel, CancellationToken cancellationToken, bool isInitialMessage)
      {
         discordClient ??= interactionContext.Client;
         discordGuild ??= interactionContext.Guild;
         interactionDiscordChannel ??= interactionContext.Channel;

         try
         {
            VoiceNextExtension voiceNextExtension = discordClient.GetVoiceNext();

            if (voiceNextExtension == null)
               return;

            VoiceNextConnection voiceNextConnection = voiceNextExtension.GetConnection(discordGuild);
            DiscordVoiceState discordMemberVoiceState = interactionContext != null ? interactionContext.Member?.VoiceState : discordMember?.VoiceState;

            if (discordMemberVoiceState?.Channel == null)
               return;

            voiceNextConnection ??= await voiceNextExtension.ConnectAsync(discordMemberVoiceState.Channel);

            if (isInitialMessage)
            {
               if (interactionContext != null)
                  await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!"));
               else
                  await interactionDiscordChannel.SendMessageAsync($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
               Uri networkDriveUri = new(@"M:\");
               string[] allFiles = Directory.GetFiles(networkDriveUri.AbsolutePath);

               Random random = new();
               int randomInt = random.Next(0, allFiles.Length - 1);
               string selectedFileToPlay = allFiles[randomInt];

               TagLib.File metaTagFileToPlay = TagLib.File.Create(@$"{selectedFileToPlay}");
               DiscordEmbedBuilder discordEmbedBuilder = CustomDiscordEmbedBuilder(null, null, metaTagFileToPlay);

               try
               {
                  DiscordMessage discordMessage = await interactionDiscordChannel.SendMessageAsync(discordEmbedBuilder.Build());

                  DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
                  DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
                  DiscordComponent[] discordComponents = new DiscordComponent[2];
                  discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song", "Next!", false, discordComponentEmojisNext);
                  discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song", "Stop!", false, discordComponentEmojisStop);

                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents));

                  ProcessStartInfo ffmpegProcessStartInfo = new()
                  {
                     FileName = "..\\..\\..\\Model\\Executables\\ffmpeg\\ffmpeg.exe",
                     Arguments = $@"-i ""{selectedFileToPlay}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                     RedirectStandardOutput = true,
                     UseShellExecute = false
                  };
                  Process ffmpegProcess = Process.Start(ffmpegProcessStartInfo);
                  Stream ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;

                  VoiceTransmitSink voiceTransmitSink = voiceNextConnection.GetTransmitSink();
                  voiceTransmitSink.VolumeModifier = 0.2;

                  Task ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink);

                  int timeSpanAdvanceInt = 0;
                  while (!ffmpegCopyTask.IsCompleted)
                  {
                     if (timeSpanAdvanceInt % 10 == 0)
                     {
                        discordEmbedBuilder.Description = TimeLineStringBuilderWhilePlaying(timeSpanAdvanceInt, metaTagFileToPlay.Properties.Duration, cancellationToken);
                        await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithEmbed(discordEmbedBuilder.Build()));
                     }

                     if (cancellationToken.IsCancellationRequested)
                     {
                        ffmpegStream.Close();
                        break;
                     }

                     timeSpanAdvanceInt++;
                     await Task.Delay(1000);
                  }

                  discordEmbedBuilder.Description = TimeLineStringBuilderAfterSong(timeSpanAdvanceInt, metaTagFileToPlay.Properties.Duration, cancellationToken);
                  await discordMessage.ModifyAsync(x => x.WithEmbed(discordEmbedBuilder.Build()));

                  await voiceTransmitSink.FlushAsync();
               }
               catch
               {
                  // ignored
               }
            }
         }
         catch (Exception exc)
         {
            if (interactionContext != null)
               interactionContext.Client.Logger.LogError(exc.Message);
            else
               discordClient.Logger.LogError(exc.Message);
         }
      }

      [SlashCommand("Play" + Bot.isDevBot, "Play spotify or youtube link!")]
      private async Task PlayCommand(InteractionContext interactionContext, [Option("Link", "Link!")] string webLink)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
         _queueCreating = true;
         Uri webLinkUri = new(webLink);

         if (interactionContext.Member.VoiceState == null)
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
            return;
         }

         bool isYouTube = false;
         bool isYouTubePlaylist = false;
         bool isYouTubePlaylistWithIndex = false;
         bool isSpotify = false;
         bool isSpotifyPlaylist = false;
         bool isSpotifyAlbum = false;
         bool musicPlayingAlready = MusicAlreadyPlaying(interactionContext.Guild);

         if (webLink.Contains("watch?v=") || webLink.Contains("&list=") || webLink.Contains("playlist?list="))
         {

            isYouTube = true;

            if (webLink.Contains("&list=") || webLink.Contains("playlist?list="))
            {
               isYouTubePlaylist = true;

               if (webLink.Contains("&index="))
                  isYouTubePlaylistWithIndex = true;
            }
         }
         else if (webLink.Contains("/track/") || webLink.Contains("/playlist/") || webLink.Contains("/album/"))
         {
            //https://open.spotify.com/artist/1aS5tqEs9ci5P9KD9tZWa6/discography/all?pageUri=spotify:album:1L8yTtYjg4JhfN7Aa6bqmN
            isSpotify = true;

            if (webLink.Contains("/playlist/"))
               isSpotifyPlaylist = true;
            else if (webLink.Contains("/album/"))
               isSpotifyAlbum = true;
         }
         else
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Sag ICH!!!"));
            return;
         }

         if (isYouTube)
         {

            Uri playlistUri = webLinkUri;

            Uri networkDriveUri = new(@"N:\");
            YoutubeDL youtubeDl = new()
            {
               YoutubeDLPath = "..\\..\\..\\Model\\Executables\\youtube-dl\\yt-dlp.exe",
               FFmpegPath = "..\\..\\..\\Model\\Executables\\ffmpeg\\ffmpeg.exe",
               OutputFolder = networkDriveUri.AbsolutePath,
               RestrictFilenames = true,
               IgnoreDownloadErrors = false
            };

            try
            {
               if (isYouTubePlaylist)
               {
                  int playlistSelectedVideoIndex = 1;
                  string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "&list=", "&list=".Length), "&index=", 0);

                  if (isYouTubePlaylistWithIndex)
                  {
                     playlistSelectedVideoIndex = Convert.ToInt32(StringCutter.RemoveUntilWord(webLink, "&index=", "&index=".Length));
                     playlistUri = new Uri("https://www.youtube.com/playlist?list=" + playlistId);
                  }

                  OptionSet optionSet = new()
                  {
                     PlaylistStart = playlistSelectedVideoIndex
                  };
                  VideoData[] videoDataArray = youtubeDl.RunVideoDataFetch(playlistUri.AbsoluteUri, new CancellationToken(), true, optionSet).Result.Data.Entries;

                  int startIndex = 0;
                  if (!musicPlayingAlready)
                  {
                     await PlayQueueAsyncTask(interactionContext, new Uri(videoDataArray[startIndex].Url), null);
                     startIndex++;
                  }

                  while (startIndex < videoDataArray.Length)
                  {
                     _queueItemList.Add(new QueueItem(interactionContext.Guild, new Uri(videoDataArray[startIndex].Url), null));
                     startIndex++;
                  }
               }
               else
               {
                  string selectedVideoId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "watch?v=", "watch?v=".Length), "&list=", 0);
                  Uri selectedVideoUri = new("https://www.youtube.com/watch?v=" + selectedVideoId);

                  if (!musicPlayingAlready)
                     await PlayQueueAsyncTask(interactionContext, selectedVideoUri, null);
                  else
                     _queueItemList.Add(new QueueItem(interactionContext.Guild, selectedVideoUri, null));
               }
            }
            catch
            {
               _queueCreating = false;
               await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error!"));
            }
         }
         else
         {
            SpotifyClient spotifyClient = GetSpotifyClientConfig();

            if (isSpotifyPlaylist)
            {
               string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/playlist/", "/playlist/".Length), "?si", 0);
               List<PlaylistTrack<IPlayableItem>> playlistTrackList = spotifyClient.Playlists.GetItems(playlistId).Result.Items;

               if (playlistTrackList != null && playlistTrackList.Count != 0)
               {
                  int startIndex = 0;
                  if (!musicPlayingAlready)
                  {
                     FullTrack playlistTrack = playlistTrackList[startIndex].Track as FullTrack;
                     FullTrack fullTrack = spotifyClient.Tracks.Get(playlistTrack!.Id).Result;
                     Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);

                     await PlayQueueAsyncTask(interactionContext, youTubeUri, new Uri("https://open.spotify.com/track/" + playlistTrack!.Id));
                     startIndex++;
                  }

                  while (startIndex < playlistTrackList.Count)
                  {
                     FullTrack playlistTrack = playlistTrackList[startIndex].Track as FullTrack;
                     FullTrack fullTrack = spotifyClient.Tracks.Get(playlistTrack!.Id).Result;
                     Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);

                     _queueItemList.Add(new QueueItem(interactionContext.Guild, youTubeUri, new Uri("https://open.spotify.com/track/" + playlistTrack!.Id)));
                     startIndex++;
                  }
               }
            }
            else if (isSpotifyAlbum)
            {
               string albumId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/album/", "/album/".Length), "?si", 0);
               List<SimpleTrack> simpleTrackList = spotifyClient.Albums.GetTracks(albumId).Result.Items;

               if (simpleTrackList != null)
               {
                  int startIndex = 0;

                  if (!musicPlayingAlready)
                  {
                     SimpleTrack simpleTrack = simpleTrackList[startIndex];
                     FullTrack fullTrack = spotifyClient.Tracks.Get(simpleTrack!.Id).Result;
                     Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);

                     await PlayQueueAsyncTask(interactionContext, youTubeUri, webLinkUri);

                     startIndex++;
                  }

                  while (startIndex < simpleTrackList.Count)
                  {
                     SimpleTrack simpleTrack = simpleTrackList[startIndex];
                     FullTrack fullTrack = spotifyClient.Tracks.Get(simpleTrack!.Id).Result;
                     Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);

                     _queueItemList.Add(new QueueItem(interactionContext.Guild, youTubeUri, new Uri("https://open.spotify.com/track/" + simpleTrack!.Id)));

                     startIndex++;
                  }
               }
            }
            else
            {
               string trackId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/track/", "/track/".Length), "?si", 0);

               if (!musicPlayingAlready)
               {
                  FullTrack fullTrack = spotifyClient.Tracks.Get(trackId).Result;
                  Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);

                  await PlayQueueAsyncTask(interactionContext, youTubeUri, new Uri("https://open.spotify.com/track/" + trackId));
               }
               else
               {
                  FullTrack fullTrack = spotifyClient.Tracks.Get(trackId).Result;
                  Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);

                  QueueItem queueKeyPair = new(interactionContext.Guild, youTubeUri, new Uri("https://open.spotify.com/track/" + trackId));
                  _queueItemList.Add(queueKeyPair);
               }
            }
         }

         if (musicPlayingAlready)
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing already! Your songs are in the queue now!"));

         _queueCreating = false;
      }

      private async Task<Uri> SearchYoutubeFromSpotify(FullTrack fullTrack)
      {
         YoutubeClient youtubeClient = new();

         IReadOnlyList<YoutubeExplode.Search.VideoSearchResult> videoSearchResults = await youtubeClient.Search.GetVideosAsync($"{fullTrack.Artists[0].Name} - {fullTrack.Name} - {fullTrack.ExternalIds.Values.FirstOrDefault()}");

         if (videoSearchResults.Count == 0)
            videoSearchResults = await youtubeClient.Search.GetVideosAsync($"{fullTrack.Artists[0].Name} - {fullTrack.Name}"); //this needs a limit

         return new Uri(videoSearchResults[0].Url);
      }

      private static Task PlayQueueAsyncTask(InteractionContext interactionContext, Uri youtubeUri, Uri spotifyUri)
      {
         CancellationTokenSource tokenSource = new();
         CancellationToken cancellationToken = tokenSource.Token;
         CancellationTokenItem cancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
         CancellationTokenItemList.Add(cancellationTokenKeyPair);

         QueueItem queueItem = new(interactionContext.Guild, youtubeUri, spotifyUri);

         _queueItemList.Add(queueItem);

         try
         {
            Task.Run(() => PlayFromQueueAsyncTask(interactionContext, null, null, null, null, queueItem, cancellationToken, true), cancellationToken);
         }
         catch
         {
            CancellationTokenItemList.Remove(cancellationTokenKeyPair);
         }

         return Task.CompletedTask;
      }

      private static async Task PlayFromQueueAsyncTask(InteractionContext interactionContext, DiscordClient discordClient, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionChannel, QueueItem queueItem, CancellationToken cancellationToken, bool isInitialMessage)
      {
         discordGuild ??= interactionContext.Guild;
         discordClient ??= interactionContext.Client;
         interactionChannel ??= interactionContext.Channel;

         VoiceNextExtension voiceNext = discordClient.GetVoiceNext();
         if (voiceNext == null)
            return;

         VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(discordGuild);
         DiscordVoiceState voiceState = interactionContext != null ? interactionContext.Member?.VoiceState : discordMember?.VoiceState;
         if (voiceState?.Channel == null)
            return;

         voiceNextConnection ??= await voiceNext.ConnectAsync(voiceState.Channel);

         if (isInitialMessage)
         {
            if (interactionContext != null)
               await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!"));
         }

         VoiceTransmitSink voiceTransmitSink = voiceNextConnection.GetTransmitSink();
         voiceTransmitSink.VolumeModifier = 0.2;

         try
         {
            _queueItemList.Remove(queueItem);

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
            RunResult<string> audioDownload = await youtubeDl.RunAudioDownload(queueItem.YouTubeUri.AbsoluteUri, AudioConversionFormat.Mp3, new CancellationToken(), null, null, optionSet);
            VideoData audioDownloadMetaData = youtubeDl.RunVideoDataFetch(queueItem.YouTubeUri.AbsoluteUri).Result.Data;
            TimeSpan audioDownloadTimeSpan = default;
            if (audioDownloadMetaData?.Duration != null)
               audioDownloadTimeSpan = new TimeSpan(0, 0, 0, (int)audioDownloadMetaData.Duration.Value);

            DiscordEmbedBuilder discordEmbedBuilder = CustomDiscordEmbedBuilder(new Uri(audioDownload.Data), audioDownloadMetaData, null);
            string audioDownloadError = null;

            if (audioDownload.ErrorOutput.Length > 1)
            {
               audioDownloadError = $"{audioDownload.ErrorOutput[1]} `{queueItem.YouTubeUri.AbsoluteUri}`";
            }

            DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
            DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
            DiscordComponentEmoji discordComponentEmojisShuffle = new("🔀");
            DiscordComponentEmoji discordComponentEmojisQueue = new("⏬");
            DiscordComponent[] discordComponents = new DiscordComponent[4];
            discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_stream", "Next!", false, discordComponentEmojisNext);
            discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_stream", "Stop!", false, discordComponentEmojisStop);
            discordComponents[2] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Success, "shuffle_stream", "Shuffle!", false, discordComponentEmojisShuffle);
            discordComponents[3] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Secondary, "queue_stream", "Show queue!", false, discordComponentEmojisQueue);

            DiscordMessage discordMessage;
            if (queueItem.IsYouTube && !queueItem.IsSpotify)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("YouTube", $"[-🔗-]({queueItem.YouTubeUri.AbsoluteUri})", true));
               discordMessage = await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()));
            }
            else if (queueItem.IsYouTube && queueItem.IsSpotify)
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("Spotify", $"[-🔗-]({queueItem.SpotifyUri.AbsoluteUri})", true));
               discordEmbedBuilder.AddField(new DiscordEmbedField("YouTube", $"[-🔗-]({queueItem.YouTubeUri.AbsoluteUri})", true));
               discordMessage = await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()));
            }
            else
               discordMessage = await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).WithContent(audioDownloadError));

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
               Stream ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;
               Task ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink);

               int timeSpanAdvanceInt = 0;
               while (!ffmpegCopyTask.IsCompleted)
               {
                  if (timeSpanAdvanceInt % 10 == 0)
                  {
                     discordEmbedBuilder.Description = TimeLineStringBuilderWhilePlaying(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);
                     await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()));
                  }

                  if (cancellationToken.IsCancellationRequested)
                  {
                     ffmpegStream.Close();
                     break;
                  }

                  timeSpanAdvanceInt++;
                  await Task.Delay(1000);
               }

               discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_stream", "Skipped!", true, discordComponentEmojisNext);
               discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_stream", "Stop!", true, discordComponentEmojisStop);
               discordComponents[2] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Success, "shuffle_stream", "Shuffle!", true, discordComponentEmojisShuffle);
               discordComponents[3] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Secondary, "queue_stream", "Show queue!", true, discordComponentEmojisQueue);

               discordEmbedBuilder.Description = TimeLineStringBuilderAfterSong(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);
               await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()));

               if (!cancellationToken.IsCancellationRequested)
               {
                  CancellationTokenItemList.RemoveAll(x => x.CancellationTokenSource.Token == cancellationToken && x.DiscordGuild == discordGuild);
               }
            }
         }
         catch (Exception exc)
         {
            interactionChannel.SendMessageAsync("Something went wrong!" + exc);

            if (interactionContext != null)
               interactionContext.Client.Logger.LogError(exc.Message);
            else
               discordClient.Logger.LogError(exc.Message);
         }
         finally
         {
            await voiceTransmitSink.FlushAsync();

            if (!cancellationToken.IsCancellationRequested)
            {
               if (_queueItemList.All(x => x.DiscordGuild != discordGuild))
               {
                  interactionChannel.SendMessageAsync("Queue is empty!");
               }

               foreach (QueueItem queueListItem in _queueItemList)
               {
                  if (queueListItem.DiscordGuild == discordGuild)
                  {
                     CancellationTokenSource cancellationTokenSource = new();
                     CancellationToken token = cancellationTokenSource.Token;
                     CancellationTokenItem cancellationTokenItem = new(discordGuild, cancellationTokenSource);
                     CancellationTokenItemList.Add(cancellationTokenItem);
                     if (interactionContext != null)
                        Task.Run(() => PlayFromQueueAsyncTask(interactionContext, interactionContext.Client, interactionContext.Guild, interactionContext.Client.CurrentUser.ConvertToMember(interactionContext.Guild).Result,
                           interactionContext.Channel, queueListItem, token, false));
                     else
                        Task.Run(() => PlayFromQueueAsyncTask(interactionContext, discordClient, discordGuild, discordMember, interactionChannel, queueListItem, token, false));
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
            descriptionString = "▶️";
         if (playerAdvanceTimeSpan.Hours != 0)
            descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Hours:#00}:{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
         else
            descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";

         return descriptionString;
      }

      public static string TimeLineStringBuilderAfterSong(int timeSpanAdvanceInt, TimeSpan totalTimeSpan, CancellationToken cancellationToken)
      {
         TimeSpan playerAdvanceTimeSpan = TimeSpan.FromSeconds(timeSpanAdvanceInt);

         string durationString = "";
         if (playerAdvanceTimeSpan.Hours != 0)
            durationString = $"{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}";
         else
            durationString = $"{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}";

         if (!cancellationToken.IsCancellationRequested)
         {
            return $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
         }
         else
         {
            string descriptionString = "⏹️";
            if (cancellationToken.IsCancellationRequested)
               descriptionString = "▶️";
            string playerAdvanceString = PlayerAdvance(timeSpanAdvanceInt, totalTimeSpan);

            if (playerAdvanceTimeSpan.Hours != 0)
               descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Hours:#00}:{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
            else
               descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";

            return descriptionString;
         }
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
               strings[i] = "🔘";
            else
               strings[i] = "▬";
         }

         foreach (string item in strings)
         {
            playerAdvanceString += item;
         }

         return playerAdvanceString;
      }

      public static AcoustId.Root AcoustIdFromFingerPrint(Uri filePathUri)
      {
         string[] fingerPrintDuration = default(string[]);
         string[] fingerPrintFingerprint = default(string[]);
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
            string url = "http://api.acoustid.org/v2/lookup?client=" + Bot.Connections.AcoustIdApiKey + "&duration=" + fingerPrintDuration[1] + "&fingerprint=" + fingerPrintFingerprint[1] +
                         "&meta=recordings+recordingIds+releases+releaseIds+ReleaseGroups+releaseGroupIds+tracks+compress+userMeta+sources";

            string httpClientContent = new HttpClient().GetStringAsync(url).Result;
            acoustId = AcoustId.CreateObj(httpClientContent);
         }

         return acoustId;
      }

      public static DiscordEmbedBuilder CustomDiscordEmbedBuilder(Uri filePathUri, VideoData audioDownloadMetaData, TagLib.File metaTagFileToPlay)
      {
         DiscordEmbedBuilder discordEmbedBuilder = new()
         {
            Title = "Preset"
         };

         if (filePathUri != null)
         {
            AcoustId.Root acoustIdRoot = AcoustIdFromFingerPrint(filePathUri);
            string recordingMbId = "";
            bool needThumbnail = true;
            if (acoustIdRoot.Results?.Count > 0 && acoustIdRoot.Results[0].Recordings?[0].Releases != null)
            {
               DateTime rightAlbumDateTime = new();
               AcoustId.Release rightAlbum = new();
               /*AcoustId.Artist rightArtist = new();
               if (acoustIdRoot.Results[0].Recordings[0].Artists != null)
                  rightArtist = acoustIdRoot.Results[0].Recordings[0].Artists[0];*/

               foreach (AcoustId.Release albumItem in acoustIdRoot.Results[0].Recordings[0].Releases)
               {
                  if (acoustIdRoot.Results[0].Recordings[0].Releases.Count == 1)
                  {
                     rightAlbum = albumItem;
                     break;
                  }

                  if (albumItem.Date == null || albumItem.Date.Year == 0)
                     continue;

                  if (albumItem.Date.Month == 0)
                     albumItem.Date.Month = 1;
                  if (albumItem.Date.Day == 0)
                     albumItem.Date.Day = 1;

                  if (rightAlbumDateTime.Equals(new DateTime()))
                     rightAlbumDateTime = new DateTime(albumItem.Date.Year, albumItem.Date.Month, albumItem.Date.Day);

                  DateTime albumItemDateTime = new(albumItem.Date.Year, albumItem.Date.Month, albumItem.Date.Day);
                  if (rightAlbumDateTime >= albumItemDateTime)
                  {
                     rightAlbum = albumItem;
                     rightAlbumDateTime = albumItemDateTime;
                  }
               }
               if (rightAlbum.Title == "")
                  rightAlbum.Title = "N/A";

               recordingMbId = acoustIdRoot.Results[0].Recordings[0].Id;
               IRecording iRecording = new Query().LookupRecordingAsync(new Guid(recordingMbId)).Result;

               string genres = "";
               if (iRecording.Genres != null)
               {
                  foreach (char genre in genres)
                  {
                     genres += genre;

                     if (genres.Last() != genre)
                        genres += ", ";
                  }
               }
               if (genres == "")
                  genres = "N/A";

               discordEmbedBuilder.AddField(new DiscordEmbedField("Album", rightAlbum.Title, true));
               discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));
               if (rightAlbum.Id != null)
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

            discordEmbedBuilder.Title = audioDownloadMetaData.Title;
            discordEmbedBuilder.WithAuthor(audioDownloadMetaData.Creator);
            discordEmbedBuilder.AddField(new DiscordEmbedField("Uploader", audioDownloadMetaData.Uploader, true));

            /*string tags = "";
            for (int i = 0; i < audioDownloadMetaData.Tags.Length; i++)
            {
               tags += audioDownloadMetaData.Tags[i];

               if (i > 2)
                  break;
               else
                  tags += ", ";
            }

            if (tags == "")
               tags = "Empty";

            discordEmbedBuilder.AddField(new DiscordEmbedField("Tags", tags, true));*/

            discordEmbedBuilder.WithUrl(audioDownloadMetaData.WebpageUrl);

            if (needThumbnail)
            {
               discordEmbedBuilder.WithThumbnail(audioDownloadMetaData.Thumbnails[18].Url);
               Stream streamForBitmap = new HttpClient().GetStreamAsync(audioDownloadMetaData.Thumbnails[18].Url).Result;

               Bitmap bitmapAlbumCover = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Bitmap(streamForBitmap) : null;
               if (bitmapAlbumCover != null)
               {
                  Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
                  discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
               }
            }
            if (recordingMbId != "")
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainz", $"[-🔗-](https://musicbrainz.org/recording/{recordingMbId})", true));
         }
         else if (metaTagFileToPlay != null)
         {
            discordEmbedBuilder.Title = metaTagFileToPlay.Tag.Title;
            discordEmbedBuilder.WithAuthor(metaTagFileToPlay.Tag.JoinedPerformers);
            if (metaTagFileToPlay.Tag.Album != null)
               discordEmbedBuilder.AddField(new DiscordEmbedField("Album", metaTagFileToPlay.Tag.Album, true));
            if (metaTagFileToPlay.Tag.JoinedGenres != null)
               discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", metaTagFileToPlay.Tag.JoinedGenres, true));

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
                     bitmapStream = new HttpClient().GetStreamAsync($"https://coverartarchive.org/release-group/{metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId}/front").Result;
                     discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release-group/{metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId}/front");
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
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzArtistId", metaTagFileToPlay.Tag.MusicBrainzArtistId));
               if (metaTagFileToPlay.Tag.MusicBrainzDiscId != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzDiscId", metaTagFileToPlay.Tag.MusicBrainzDiscId));
               if (metaTagFileToPlay.Tag.MusicBrainzReleaseArtistId != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseArtistId", metaTagFileToPlay.Tag.MusicBrainzReleaseArtistId));
               if (metaTagFileToPlay.Tag.MusicBrainzReleaseCountry != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseCountry", metaTagFileToPlay.Tag.MusicBrainzReleaseCountry));
               if (metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseGroupId", metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId));
               if (metaTagFileToPlay.Tag.MusicBrainzReleaseId != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseId", metaTagFileToPlay.Tag.MusicBrainzReleaseId));
               if (metaTagFileToPlay.Tag.MusicBrainzReleaseStatus != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseStatus", metaTagFileToPlay.Tag.MusicBrainzReleaseStatus));
               if (metaTagFileToPlay.Tag.MusicBrainzReleaseType != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseType", metaTagFileToPlay.Tag.MusicBrainzReleaseType));
               if (metaTagFileToPlay.Tag.MusicBrainzTrackId != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzTrackId", metaTagFileToPlay.Tag.MusicBrainzTrackId));
               if (metaTagFileToPlay.Tag.MusicIpId != null)
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicIpId", metaTagFileToPlay.Tag.MusicIpId));
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
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
            return;
         }

         if (_queueItemList.Any(x => x.DiscordGuild == interactionContext.Guild))
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Youtube music is playing! This interaction is locked!"));
            return;
         }

         CancellationTokenSource tokenSource = null;
         foreach (CancellationTokenItem keyValuePairItem in CancellationTokenItemList.Where(x => x.DiscordGuild == interactionContext.Guild))
         {
            tokenSource = keyValuePairItem.CancellationTokenSource;
            CancellationTokenItemList.Remove(keyValuePairItem);
            break;
         }

         if (tokenSource != null)
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Stopped the music!"));
            tokenSource.Cancel();
            tokenSource.Dispose();
         }
         else
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing to stop!"));
      }

      [SlashCommand("Skip" + Bot.isDevBot, "Skip this song!")]
      private async Task SkipCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
         await PlayMusic.NextSongTask(interactionContext);
      }

      [SlashCommand("Next" + Bot.isDevBot, "Skip this song!")]
      private async Task NextCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
         await PlayMusic.NextSongTask(interactionContext);
      }

      [SlashCommand("Shuffle" + Bot.isDevBot, "Randomize the queue!")]
      private async Task Shuffle(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

         DiscordMessage discordMessage = await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shuffle requested!"));

         await ShufflePlaylist(discordMessage);
      }

      private static async Task ShufflePlaylist(DiscordMessage discordMessage)
      {
         if (_queueCreating)
         {
            await discordMessage.ModifyAsync(x => x.WithContent("Queue is generating!"));
         }
         else
         {
            List<QueueItem> queueItemListMixed = new();
            List<QueueItem> queueItemList = _queueItemList;

            int queueLength = _queueItemList.Count;
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
               queueItemListMixed.Add(queueItemList[randomInt]);
            }

            _queueItemList = queueItemListMixed;

            await discordMessage.ModifyAsync(x => x.WithContent("Queue has been altered!"));
         }
      }

      private static async Task NextSongTask(InteractionContext interactionContext)
      {
         if (interactionContext.Member.VoiceState == null)
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
            return;
         }

         if (_queueItemList.Any(x => x.DiscordGuild == interactionContext.Guild))
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Youtube music is playing! This interaction is locked!"));
            return;
         }

         CancellationTokenSource tokenSource = null;
         foreach (CancellationTokenItem keyValuePairItem in CancellationTokenItemList.Where(x => x.DiscordGuild == interactionContext.Guild))
         {
            tokenSource = keyValuePairItem.CancellationTokenSource;
            CancellationTokenItemList.Remove(keyValuePairItem);
            break;
         }

         if (tokenSource != null)
         {
            tokenSource.Cancel();
            tokenSource.Dispose();
         }
         else
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing to skip!"));
            return;
         }

         tokenSource = new CancellationTokenSource();
         CancellationToken cancellationToken = tokenSource.Token;
         CancellationTokenItem cancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
         CancellationTokenItemList.Add(cancellationTokenKeyPair);

         try
         {
            Task.Run(() => DrivePlayTask(interactionContext, null, null, null, null, cancellationToken, false), cancellationToken);
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            CancellationTokenItemList.Remove(cancellationTokenKeyPair);
         }
      }

      internal static Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
      {
         eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

         switch (eventArgs.Id)
         {
            case "next_song":
               {
                  if (_queueItemList.Any(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     eventArgs.Channel.SendMessageAsync("Youtube music is playing! This interaction is locked!");
                     return Task.CompletedTask;
                  }

                  DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;

                  if (discordMember.VoiceState == null)
                  {
                     eventArgs.Channel.SendMessageAsync("You have to be connected!");
                     return Task.CompletedTask;
                  }

                  CancellationTokenSource tokenSource = null;
                  foreach (CancellationTokenItem keyValuePairItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     tokenSource = keyValuePairItem.CancellationTokenSource;
                     CancellationTokenItemList.Remove(keyValuePairItem);
                     break;
                  }

                  if (tokenSource != null)
                  {
                     tokenSource.Cancel();
                     tokenSource.Dispose();
                  }
                  else
                  {
                     eventArgs.Channel.SendMessageAsync("Nothing to skip!");
                     return Task.CompletedTask;
                  }

                  tokenSource = new CancellationTokenSource();
                  CancellationToken cancellationToken = tokenSource.Token;
                  CancellationTokenItem cancellationTokenKeyPair = new(eventArgs.Guild, tokenSource);
                  CancellationTokenItemList.Add(cancellationTokenKeyPair);

                  try
                  {
                     Task.Run(() => DrivePlayTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, cancellationToken, false), cancellationToken);
                  }
                  catch (Exception ex)
                  {
                     Console.WriteLine(ex.Message);
                     CancellationTokenItemList.Remove(cancellationTokenKeyPair);
                  }

                  break;
               }
            case "stop_song":
               {
                  if (_queueItemList.Any(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     eventArgs.Channel.SendMessageAsync("Youtube music is playing! This interaction is locked!");
                     return Task.CompletedTask;
                  }

                  CancellationTokenSource tokenSource = null;
                  foreach (CancellationTokenItem keyValuePairItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     tokenSource = keyValuePairItem.CancellationTokenSource;
                     CancellationTokenItemList.Remove(keyValuePairItem);
                     break;
                  }

                  if (tokenSource != null)
                  {
                     eventArgs.Channel.SendMessageAsync("Stopped the music!");
                     tokenSource.Cancel();
                     tokenSource.Dispose();
                  }
                  else
                     eventArgs.Channel.SendMessageAsync("Nothing to stop!");

                  break;
               }
            case "next_song_stream":
               {
                  DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;

                  if (discordMember.VoiceState == null)
                  {
                     eventArgs.Channel.SendMessageAsync("You have to be connected!");
                     return Task.CompletedTask;
                  }

                  if (_queueItemList.Count == 0)
                  {
                     eventArgs.Channel.SendMessageAsync("Nothing to skip!");
                     return Task.CompletedTask;
                  }

                  List<CancellationTokenSource> cancellationTokenSourceList = new();
                  foreach (CancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
                  }

                  CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
                  {
                     cancellationToken.Cancel();
                     cancellationToken.Dispose();
                  }

                  CancellationTokenSource newCancellationTokenSource = new();
                  CancellationToken newCancellationToken = newCancellationTokenSource.Token;

                  foreach (QueueItem queueItem in _queueItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     CancellationTokenItem newCancellationTokenItem = new(eventArgs.Guild, newCancellationTokenSource);
                     CancellationTokenItemList.Add(newCancellationTokenItem);

                     Task.Run(() => PlayFromQueueAsyncTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, queueItem, newCancellationToken, false), newCancellationToken);
                     break;
                  }

                  break;
               }
            case "stop_song_stream":
               {
                  DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;

                  if (discordMember.VoiceState == null)
                  {
                     eventArgs.Channel.SendMessageAsync("You have to be connected!");
                     return Task.CompletedTask;
                  }

                  bool nothingToStop = true;

                  List<CancellationTokenSource> cancellationTokenSourceList = new();

                  foreach (CancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     nothingToStop = false;
                     cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
                  }

                  CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
                  {
                     cancellationToken.Cancel();
                     cancellationToken.Dispose();
                  }

                  _queueItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  if (nothingToStop)
                     eventArgs.Channel.SendMessageAsync("Nothing to stop!");
                  else
                     eventArgs.Channel.SendMessageAsync("Stopped the music!");

                  break;
               }
            case "shuffle_stream":
               {
                  DiscordMessage discordMessage = eventArgs.Channel.SendMessageAsync("Shuffle requested!").Result;

                  ShufflePlaylist(discordMessage);
                  break;
               }
            case "queue_stream":
               {
                  DiscordMessage discordMessage = eventArgs.Channel.SendMessageAsync("Loading!").Result;

                  if (_queueItemList.Count == 0)
                     discordMessage.ModifyAsync("Queue is empty!");
                  else
                  {
                     string descriptionString = "";
                     DiscordEmbedBuilder discordEmbedBuilder = new();
                     YoutubeClient youtubeClient = new();
                     for (int i = 0; i < 15; i++)
                     {
                        if (_queueItemList.Count == i)
                           break;

                        Video videoData = youtubeClient.Videos.GetAsync(_queueItemList[i].YouTubeUri.AbsoluteUri).Result;

                        if (_queueItemList[i].IsSpotify)
                           descriptionString += "[YouTube]" + $"({_queueItemList[i].YouTubeUri.AbsoluteUri})   " + "[Spotify]" + $"({_queueItemList[i].SpotifyUri.AbsoluteUri})  " + videoData.Title + "\n";
                        else
                           descriptionString += "[YouTube]" + $"({_queueItemList[i].YouTubeUri.AbsoluteUri})   " + "[Spotify]" + $"({_queueItemList[i].SpotifyUri.AbsoluteUri})  " + videoData.Title + "\n";
                     }

                     discordEmbedBuilder.Title = $"{_queueItemList.Count} Track/s in queue!";
                     discordEmbedBuilder.WithDescription(descriptionString);
                     discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder));
                  }

                  break;
               }
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

                  foreach (CancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     nothingToStop = false;
                     cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
                  }

                  CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
                  {
                     cancellationToken.Cancel();
                     cancellationToken.Dispose();
                  }

                  _queueItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  if (nothingToStop)
                     eventArgs.Channel.SendMessageAsync("Nothing to stop!");
                  else
                     eventArgs.Channel.SendMessageAsync("Stopped the music!");
                  VoiceNextExtension voiceNext = client.GetVoiceNext();
                  VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(eventArgs.Guild);
                  voiceNextConnection.Disconnect();
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
                  bool nothingToStop = true;

                  List<CancellationTokenSource> cancellationTokenSourceList = new();

                  foreach (CancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     nothingToStop = false;
                     cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
                  }

                  CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
                  {
                     cancellationToken.Cancel();
                     cancellationToken.Dispose();
                  }

                  _queueItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  if (eventArgs.Channel != null)
                  {
                     eventArgs.Channel.SendMessageAsync(nothingToStop ? "Nothing to stop!" : "Stopped the music!");
                  }
               }
            }
         }
         catch
         {
            // ignored
         }
      }
   }
}