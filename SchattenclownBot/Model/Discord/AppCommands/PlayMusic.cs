using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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
         IsSpotify = YouTubeUri == null;
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
      private static readonly List<QueueItem> QueueItemList = new();

      [SlashCommand(Bot.isDevBot + "DrivePlay", "Just plays some random music!")]
      private async Task DrivePlayCommand(InteractionContext interactionContext)
      {
         //check if this fix error
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

         if (interactionContext.Member.VoiceState == null)
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
            return;
         }

         if (QueueItemList.Any(x => x.DiscordGuild == interactionContext.Guild))
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing! This interaction is locked!"));
            return;
         }

         bool musicAlreadyPlaying = CancellationTokenItemList.Any(x => x.DiscordGuild == interactionContext.Guild);

         if (!musicAlreadyPlaying)
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
         try
         {
            VoiceNextExtension voiceNextExtension = interactionContext != null ? interactionContext.Client.GetVoiceNext() : discordClient.GetVoiceNext();

            if (voiceNextExtension == null)
               return;

            VoiceNextConnection voiceNextConnection = interactionContext != null ? voiceNextExtension.GetConnection(interactionContext.Guild) : voiceNextExtension.GetConnection(discordGuild);
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

               #region MetaTags
               TagLib.File metaTagFileToPlay = TagLib.File.Create(@$"{selectedFileToPlay}");
               MusicBrainz.Root musicBrainzObj = null;
               if (metaTagFileToPlay.Tag.MusicBrainzReleaseId != null)
               {
                  Uri coverArtUri = new($"https://coverartarchive.org/release/{metaTagFileToPlay.Tag.MusicBrainzReleaseId}");
                  HttpClient coverArtHttpClient = new();
                  coverArtHttpClient.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                  try
                  {
                     string httpClientContent = await coverArtHttpClient.GetStringAsync(coverArtUri);
                     musicBrainzObj = MusicBrainz.CreateObj(httpClientContent);
                  }
                  catch
                  {
                     //ignore
                  }
               }
               #endregion

               try
               {
                  #region discordEmbedBuilder
                  DiscordEmbedBuilder discordEmbedBuilder = new()
                  {
                     Title = metaTagFileToPlay.Tag.Title
                  };
                  discordEmbedBuilder.WithAuthor(metaTagFileToPlay.Tag.JoinedPerformers);
                  if (metaTagFileToPlay.Tag.Album != null)
                     discordEmbedBuilder.AddField(new DiscordEmbedField("Album", metaTagFileToPlay.Tag.Album, true));
                  if (metaTagFileToPlay.Tag.JoinedGenres != null)
                     discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", metaTagFileToPlay.Tag.JoinedGenres, true));

                  HttpClient bitmapHttpClient = new();
                  Stream bitmapStream = null;
                  if (musicBrainzObj != null)
                  {
                     discordEmbedBuilder.WithThumbnail(musicBrainzObj.Images.FirstOrDefault().ImageString);
                     bitmapStream = await bitmapHttpClient.GetStreamAsync(musicBrainzObj.Images.FirstOrDefault().ImageString);
                     discordEmbedBuilder.WithUrl(musicBrainzObj.Release);
                  }
                  else if (metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId != null)
                  {
                     discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release-group/{metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId}/front");
                     bitmapStream = await bitmapHttpClient.GetStreamAsync($"https://coverartarchive.org/release-group/{metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId}/front");
                  }

                  if (bitmapStream != null)
                  {
                     Bitmap albumCoverBitmap = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Bitmap(bitmapStream) : null;
                     if (albumCoverBitmap != null)
                     {
                        Color dominantColor = ColorMath.GetDominantColor(albumCoverBitmap);
                        discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
                     }
                  }
                  else
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
                  #endregion

                  DiscordMessage discordMessage = interactionContext != null ? await interactionContext.Channel.SendMessageAsync(discordEmbedBuilder.Build()) : await interactionDiscordChannel.SendMessageAsync(discordEmbedBuilder.Build());

                  DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
                  DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
                  DiscordComponent[] discordComponents = new DiscordComponent[2];
                  discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song", "Next!", false, discordComponentEmojisNext);
                  discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song", "Stop!", false, discordComponentEmojisStop);

                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents));

                  ProcessStartInfo ffmpegProcessStartInfo = new()
                  {
                     FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/ffmpeg" : "..\\..\\..\\ffmpeg\\ffmpeg.exe",
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

      [SlashCommand(Bot.isDevBot + "Play", "Play spotify or youtube link!")]
      private async Task PlayCommand(InteractionContext interactionContext, [Option("Link", "Link!")] string webLink)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
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
            int playlistSelectedVideoIndex = 1;
            Uri selectedVideoUri = default;
            Uri playlistUri = webLinkUri;
            string selectedVideoId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "watch?v=", "watch?v=".Length), "&list=", 0);

            if (isYouTubePlaylist)
            {
               string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "&list=", "&list=".Length), "&index=", 0);
               if (isYouTubePlaylistWithIndex)
               {
                  playlistSelectedVideoIndex = Convert.ToInt32(StringCutter.RemoveUntilWord(webLink, "&index=", "&index=".Length));
                  playlistUri = new Uri("https://www.youtube.com/playlist?list=" + playlistId);
               }
            }
            else
            {
               selectedVideoUri = new Uri("https://www.youtube.com/watch?v=" + selectedVideoId);
            }

            Uri uri = new(@"N:\");
            YoutubeDL youtubeDl = new()
            {
               YoutubeDLPath = "..\\..\\..\\youtube-dl\\yt-dlp.exe",
               FFmpegPath = "..\\..\\..\\ffmpeg\\ffmpeg.exe",
               OutputFolder = uri.AbsolutePath,
               RestrictFilenames = false,
               OverwriteFiles = true,
               IgnoreDownloadErrors = false
            };

            try
            {
               VideoData[] videoDataArray = default;
               if (isYouTubePlaylist)
               {
                  OptionSet optionSet = new()
                  {
                     PlaylistStart = playlistSelectedVideoIndex
                  };
                  videoDataArray = youtubeDl.RunVideoDataFetch(playlistUri.AbsoluteUri, new CancellationToken(), true, optionSet).Result.Data.Entries;
               }

               if (MusicAlreadyPlaying(interactionContext.Guild))
               {
                  if (isYouTubePlaylist)
                  {
                     foreach (VideoData videoData in videoDataArray)
                     {
                        QueueItem queueItem = new(interactionContext.Guild, new Uri(videoData.Url), null);
                        QueueItemList.Add(queueItem);
                     }
                  }
                  else
                  {
                     QueueItem queueItem = new(interactionContext.Guild, selectedVideoUri, null);
                     QueueItemList.Add(queueItem);
                  }
                  await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing already! Your songs are in the queue now!"));
               }
               else
               {
                  if (isYouTubePlaylist)
                  {
                     await PlayQueueAsyncTask(interactionContext, new Uri(videoDataArray[0].Url), null);

                     for (int i = 1; i < videoDataArray.Length; i++)
                     {
                        QueueItem queueItem = new(interactionContext.Guild, new Uri(videoDataArray[i].Url), null);
                        QueueItemList.Add(queueItem);
                     }
                  }
                  else
                     await PlayQueueAsyncTask(interactionContext, selectedVideoUri, null);
               }
            }
            catch
            {
               await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error!"));
            }
         }
         else if (isSpotify)
         {
            List<PlaylistTrack<IPlayableItem>> spotifyPlaylistItems = default;
            List<SimpleTrack> spotifyAlbumItems = default;

            if (isSpotifyPlaylist)
            {
               string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/playlist/", "/playlist/".Length), "?si", 0);
               SpotifyClient spotifyClient = GetSpotifyClientConfig();
               spotifyPlaylistItems = spotifyClient.Playlists.GetItems(playlistId).Result.Items;
            }
            else if (isSpotifyAlbum)
            {
               string albumId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/album/", "/album/".Length), "?si", 0);
               SpotifyClient spotifyClient = GetSpotifyClientConfig();
               spotifyAlbumItems = spotifyClient.Albums.GetTracks(albumId).Result.Items;
            }

            if (MusicAlreadyPlaying(interactionContext.Guild))
            {
               if (isSpotifyPlaylist)
               {
                  if (spotifyPlaylistItems != null && spotifyPlaylistItems.Count != 0)
                  {
                     foreach (PlaylistTrack<IPlayableItem> playlistItem in spotifyPlaylistItems)
                     {
                        if (playlistItem.Track is FullTrack spotifyTrack)
                        {
                           QueueItem queueKeyPair = new(interactionContext.Guild, null, new Uri("https://open.spotify.com/track/" + spotifyTrack.Id));
                           QueueItemList.Add(queueKeyPair);
                        }
                     }
                  }
               }
               else if (isSpotifyAlbum)
               {
                  if (spotifyAlbumItems != null)
                     foreach (SimpleTrack albumItem in spotifyAlbumItems)
                     {
                        QueueItem queueKeyPair = new(interactionContext.Guild, null, new Uri("https://open.spotify.com/track/" + albumItem.Id));
                        QueueItemList.Add(queueKeyPair);
                     }
               }
               else
               {
                  QueueItem queueKeyPair = new(interactionContext.Guild, null, webLinkUri);
                  QueueItemList.Add(queueKeyPair);
               }
               await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing already! Your songs are in the queue now!"));
            }
            else
            {
               if (isSpotifyPlaylist)
               {
                  if (spotifyPlaylistItems != null && spotifyPlaylistItems.Count != 0)
                  {
                     FullTrack spotifyTrack = spotifyPlaylistItems[0].Track as FullTrack;
                     await PlayQueueAsyncTask(interactionContext, null, new Uri("https://open.spotify.com/track/" + spotifyTrack!.Id));

                     for (int i = 1; i < spotifyPlaylistItems.Count; i++)
                     {
                        spotifyTrack = spotifyPlaylistItems[i].Track as FullTrack;
                        QueueItem queueKeyPair = new(interactionContext.Guild, null, new Uri("https://open.spotify.com/track/" + spotifyTrack!.Id));
                        QueueItemList.Add(queueKeyPair);
                     }
                  }
               }
               else if (isSpotifyAlbum)
               {
                  if (spotifyAlbumItems != null)
                  {
                     SimpleTrack spotifyTrack = spotifyAlbumItems[0];
                     await PlayQueueAsyncTask(interactionContext, null, new Uri("https://open.spotify.com/track/" + spotifyTrack!.Id));

                     for (int i = 1; i < spotifyAlbumItems.Count; i++)
                     {
                        spotifyTrack = spotifyAlbumItems[i];
                        QueueItem queueKeyPair = new(interactionContext.Guild, null, new Uri("https://open.spotify.com/track/" + spotifyTrack!.Id));
                        QueueItemList.Add(queueKeyPair);
                     }
                  }
               }
               else
                  await PlayQueueAsyncTask(interactionContext, null, webLinkUri);
            }
         }
         else
         {

         }
      }

      public static SpotifyClient GetSpotifyClientConfig()
      {
         SpotifyClientConfig spotifyClientConfig = SpotifyClientConfig.CreateDefault();
         ClientCredentialsRequest clientCredentialsRequest = new(Bot.Connections.Token.ClientId, Bot.Connections.Token.ClientSecret);
         ClientCredentialsTokenResponse clientCredentialsTokenResponse = new OAuthClient(spotifyClientConfig).RequestToken(clientCredentialsRequest).Result;
         SpotifyClient spotifyClient = new(clientCredentialsTokenResponse.AccessToken);
         return spotifyClient;
      }

      private static bool MusicAlreadyPlaying(DiscordGuild discordGuild)
      {
         return CancellationTokenItemList.Any(cancellationTokenItem => cancellationTokenItem.DiscordGuild == discordGuild);
      }

      private static Task PlayQueueAsyncTask(InteractionContext interactionContext, Uri youtubeUri, Uri spotifyUri)
      {
         CancellationTokenSource tokenSource = new();
         CancellationToken cancellationToken = tokenSource.Token;
         CancellationTokenItem cancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
         CancellationTokenItemList.Add(cancellationTokenKeyPair);

         QueueItem queueKeyPair = youtubeUri != null ? new QueueItem(interactionContext.Guild, youtubeUri, null) : new QueueItem(interactionContext.Guild, null, spotifyUri);

         QueueItemList.Add(queueKeyPair);

         try
         {
            Task.Run(() => PlayFromQueueAsyncTask(interactionContext, null, null, null, null, youtubeUri, cancellationToken, true), cancellationToken);
         }
         catch
         {
            CancellationTokenItemList.Remove(cancellationTokenKeyPair);
         }

         return Task.CompletedTask;
      }

      private static async Task PlayFromQueueAsyncTask(InteractionContext interactionContext, DiscordClient discordClient, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionChannel, Uri youtubeUri, CancellationToken cancellationToken, bool isInitialMessage)
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
            QueueItem queueItemObj = new();
            foreach (QueueItem queueItem in QueueItemList)
            {
               if (((queueItem.DiscordGuild == discordGuild) && queueItem.YouTubeUri != null && queueItem.IsYouTube) || (queueItem.SpotifyUri != null && queueItem.IsSpotify))
               {
                  queueItemObj = queueItem;
                  QueueItemList.Remove(queueItem);
                  break;
               }
            }

            SpotDl spotDlMetaData = new();
            if (queueItemObj.IsSpotify)
            {
               string trackId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(queueItemObj.SpotifyUri.AbsoluteUri, "/track/", "/track/".Length), "?si", 0);

               ProcessStartInfo spotDlProcessStartInfo = new()
               {
                  FileName = "..\\..\\..\\spotdl\\spotdl.exe",
                  Arguments = "--restrict --ffmpeg ..\\..\\..\\ffmpeg\\ffmpeg.exe --save-file "
               };
               spotDlProcessStartInfo.Arguments += "..\\..\\..\\spotdl\\tracks\\" + $@"{trackId}.spotdl --preload save {queueItemObj.SpotifyUri} ";
               await Process.Start(spotDlProcessStartInfo)!.WaitForExitAsync();

               StreamReader streamReaderTrack = new("..\\..\\..\\spotdl\\tracks\\" + $@"{trackId}.spotdl");
               string jsonTrackInfos = await streamReaderTrack.ReadToEndAsync();
               spotDlMetaData = JsonConvert.DeserializeObject<List<SpotDl>>(jsonTrackInfos)?[0];
               if (spotDlMetaData != null)
                  youtubeUri = new Uri(spotDlMetaData.download_url);
            }

            Uri networkDriveUri = new(@"N:\");
            YoutubeDL youtubeDl = new()
            {
               YoutubeDLPath = "..\\..\\..\\youtube-dl\\yt-dlp.exe",
               FFmpegPath = "..\\..\\..\\ffmpeg\\ffmpeg.exe",
               OutputFolder = networkDriveUri.AbsolutePath,
               RestrictFilenames = true,
               OverwriteFiles = true,
               IgnoreDownloadErrors = false
            };

            OptionSet optionSet = new()
            {
               AddMetadata = true
            };
            RunResult<string> audioDownload = await youtubeDl.RunAudioDownload(youtubeUri.AbsoluteUri, AudioConversionFormat.Opus, new CancellationToken(), null, null, optionSet);
            VideoData audioDownloadMetaData = youtubeDl.RunVideoDataFetch(youtubeUri.AbsoluteUri).Result.Data;
            TimeSpan audioDownloadTimeSpan = default;
            if (audioDownloadMetaData != null && audioDownloadMetaData.Duration != null)
               audioDownloadTimeSpan = new TimeSpan(0, 0, 0, (int)audioDownloadMetaData.Duration.Value);

            bool wildFunctionSuccess = false;
            DiscordEmbedBuilder discordEmbedBuilder = null;
            string audioDownloadError = null;

            if (queueItemObj.IsYouTube && audioDownload.ErrorOutput.Length <= 1)
            {
               discordEmbedBuilder = CustomDiscordEmbedBuilder(null, new Uri(audioDownload.Data), audioDownloadMetaData);
               if (discordEmbedBuilder != null)
                  wildFunctionSuccess = true;
            }
            else if (queueItemObj.IsSpotify)
            {
               if (spotDlMetaData != null)
               {
                  discordEmbedBuilder = CustomDiscordEmbedBuilder(spotDlMetaData, null, null);
               }
            }
            else if (audioDownload.ErrorOutput.Length > 1)
            {
               audioDownloadError = $"{audioDownload.ErrorOutput[1]} `{youtubeUri.AbsoluteUri}`";
            }

            DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
            DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
            DiscordComponent[] discordComponents = new DiscordComponent[2];
            discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_yt", "Next!", false, discordComponentEmojisNext);
            discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_yt", "Stop!", false, discordComponentEmojisStop);

            DiscordMessage discordMessage;
            if (queueItemObj.IsSpotify)
               discordMessage = await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()).WithContent(queueItemObj.SpotifyUri.AbsoluteUri));
            else if (wildFunctionSuccess)
               discordMessage = await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()).WithContent(youtubeUri.AbsoluteUri));
            else
               discordMessage = await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).WithContent(audioDownloadError));

            ProcessStartInfo ffmpegProcessStartInfo = new()
            {
               FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/ffmpeg" : "..\\..\\..\\ffmpeg\\ffmpeg.exe",
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

                     if (queueItemObj.IsYouTube)
                        await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUri.AbsoluteUri).WithEmbed(discordEmbedBuilder.Build()));
                     else if (queueItemObj.IsSpotify)
                        await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()).WithContent(queueItemObj.SpotifyUri.AbsoluteUri));
                  }

                  if (cancellationToken.IsCancellationRequested)
                  {
                     ffmpegStream.Close();
                     break;
                  }

                  timeSpanAdvanceInt++;
                  await Task.Delay(1000);
               }

               discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_yt", "Skipped!", true, discordComponentEmojisNext);
               discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_yt", "Stop!", true, discordComponentEmojisStop);
               discordEmbedBuilder.Description = TimeLineStringBuilderAfterSong(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);
               if (queueItemObj.IsYouTube)
                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUri.AbsoluteUri).WithEmbed(discordEmbedBuilder.Build()));
               else if (queueItemObj.IsSpotify)
                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()).WithContent(queueItemObj.SpotifyUri.AbsoluteUri));

               if (!cancellationToken.IsCancellationRequested)
               {
                  foreach (CancellationTokenItem tokenKeyPair in CancellationTokenItemList.Where(x => x.DiscordGuild == (interactionContext != null ? interactionContext.Guild : discordGuild)))
                  {
                     CancellationTokenItemList.Remove(tokenKeyPair);
                     break;
                  }
               }
            }
         }
         catch (Exception exc)
         {
            interactionChannel.SendMessageAsync("Something went wrong!");

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
               if (QueueItemList.All(x => x.DiscordGuild != discordGuild))
               {

               }

               foreach (QueueItem queueKeyPairItem in QueueItemList)
               {
                  if (queueKeyPairItem.DiscordGuild == discordGuild)
                  {
                     CancellationTokenSource tokenSource = new();
                     CancellationToken newCancellationToken = tokenSource.Token;
                     CancellationTokenItem cancellationTokenKeyPair = new(discordGuild, tokenSource);
                     CancellationTokenItemList.Add(cancellationTokenKeyPair);
                     if (interactionContext != null)
                        Task.Run(() => PlayFromQueueAsyncTask(interactionContext, interactionContext.Client, interactionContext.Guild, interactionContext.Client.CurrentUser.ConvertToMember(interactionContext.Guild).Result,
                           interactionContext.Channel, queueKeyPairItem.YouTubeUri, newCancellationToken, false));
                     else
                        Task.Run(() => PlayFromQueueAsyncTask(interactionContext, discordClient, discordGuild, discordMember, interactionChannel, queueKeyPairItem.YouTubeUri, newCancellationToken, false));
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
         string durationString = $"{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}";

         if (!cancellationToken.IsCancellationRequested)
         {
            return $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
         }
         else
         {
            TimeSpan playerAdvanceTimeSpan = TimeSpan.FromSeconds(timeSpanAdvanceInt);
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
            FileName = "..\\..\\..\\fpcalc\\fpcalc.exe",
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

            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "C# console program");
            string httpClientContent = httpClient.GetStringAsync(url).Result;
            acoustId = AcoustId.CreateObj(httpClientContent);
         }

         return acoustId;
      }

      public static DiscordEmbedBuilder CustomDiscordEmbedBuilder(SpotDl spotDl, Uri filePathUri, VideoData audioDownloadMetaData)
      {
         DiscordEmbedBuilder discordEmbedBuilder = new();
         if (spotDl != null)
         {
            discordEmbedBuilder.Title = spotDl.name;

            string artists = "";
            if (spotDl.artists.Count > 0)
            {
               foreach (string artist in spotDl.artists)
               {
                  artists += artist;
                  if (spotDl.artists.Last() != artist)
                     artists += ", ";
               }

               discordEmbedBuilder.WithAuthor(artists);
            }
            else
               discordEmbedBuilder.WithAuthor(spotDl.artist);

            string genres = "";
            if (spotDl.genres.Count > 0)
            {
               foreach (string genre in spotDl.genres)
               {
                  genres += genre;
                  if (spotDl.genres.Last() != genre)
                     genres += ", ";
               }
            }
            else
               genres = "N/A";

            discordEmbedBuilder.AddField(new DiscordEmbedField("Album", spotDl.album_name, true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));
            discordEmbedBuilder.WithUrl(spotDl.download_url);

            if (spotDl.cover_url != "")
            {
               try
               {
                  discordEmbedBuilder.WithThumbnail(spotDl.cover_url);
                  Stream streamForBitmap = new HttpClient().GetStreamAsync(spotDl.cover_url).Result;

                  Bitmap bitmapAlbumCover = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Bitmap(streamForBitmap) : null;
                  if (bitmapAlbumCover != null)
                  {
                     Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
                     discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
                  }
               }
               catch
               {
                  //invalid url
               }
            }
         }
         else if (filePathUri != null)
         {
            AcoustId.Root acoustIdRoot = AcoustIdFromFingerPrint(filePathUri);

            if (acoustIdRoot.Results != null && acoustIdRoot.Results.Count != 0 && acoustIdRoot.Results[0].Recordings[0] != null && acoustIdRoot.Results[0].Recordings[0].Releases != null)
            {
               string recordingMbId = acoustIdRoot.Results[0].Recordings[0].Id;
               Query musicBrainzQuery = new();
               IRecording iRecording = musicBrainzQuery.LookupRecordingAsync(new Guid(recordingMbId)).Result;

               string genres = "N/A";

               DateTime rightAlbumDateTime = new();
               AcoustId.Release rightAlbum = new();
               AcoustId.Artist rightArtist = new();
               if (acoustIdRoot.Results[0].Recordings[0].Artists != null)
                  rightArtist = acoustIdRoot.Results[0].Recordings[0].Artists[0];

               foreach (AcoustId.Release albumItem in acoustIdRoot.Results[0].Recordings[0].Releases)
               {
                  if (acoustIdRoot.Results[0].Recordings[0].Releases.Count == 1)
                  {
                     rightAlbum = albumItem;
                     break;
                  }

                  if (albumItem.Date == null || albumItem.Date.Year == 0 || albumItem.Date.Month == 0 || albumItem.Date.Day == 0)
                     continue;

                  if (rightAlbumDateTime.Equals(new DateTime()))
                     rightAlbumDateTime = new DateTime(albumItem.Date.Year, albumItem.Date.Month, albumItem.Date.Day);

                  DateTime albumItemDateTime = new(albumItem.Date.Year, albumItem.Date.Month, albumItem.Date.Day);
                  if (rightAlbumDateTime >= albumItemDateTime)
                  {
                     rightAlbum = albumItem;
                     rightAlbumDateTime = albumItemDateTime;
                  }
               }

               discordEmbedBuilder.Title = iRecording.Title;
               if (rightArtist != null)
                  discordEmbedBuilder.WithAuthor(rightArtist.Name);

               discordEmbedBuilder.AddField(new DiscordEmbedField("Album", rightAlbum.Title, true));
               discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));
               discordEmbedBuilder.WithUrl(audioDownloadMetaData.WebpageUrl);

               if (rightAlbum.Id != null)
               {
                  try
                  {
                     discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release/{rightAlbum.Id}/front");
                     Stream streamForBitmap = new HttpClient().GetStreamAsync($"https://coverartarchive.org/release/{rightAlbum.Id}/front").Result;

                     Bitmap bitmapAlbumCover = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Bitmap(streamForBitmap) : null;
                     if (bitmapAlbumCover != null)
                     {
                        Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
                        discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
                     }
                  }
                  catch
                  {
                     //invalid url
                  }
               }
            }
            else
            {
               discordEmbedBuilder.Title = audioDownloadMetaData.Title;
               discordEmbedBuilder.WithAuthor(audioDownloadMetaData.Creator);
               discordEmbedBuilder.AddField(new DiscordEmbedField("Uploader", audioDownloadMetaData.Uploader, true));

               string tags = "";

               for (int i = 0; i < audioDownloadMetaData.Tags.Length; i++)
               {
                  tags += audioDownloadMetaData.Tags[i];

                  if (i > 6)
                     break;
                  else
                     tags += ", ";
               }

               discordEmbedBuilder.AddField(new DiscordEmbedField("Tags", tags, true));

               discordEmbedBuilder.WithUrl(audioDownloadMetaData.WebpageUrl);
               discordEmbedBuilder.WithThumbnail(audioDownloadMetaData.Thumbnails[18].Url);
               Stream streamForBitmap = new HttpClient().GetStreamAsync(audioDownloadMetaData.Thumbnails[18].Url).Result;

               Bitmap bitmapAlbumCover = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Bitmap(streamForBitmap) : null;
               if (bitmapAlbumCover != null)
               {
                  Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
                  discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
               }
            }
         }

         return discordEmbedBuilder;
      }

      [SlashCommand(Bot.isDevBot + "Stop", "Stop the music!")]
      private async Task StopCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

         if (interactionContext.Member.VoiceState == null)
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
            return;
         }

         if (QueueItemList.Any(x => x.DiscordGuild == interactionContext.Guild))
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

      [SlashCommand(Bot.isDevBot + "Skip", "Skip this song!")]
      private async Task SkipCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
         await PlayMusic.NextSongTask(interactionContext);
      }

      [SlashCommand(Bot.isDevBot + "Next", "Skip this song!")]
      private async Task NextCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
         await PlayMusic.NextSongTask(interactionContext);
      }

      private static async Task NextSongTask(InteractionContext interactionContext)
      {
         if (interactionContext.Member.VoiceState == null)
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
            return;
         }

         if (QueueItemList.Any(x => x.DiscordGuild == interactionContext.Guild))
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
                  if (QueueItemList.Any(x => x.DiscordGuild == eventArgs.Guild))
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
                  if (QueueItemList.Any(x => x.DiscordGuild == eventArgs.Guild))
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
            case "next_song_yt":
               {
                  DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;

                  if (discordMember.VoiceState == null)
                  {
                     eventArgs.Channel.SendMessageAsync("You have to be connected!");
                     return Task.CompletedTask;
                  }

                  if (QueueItemList.Count == 0)
                  {
                     eventArgs.Channel.SendMessageAsync("Nothing to skip!");
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

                  CancellationTokenSource nextYtTokenSource = new();
                  CancellationToken nextYtCancellationToken = nextYtTokenSource.Token;
                  CancellationTokenItem nextKeyPairItem = new();

                  try
                  {
                     foreach (QueueItem queueKeyPairItem in QueueItemList)
                     {
                        if (queueKeyPairItem.DiscordGuild == eventArgs.Guild)
                        {
                           nextKeyPairItem = new CancellationTokenItem(eventArgs.Guild, nextYtTokenSource);
                           CancellationTokenItemList.Add(nextKeyPairItem);

                           Task.Run(() => PlayFromQueueAsyncTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, queueKeyPairItem.YouTubeUri, nextYtCancellationToken, false), nextYtCancellationToken);

                           break;
                        }
                     }
                  }
                  catch (Exception ex)
                  {
                     Console.WriteLine(ex.Message);
                     if (nextKeyPairItem.CancellationTokenSource != null)
                        CancellationTokenItemList.Remove(nextKeyPairItem);
                  }

                  break;
               }
            case "stop_song_yt":
               {
                  DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;

                  if (discordMember.VoiceState == null)
                  {
                     eventArgs.Channel.SendMessageAsync("You have to be connected!");
                     return Task.CompletedTask;
                  }

                  bool nothingToPlay = true;

                  foreach (CancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     nothingToPlay = false;
                     eventArgs.Channel.SendMessageAsync("Stopped the music!");
                     cancellationTokenItem.CancellationTokenSource.Cancel();
                     cancellationTokenItem.CancellationTokenSource.Dispose();
                     CancellationTokenItemList.Remove(cancellationTokenItem);
                  }

                  QueueItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  if (nothingToPlay)
                     eventArgs.Channel.SendMessageAsync("Nothing to stop!");

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
                  CancellationTokenSource tokenSource = null;
                  foreach (CancellationTokenItem keyValuePairItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     tokenSource = keyValuePairItem.CancellationTokenSource;
                     CancellationTokenItemList.Remove(keyValuePairItem);
                     break;
                  }

                  if (tokenSource != null)
                  {
                     await discordMember.VoiceState.Channel.SendMessageAsync("Stopped the music!");
                     tokenSource.Cancel();
                     tokenSource.Dispose();
                     QueueItemList.Clear();
                     VoiceNextExtension voiceNext = client.GetVoiceNext();
                     VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(eventArgs.Guild);
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
                     QueueItemList.Clear();
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