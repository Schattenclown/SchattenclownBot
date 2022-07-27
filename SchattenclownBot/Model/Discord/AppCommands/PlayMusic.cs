using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using MetaBrainz.MusicBrainz;
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
      public bool IsYouTubeUri { get; set; }

      internal QueueItem(DiscordGuild discordGuild, Uri youTubeUri, Uri spotifyUri)
      {
         DiscordGuild = discordGuild;
         YouTubeUri = youTubeUri;
         SpotifyUri = spotifyUri;
         IsYouTubeUri = YouTubeUri != null;
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

      [SlashCommand("DrivePlay", "Just plays some random music!")]
      private async Task DrivePlayCommand(InteractionContext interactionContext)
      {
         //check if this fix error
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Loading!"));

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
            catch (Exception ex)
            {
               Console.WriteLine(ex.Message);
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
                  TimeSpan timeSpanAdvance = new(0, 0, 0, 0);
                  string playerAdvance = "";
                  while (!ffmpegCopyTask.IsCompleted)
                  {
                     #region TimeLineAlgo
                     if (timeSpanAdvanceInt % 10 == 0)
                     {
                        timeSpanAdvance = TimeSpan.FromSeconds(timeSpanAdvanceInt);

                        string[] strings = new string[15];
                        double thisIsOneHundredPercent = metaTagFileToPlay.Properties.Duration.TotalSeconds;

                        double dotPositionInPercent = 100.0 / thisIsOneHundredPercent * timeSpanAdvanceInt;

                        double dotPositionInInt = 15.0 / 100.0 * dotPositionInPercent;

                        for (int i = 0; i < strings.Length; i++)
                        {
                           if (Convert.ToInt32(dotPositionInInt) == i)
                              strings[i] = "🔘";
                           else
                              strings[i] = "▬";
                        }

                        playerAdvance = "";
                        foreach (string item in strings)
                        {
                           playerAdvance += item;
                        }

                        string descriptionString = "⏹️";
                        if (cancellationToken.IsCancellationRequested)
                           descriptionString = "▶️";

                        //hierweiter
                        descriptionString += $" {playerAdvance} [{timeSpanAdvance.Hours:#00}:{timeSpanAdvance.Minutes:#00}:{timeSpanAdvance.Seconds:#00}/{metaTagFileToPlay.Properties.Duration.Hours:#00}:{metaTagFileToPlay.Properties.Duration.Minutes:#00}:{metaTagFileToPlay.Properties.Duration.Seconds:#00}] 🔉";
                        discordEmbedBuilder.Description = descriptionString;
                        await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithEmbed(discordEmbedBuilder.Build()));
                     }
                     #endregion

                     if (cancellationToken.IsCancellationRequested)
                     {
                        ffmpegStream.Close();
                        break;
                     }

                     timeSpanAdvanceInt++;
                     await Task.Delay(1000);
                  }

                  #region MoteTimeLineAlgo
                  //algorithms to create the timeline
                  string durationString = $"{metaTagFileToPlay.Properties.Duration.Hours:#00}:{metaTagFileToPlay.Properties.Duration.Minutes:#00}:{metaTagFileToPlay.Properties.Duration.Seconds:#00}";

                  if (!cancellationToken.IsCancellationRequested)
                     discordEmbedBuilder.Description = $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
                  else
                  {
                     string descriptionString = "⏹️";
                     if (cancellationToken.IsCancellationRequested)
                        descriptionString = "▶️";

                     descriptionString += $" {playerAdvance} [{timeSpanAdvance.Hours:#00}:{timeSpanAdvance.Minutes:#00}:{timeSpanAdvance.Seconds:#00}/{metaTagFileToPlay.Properties.Duration.Hours:#00}:{metaTagFileToPlay.Properties.Duration.Minutes:#00}:{metaTagFileToPlay.Properties.Duration.Seconds:#00}] 🔉";
                     discordEmbedBuilder.Description = descriptionString;
                  }
                  await discordMessage.ModifyAsync(x => x.WithEmbed(discordEmbedBuilder.Build()));
                  #endregion

                  await voiceTransmitSink.FlushAsync();
                  await voiceNextConnection.WaitForPlaybackFinishAsync();
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

      [SlashCommand("Play", "Play spotify or youtube link!")]
      private async Task PlayCommand(InteractionContext interactionContext, [Option("Link", "Link!")] string webLink)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Loading!"));
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
         else if (webLink.Contains("/track/") || webLink.Contains("/playlist/"))
         {
            isSpotify = true;

            if (webLink.Contains("/playlist/"))
            {
               isSpotifyPlaylist = true;
            }
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
            string playlistUri = "";
            string selectedVideoId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "watch?v=", "watch?v=".Length), "&list=", 0);

            if (isYouTubePlaylist)
            {
               string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "&list=", "&list=".Length), "&index=", 0);
               if(isYouTubePlaylistWithIndex)
                  playlistSelectedVideoIndex = Convert.ToInt32(StringCutter.RemoveUntilWord(webLink, "&index=", "&index=".Length));
               playlistUri = "https://www.youtube.com/playlist?list=" + playlistId;
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
               OverwriteFiles = false,
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
                  videoDataArray = youtubeDl.RunVideoDataFetch(playlistUri, new CancellationToken(), true, optionSet).Result.Data.Entries;
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
                     await PlayQueueAsyncTask(interactionContext, new Uri(videoDataArray[0].Url), null);
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
            FullTrack spotifyTrack;
            Uri spotifyTrackUri = default;

            if (isSpotifyPlaylist)
            {
               string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/playlist/", "/playlist/".Length), "?si", 0);

               SpotifyClientConfig spotifyClientConfig = SpotifyClientConfig.CreateDefault();
               ClientCredentialsRequest clientCredentialsRequest = new(Bot.Connections.Token.ClientId, Bot.Connections.Token.ClientSecret);
               ClientCredentialsTokenResponse clientCredentialsTokenResponse = await new OAuthClient(spotifyClientConfig).RequestToken(clientCredentialsRequest);
               SpotifyClient spotifyClient = new(clientCredentialsTokenResponse.AccessToken);
               spotifyPlaylistItems = spotifyClient.Playlists.GetItems(playlistId).Result.Items;

               if (spotifyPlaylistItems != null)
               {
                  spotifyTrack = spotifyPlaylistItems[0].Track as FullTrack;
                  if (spotifyTrack != null)
                     spotifyTrackUri = new Uri("https://open.spotify.com/track/" + spotifyTrack.Id);
               }
            }

            if (MusicAlreadyPlaying(interactionContext.Guild))
            {
               if (isSpotifyPlaylist)
               {
                  if (spotifyPlaylistItems != null && spotifyPlaylistItems.Count != 0)
                  {
                     foreach (PlaylistTrack<IPlayableItem> playlistItem in spotifyPlaylistItems)
                     {
                        spotifyTrack = playlistItem.Track as FullTrack;
                        if (spotifyTrack != null)
                           spotifyTrackUri = new Uri("https://open.spotify.com/track/" + spotifyTrack.Id);

                        QueueItem queueKeyPair = new(interactionContext.Guild, null, spotifyTrackUri);
                        QueueItemList.Add(queueKeyPair);
                     }
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
                  //hier msus mehr oder?
                  await PlayQueueAsyncTask(interactionContext, null, spotifyTrackUri);
               }
               else
                  await PlayQueueAsyncTask(interactionContext, null, webLinkUri);
            }
         }
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
         catch (Exception ex)
         {
            Console.WriteLine(ex.Message);
            CancellationTokenItemList.Remove(cancellationTokenKeyPair);
         }

         return Task.CompletedTask;
      }

      private static async Task PlayFromQueueAsyncTask(InteractionContext interactionContext, DiscordClient client, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionChannel, Uri youtubeUri, CancellationToken cancellationToken, bool isInitialMessage)
      {
         try
         {
            QueueItem queueItemObj = new();
            foreach (QueueItem queueListItem in QueueItemList)
            {
               if (queueListItem.DiscordGuild == (interactionContext != null ? interactionContext.Guild : discordGuild) && queueListItem.YouTubeUri != null && queueListItem.IsYouTubeUri)
               {
                  queueItemObj = queueListItem;
                  QueueItemList.Remove(queueListItem);
                  break;
               }
               else if (queueListItem.DiscordGuild == (interactionContext != null ? interactionContext.Guild : discordGuild) && queueListItem.SpotifyUri != null && !queueListItem.IsYouTubeUri)
               {
                  queueItemObj = queueListItem;
                  QueueItemList.Remove(queueListItem);
                  break;
               }
            }

            SpotDl spotDlMetaData = new();
            if (!queueItemObj.IsYouTubeUri)
            {
               string trackString = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(queueItemObj.SpotifyUri.AbsoluteUri, "/track/", "/track/".Length), "?si", 0);

               try
               {
                  ProcessStartInfo processStartInfo = new()
                  {
                     FileName = "..\\..\\..\\spotdl\\spotdl.exe",
                     Arguments = "--restrict --ffmpeg ..\\..\\..\\ffmpeg\\ffmpeg.exe --save-file "
                  };
                  processStartInfo.Arguments += "..\\..\\..\\spotdl\\tracks\\" + $@"{trackString}.spotdl --preload save ""{queueItemObj.SpotifyUri}"" ";
                  await Process.Start(processStartInfo)!.WaitForExitAsync();
                  await Task.Delay(100);

                  StreamReader streamReaderTrack = new("..\\..\\..\\spotdl\\tracks\\" + $@"{trackString}.spotdl");
                  string jsonTracks = await streamReaderTrack.ReadToEndAsync();
                  spotDlMetaData = JsonConvert.DeserializeObject<List<SpotDl>>(jsonTracks)?[0];
                  if (spotDlMetaData != null)
                     youtubeUri = new Uri(spotDlMetaData.download_url);
               }
               catch (Exception e)
               {
                  Console.WriteLine(e);
                  if (interactionContext != null)
                     await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Cant play this song!"));
                  else
                     await interactionChannel.SendMessageAsync("Cant play this song!");
                  return;
               }
            }

            VoiceNextExtension voiceNext = interactionContext != null ? interactionContext.Client.GetVoiceNext() : client.GetVoiceNext();

            if (voiceNext == null)
               return;

            VoiceNextConnection voiceNextConnection = interactionContext != null ? voiceNext.GetConnection(interactionContext.Guild) : voiceNext.GetConnection(discordGuild);
            DiscordVoiceState voiceState = interactionContext != null ? interactionContext.Member?.VoiceState : discordMember?.VoiceState;

            if (voiceState?.Channel == null)
               return;

            voiceNextConnection ??= await voiceNext.ConnectAsync(voiceState.Channel);

            if (isInitialMessage)
            {
               if (interactionContext != null)
                  await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!"));
               else
                  await interactionChannel.SendMessageAsync($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!");
            }

            Uri uri = new(@"N:\");
            YoutubeDL youtubeDl = new()
            {
               YoutubeDLPath = "..\\..\\..\\youtube-dl\\yt-dlp.exe",
               FFmpegPath = "..\\..\\..\\ffmpeg\\ffmpeg.exe",
               OutputFolder = uri.AbsolutePath,
               RestrictFilenames = true,
               OverwriteFiles = false,
               IgnoreDownloadErrors = false
            };

            OptionSet optionSet = new()
            {
               AddMetadata = true
            };
            RunResult<string> audioDownload = await youtubeDl.RunAudioDownload(youtubeUri.AbsoluteUri, AudioConversionFormat.Opus, new CancellationToken(), null, null, optionSet);
            RunResult<YoutubeDLSharp.Metadata.VideoData> audioDownloadMetaData = await youtubeDl.RunVideoDataFetch(youtubeUri.AbsoluteUri);
            TimeSpan audioDownloadTimeSpan = default;
            if (audioDownloadMetaData.Data != null && audioDownloadMetaData.Data.Duration != null)
            {
               audioDownloadTimeSpan = new TimeSpan(0, 0, 0, (int)audioDownloadMetaData.Data.Duration.Value);
            }

            try
            {
               bool wyldFunctionSuccess = false;
               DiscordEmbedBuilder discordEmbedBuilder = new();
               MetaBrainz.MusicBrainz.Interfaces.Entities.IRecording musicBrainzTags;
               TimeSpan spotDlTimeSpan = new(0);
               string audioDownloadError = null;

               if (!queueItemObj.IsYouTubeUri)
               {
                  #region discordEmbedBuilder

                  if (spotDlMetaData != null)
                  {
                     discordEmbedBuilder.Title = spotDlMetaData.name;

                     string artists = "";
                     if (spotDlMetaData.artists.Count > 0)
                     {
                        foreach (string artist in spotDlMetaData.artists)
                        {
                           artists += artist;
                           if (spotDlMetaData.artists.Last() != artist)
                              artists += ", ";
                        }

                        discordEmbedBuilder.WithAuthor(artists);
                     }
                     else
                        discordEmbedBuilder.WithAuthor(spotDlMetaData.artist);

                     string genres = "";
                     if (spotDlMetaData.genres.Count > 0)
                     {
                        foreach (string genre in spotDlMetaData.genres)
                        {
                           genres += genre;
                           if (spotDlMetaData.genres.Last() != genre)
                              genres += ", ";

                           //maybe too mutch genres for discordField
                        }
                     }
                     else
                        genres = "N/A";

                     discordEmbedBuilder.AddField(new DiscordEmbedField("Album", spotDlMetaData.album_name, true));
                     discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));
                     discordEmbedBuilder.WithUrl(spotDlMetaData.download_url);

                     HttpClient httpClientForBitmap = new();
                     if (spotDlMetaData.cover_url != "")
                     {
                        try
                        {
                           discordEmbedBuilder.WithThumbnail(spotDlMetaData.cover_url);
                           Stream streamForBitmap = await httpClientForBitmap.GetStreamAsync(spotDlMetaData.cover_url);

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

                     #endregion

                     spotDlTimeSpan = TimeSpan.FromSeconds(spotDlMetaData.duration);
                  }
               }
               else if (audioDownload.ErrorOutput.Length <= 1 && queueItemObj.IsYouTubeUri)
               {
                  Query musicBrainzQuery = new();
                  string[] fingerPrintDuration = default(string[]);
                  string[] fingerPrintFingerprint = default(string[]);
                  ProcessStartInfo fingerPrintCalculationProcessStartInfo = new()
                  {
                     FileName = "..\\..\\..\\fpcalc\\fpcalc.exe",
                     Arguments = $@" ""{audioDownload.Data}""",
                     RedirectStandardOutput = true,
                     UseShellExecute = false
                  };
                  Process fingerPrintCalculationProcess = Process.Start(fingerPrintCalculationProcessStartInfo);
                  if (fingerPrintCalculationProcess != null)
                  {
                     string fingerPrintCalculationOutput = await fingerPrintCalculationProcess.StandardOutput.ReadToEndAsync();
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
                     string httpClientContent = await httpClient.GetStringAsync(url);
                     acoustId = AcoustId.CreateObj(httpClientContent);
                  }

                  if (acoustId.Results != null && acoustId.Results.Count != 0 && acoustId.Results[0].Recordings[0] != null && acoustId.Results[0].Recordings[0].Releases != null)
                  {
                     try
                     {
                        string recordingMbId = acoustId.Results[0].Recordings[0].Id;
                        string genres = "N/A";

                        DateTime compareDateTimeOne = new();
                        AcoustId.Release rightAlbum = new();
                        AcoustId.Artist rightArtist = new();
                        if (acoustId.Results[0].Recordings[0].Artists != null)
                           rightArtist = acoustId.Results[0].Recordings[0].Artists[0];

                        foreach (AcoustId.Release compareItem in acoustId.Results[0].Recordings[0].Releases)
                        {
                           if (acoustId.Results[0].Recordings[0].Releases.Count == 1)
                           {
                              rightAlbum = compareItem;
                              break;
                           }

                           if (compareItem.Date == null || compareItem.Date.Year == 0 || compareItem.Date.Month == 0 || compareItem.Date.Day == 0)
                              continue;

                           if (compareDateTimeOne.Equals(new DateTime()))
                              compareDateTimeOne = new(compareItem.Date.Year, compareItem.Date.Month, compareItem.Date.Day);

                           DateTime compareDateTimeTwo = new(compareItem.Date.Year, compareItem.Date.Month, compareItem.Date.Day);
                           if (compareDateTimeOne < compareDateTimeTwo)
                           {
                              rightAlbum = compareItem;
                              compareDateTimeOne = compareDateTimeTwo;
                           }
                        }
                        //dogShit
                        //rightAlbum = acoustId.Results[0].Recordings[0].Releases[0];

                        musicBrainzTags = await musicBrainzQuery.LookupRecordingAsync(new Guid(recordingMbId));

                        if (musicBrainzTags.Genres != null)
                           genres = musicBrainzTags.Genres.ToString();

                        #region discordEmbedBuilder
                        discordEmbedBuilder.Title = musicBrainzTags.Title;
                        if (rightArtist != null)
                           discordEmbedBuilder.WithAuthor(rightArtist.Name);

                        discordEmbedBuilder.AddField(new DiscordEmbedField("Album", rightAlbum.Title, true));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));
                        discordEmbedBuilder.WithUrl(youtubeUri);

                        HttpClient httpClientForBitmap = new();
                        if (rightAlbum.Id != null)
                        {
                           try
                           {
                              discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release/{rightAlbum.Id}/front");
                              Stream streamForBitmap = await httpClientForBitmap.GetStreamAsync($"https://coverartarchive.org/release/{rightAlbum.Id}/front");

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
                        #endregion

                        wyldFunctionSuccess = true;
                     }
                     catch
                     {
                        //ignore
                     }
                  }
               }
               else if (audioDownload.ErrorOutput.Length > 1)
               {
                  audioDownloadError = $"{audioDownload.ErrorOutput[1]} `{youtubeUri.AbsoluteUri}`";
               }

               DiscordMessage discordMessage = interactionContext != null ? await interactionContext.Channel.SendMessageAsync("Loading!") : await interactionChannel.SendMessageAsync("Loading!");

               DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
               DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
               DiscordComponent[] discordComponents = new DiscordComponent[2];
               discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_yt", "Next!", false, discordComponentEmojisNext);
               discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_yt", "Stop!", false, discordComponentEmojisStop);

               if (queueItemObj.IsYouTubeUri)
                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUri.AbsoluteUri).AddEmbed(discordEmbedBuilder.Build()));
               else if (!queueItemObj.IsYouTubeUri)
                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(queueItemObj.SpotifyUri.AbsoluteUri).AddEmbed(discordEmbedBuilder.Build()));
               else if (wyldFunctionSuccess)
                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUri.AbsoluteUri).AddEmbed(discordEmbedBuilder.Build()));
               else
                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(audioDownloadError));

               ProcessStartInfo processStartInfo = new()
               {
                  FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/ffmpeg" : "..\\..\\..\\ffmpeg\\ffmpeg.exe",
                  Arguments = $@"-i ""{audioDownload.Data}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                  RedirectStandardOutput = true,
                  UseShellExecute = false
               };
               Process ffmpegProcess = Process.Start(processStartInfo);
               if (ffmpegProcess != null)
               {
                  Stream ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;

                  VoiceTransmitSink voiceTransmitSink = voiceNextConnection.GetTransmitSink();
                  voiceTransmitSink.VolumeModifier = 0.2;

                  Task ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink);

                  int counter = 0;
                  TimeSpan timeSpan = new(0, 0, 0, 0);
                  string playerAdvance = "";
                  while (!ffmpegCopyTask.IsCompleted)
                  {
                     if (wyldFunctionSuccess && audioDownloadTimeSpan.TotalSeconds > 0 && queueItemObj.IsYouTubeUri)
                     {
                        #region TimeLineAlgo
                        if (counter % 10 == 0)
                        {
                           timeSpan = TimeSpan.FromSeconds(counter);

                           string[] strings = new string[15];
                           double thisIsOneHundredPercent = audioDownloadTimeSpan.TotalSeconds;

                           double dotPositionInPercent = 100.0 / thisIsOneHundredPercent * counter;

                           double dotPositionInInt = 15.0 / 100.0 * dotPositionInPercent;

                           for (int i = 0; i < strings.Length; i++)
                           {
                              if (Convert.ToInt32(dotPositionInInt) == i)
                                 strings[i] = "🔘";
                              else
                                 strings[i] = "▬";
                           }

                           playerAdvance = "";
                           foreach (string item in strings)
                           {
                              playerAdvance += item;
                           }

                           string descriptionString = "⏹️";
                           if (cancellationToken.IsCancellationRequested)
                              descriptionString = "▶️";

                           descriptionString += $" {playerAdvance} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{audioDownloadTimeSpan.Hours:#00}:{audioDownloadTimeSpan.Minutes:#00}:{audioDownloadTimeSpan.Seconds:#00}] 🔉";
                           discordEmbedBuilder.Description = descriptionString;
                           await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUri.AbsoluteUri).WithEmbed(discordEmbedBuilder.Build()));
                        }
                        #endregion
                     }
                     else if (!queueItemObj.IsYouTubeUri)
                     {
                        #region TimeLineAlgo
                        if (counter % 10 == 0)
                        {
                           timeSpan = TimeSpan.FromSeconds(counter);
                           string[] strings = new string[15];
                           double thisIsOneHundredPercent = spotDlTimeSpan.TotalSeconds;

                           double dotPositionInPercent = 100.0 / thisIsOneHundredPercent * counter;

                           double dotPositionInInt = 15.0 / 100.0 * dotPositionInPercent;

                           for (int i = 0; i < strings.Length; i++)
                           {
                              if (Convert.ToInt32(dotPositionInInt) == i)
                                 strings[i] = "🔘";
                              else
                                 strings[i] = "▬";
                           }

                           playerAdvance = "";
                           foreach (string item in strings)
                           {
                              playerAdvance += item;
                           }

                           string descriptionString = "⏹️";
                           if (cancellationToken.IsCancellationRequested)
                              descriptionString = "▶️";

                           descriptionString += $" {playerAdvance} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{spotDlTimeSpan.Hours:#00}:{spotDlTimeSpan.Minutes:#00}:{spotDlTimeSpan.Seconds:#00}] 🔉";
                           discordEmbedBuilder.Description = descriptionString;
                           await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(queueItemObj.SpotifyUri.AbsoluteUri).AddEmbed(discordEmbedBuilder.Build()));
                        }
                        #endregion
                     }

                     if (cancellationToken.IsCancellationRequested)
                     {
                        ffmpegStream.Close();
                        break;
                     }
                     counter++;
                     await Task.Delay(1000);
                  }

                  if (wyldFunctionSuccess && audioDownloadTimeSpan.TotalSeconds > 0 && queueItemObj.IsYouTubeUri)
                  {
                     #region MoteTimeLineAlgo
                     string durationString = $"{audioDownloadTimeSpan.Hours:#00}:{audioDownloadTimeSpan.Minutes:#00}:{audioDownloadTimeSpan.Seconds:#00}";

                     if (!cancellationToken.IsCancellationRequested)
                        discordEmbedBuilder.Description = $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
                     else
                     {
                        string descriptionString = "⏹️";
                        if (cancellationToken.IsCancellationRequested)
                           descriptionString = "▶️";

                        descriptionString += $" {playerAdvance} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{audioDownloadTimeSpan.Hours:#00}:{audioDownloadTimeSpan.Minutes:#00}:{audioDownloadTimeSpan.Seconds:#00}] 🔉";
                        discordEmbedBuilder.Description = descriptionString;
                     }
                     #endregion
                     discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_yt", "Skipped!", true, discordComponentEmojisNext);
                     discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_yt", "Stop!", true, discordComponentEmojisStop);

                     await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUri.AbsoluteUri).WithEmbed(discordEmbedBuilder.Build()));
                  }
                  else if (!queueItemObj.IsYouTubeUri)
                  {
                     #region MoteTimeLineAlgo
                     string durationString = $"{spotDlTimeSpan.Hours:#00}:{spotDlTimeSpan.Minutes:#00}:{spotDlTimeSpan.Seconds:#00}";

                     if (!cancellationToken.IsCancellationRequested)
                        discordEmbedBuilder.Description = $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
                     else
                     {
                        string descriptionString = "⏹️";
                        if (cancellationToken.IsCancellationRequested)
                           descriptionString = "▶️";

                        descriptionString += $" {playerAdvance} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{spotDlTimeSpan.Hours:#00}:{spotDlTimeSpan.Minutes:#00}:{spotDlTimeSpan.Seconds:#00}] 🔉";
                        discordEmbedBuilder.Description = descriptionString;
                     }
                     #endregion

                     discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_yt", "Skipped!", true, discordComponentEmojisNext);
                     discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_yt", "Stop!", true, discordComponentEmojisStop);

                     await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(queueItemObj.SpotifyUri.AbsoluteUri).AddEmbed(discordEmbedBuilder.Build()));
                  }
                  else
                  {
                     discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_yt", "Skipped!", true, discordComponentEmojisNext);
                     discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_yt", "Stop!", true, discordComponentEmojisStop);

                     await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUri.AbsoluteUri));
                  }

                  if (!cancellationToken.IsCancellationRequested)
                  {
                     foreach (CancellationTokenItem tokenKeyPair in CancellationTokenItemList.Where(x => x.DiscordGuild == (interactionContext != null ? interactionContext.Guild : discordGuild)))
                     {
                        CancellationTokenItemList.Remove(tokenKeyPair);
                        break;
                     }
                  }

                  await voiceTransmitSink.FlushAsync();
               }
            }
            catch
            {
               // ignored
            }


            if (!cancellationToken.IsCancellationRequested)
            {
               if (QueueItemList.All(x => x.DiscordGuild != (interactionContext != null ? interactionContext.Guild : discordGuild)))
               {
                  //Queue ist empty
               }

               foreach (QueueItem queueKeyPairItem in QueueItemList)
               {
                  if (queueKeyPairItem.DiscordGuild == (interactionContext != null ? interactionContext.Guild : discordGuild))
                  {
                     CancellationTokenSource tokenSource = new();
                     CancellationToken newCancellationToken = tokenSource.Token;
                     CancellationTokenItem cancellationTokenKeyPair = new(interactionContext != null ? interactionContext.Guild : discordGuild, tokenSource);
                     CancellationTokenItemList.Add(cancellationTokenKeyPair);
                     if (interactionContext != null)
                        Task.Run(() => PlayFromQueueAsyncTask(interactionContext, interactionContext.Client, interactionContext.Guild, interactionContext.Client.CurrentUser.ConvertToMember(interactionContext.Guild).Result,
                            interactionContext.Channel, queueKeyPairItem.YouTubeUri, newCancellationToken, false));
                     else
                        Task.Run(() => PlayFromQueueAsyncTask(interactionContext, client, discordGuild, discordMember, interactionChannel, queueKeyPairItem.YouTubeUri, newCancellationToken, false));
                     break;
                  }
               }
            }
         }
         catch (Exception exc)
         {
            if (interactionContext != null)
               interactionContext.Client.Logger.LogError(exc.Message);
            else
               client.Logger.LogError(exc.Message);
         }
      }

      [SlashCommand("Stop", "Stop the music!")]
      private async Task StopCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Loading!"));

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

      [SlashCommand("Skip", "Skip this song!")]
      private async Task SkipCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Loading!"));
         await PlayMusic.NextSongTask(interactionContext);
      }

      [SlashCommand("Next", "Skip this song!")]
      private async Task NextCommand(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Loading!"));
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

                  CancellationTokenSource tokenSource = null;
                  foreach (CancellationTokenItem keyValuePairItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     tokenSource = keyValuePairItem.CancellationTokenSource;
                     CancellationTokenItemList.Remove(keyValuePairItem);
                     break;
                  }

                  QueueItemList.Clear();

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