using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.VoiceNext;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using SchattenclownBot.Model.Discord.AppCommands.Music.Objects;
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
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using File = TagLib.File;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
   internal class Main
   {
      internal static readonly List<DcCancellationTokenItem> CancellationTokenItemList = new();
      internal static readonly List<QueueTrack> QueueTracks = new();

      internal static Task PlayFromQueueTask(Gmc gMc, QueueTrack queueTrack)
      {
         CancellationTokenSource tokenSource = new();
         CancellationToken cancellationToken = tokenSource.Token;
         DcCancellationTokenItem dcCancellationTokenKeyPair = new(gMc.DiscordGuild, tokenSource, false);
         CancellationTokenItemList.Add(dcCancellationTokenKeyPair);

         try
         {
            Task.Run(() => PlayFromQueueAsyncTask(gMc, queueTrack, cancellationToken));
         }
         catch
         {
            CancellationTokenItemList.Remove(dcCancellationTokenKeyPair);
            QueueTracks.Remove(queueTrack);
         }

         return Task.CompletedTask;
      }

      internal static async Task PlayFromQueueAsyncTask(Gmc gMc, QueueTrack queueTrack, CancellationToken cancellationToken)
      {
         bool isRepeat = false;
         VoiceNextExtension voiceNext = Bot.DiscordClient.GetVoiceNext();
         if (voiceNext == null)
         {
            CwLogger.Write($"On server {gMc.DiscordGuild.Name} VoiceNext was NULL", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
            return;
         }

         VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(gMc.DiscordGuild);
         DiscordVoiceState voiceState = gMc.DiscordMember?.VoiceState;
         if (voiceState?.Channel == null)
         {
            CwLogger.Write($"On server {gMc.DiscordGuild.Name} voiceState was NULL", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
            return;
         }

         voiceNextConnection ??= await voiceNext.ConnectAsync(voiceState.Channel);

         VoiceTransmitSink voiceTransmitSink = default;

         try
         {
            QueueTracks.Find(x => x == queueTrack)!.HasBeenPlayed = true;

            DiscordComponentEmoji discordComponentEmojisPrevious = new("⏮️");
            DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
            DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
            DiscordComponentEmoji discordComponentEmojisShuffle = new("🔀");
            DiscordComponentEmoji discordComponentEmojisQueue = new("⏬");
            DiscordComponentEmoji discordComponentEmojisRepeat = new("🔂");
            DiscordComponent[] discordComponent = new DiscordComponent[5];
            discordComponent[0] = new DiscordButtonComponent(ButtonStyle.Primary, "PreviousTrackStream", "Back!", false, discordComponentEmojisPrevious);
            discordComponent[1] = new DiscordButtonComponent(ButtonStyle.Primary, "NextTrackStream", "Next!", false, discordComponentEmojisNext);
            //discordComponent[2] = new DiscordButtonComponent(ButtonStyle.Danger, "StopTrackStream", "Stop!", false, discordComponentEmojisStop);
            discordComponent[2] = new DiscordButtonComponent(ButtonStyle.Success, "ShuffleStream", "Shuffle!", false, discordComponentEmojisShuffle);
            discordComponent[3] = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowQueueStream", "Show queue!", false, discordComponentEmojisQueue);

            if (CancellationTokenItemList.First(x => x.DiscordGuild == gMc.DiscordGuild).IsRepeat)
            {
               discordComponent[4] = new DiscordButtonComponent(ButtonStyle.Success, "IsRepeat", "Turn repeat off!", false, discordComponentEmojisRepeat);
            }
            else
            {
               discordComponent[4] = new DiscordButtonComponent(ButtonStyle.Danger, "IsRepeat", "Turn repeat on!", false, discordComponentEmojisRepeat);
            }

            DiscordEmbedBuilder discordEmbedBuilder = new();
            RunResult<string> audioDownload = default;
            TimeSpan audioDownloadTimeSpan = default;
            bool fileExists = false;

            if (queueTrack.LocalPath == null)
            {
               Uri networkDriveUri = new(@"M:\Music\");
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

               try
               {
                  if (queueTrack.SpotifyUri != null)
                  {
                     fileExists = System.IO.File.Exists(@$"M:\Spotify2mp3\{queueTrack.FullTrack.ExternalIds.First().Value}-{queueTrack.FullTrack.Id}.mp3");
                  }
               }
               catch
               {
                  //ignore
               }

               if (!fileExists)
               {
                  audioDownload = await youtubeDl.RunAudioDownload(queueTrack.YouTubeUri.AbsoluteUri, AudioConversionFormat.Mp3, new CancellationToken(), null, null, optionSet);
                  //var some = await youtubeDl.RunVideoDataFetch(queueTrack.YouTubeUri.AbsoluteUri, cancellationToken);
                  //audioDownloadMetaData = youtubeDl.RunVideoDataFetch(queueTrack.YouTubeUri.AbsoluteUri).Result.Data;

                  // Create a TagLib File object from the MP3 file
                  File tagFile = File.Create(audioDownload.Data);

                  // Get the length property of the file
                  audioDownloadTimeSpan = tagFile.Properties.Duration;
               }

               //<a:twitch:1050340762459586560>
               //<:spotify:1050436741393297470>
               //<:youtube:1050436748578136184>
               //<:MusicBrainz:1050439464452894720>

               if (queueTrack.SpotifyUri == null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("YouTube", $"[[<:youtube:1050436748578136184> Open]({queueTrack.YouTubeUri.AbsoluteUri})]", true));
               }
               else
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("Spotify", $"[[<:spotify:1050436741393297470> Open]({queueTrack.SpotifyUri.AbsoluteUri})]", true));

                  if (!fileExists)
                  {
                     discordEmbedBuilder.AddField(new DiscordEmbedField("YouTube", $"[[<:youtube:1050436748578136184> Open]({queueTrack.YouTubeUri.AbsoluteUri})]", true));
                  }
                  else
                  {
                     // Create a TagLib File object from the MP3 file
                     File tagFile = File.Create(@$"M:\Spotify2mp3\{queueTrack.FullTrack.ExternalIds.First().Value}-{queueTrack.FullTrack.Id}.mp3");

                     // Get the length property of the file
                     audioDownloadTimeSpan = tagFile.Properties.Duration;
                  }
               }
            }
            else
            {
               // Create a TagLib File object from the MP3 file
               File tagFile = File.Create(@$"{queueTrack.LocalPath}");

               // Get the length property of the file
               audioDownloadTimeSpan = tagFile.Properties.Duration;
            }

            if (queueTrack.LocalPath == null && audioDownload?.Success == false)
            {
               await Bot.DebugDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(audioDownload.ErrorOutput.Aggregate("", (current, errors) => current + errors + "\n\n" + queueTrack.YouTubeUri.AbsoluteUri))));
               await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Something went wrong!\n")));
            }
            else
            {
               string spotifyFilePath = "";

               if (queueTrack.LocalPath == null && !fileExists)
               {
                  if (audioDownload != null)
                  {
                     spotifyFilePath = audioDownload.Data;
                  }
               }
               else if (queueTrack.LocalPath == null)
               {
                  spotifyFilePath = @$"M:\Spotify2mp3\{queueTrack.FullTrack.ExternalIds.First().Value}-{queueTrack.FullTrack.Id}.mp3";
               }
               else
               {
                  spotifyFilePath = queueTrack.LocalPath;
               }

               await Bot.DebugDiscordChannel.SendMessageAsync(@$"{spotifyFilePath}");
               discordEmbedBuilder = CustomDiscordEmbedBuilder(discordEmbedBuilder, queueTrack, new Uri(@$"{spotifyFilePath}"), null, null);
               DiscordMessage discordMessage = await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponent).AddEmbed(discordEmbedBuilder.Build()));

               ProcessStartInfo ffmpegProcessStartInfo = new()
               {
                  FileName = "..\\..\\..\\Model\\Executables\\ffmpeg\\ffmpeg.exe",
                  Arguments = $@"-i ""{spotifyFilePath}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                  RedirectStandardOutput = true,
                  UseShellExecute = false
               };

               Process ffmpegProcess = Process.Start(ffmpegProcessStartInfo);
               if (ffmpegProcess != null)
               {
                  voiceTransmitSink = voiceNextConnection.GetTransmitSink();
                  voiceTransmitSink.VolumeModifier = 0.2;
                  Stream ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;
                  //maybe problem
                  Task ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink, cancellationToken: cancellationToken);

                  CwLogger.Write($"Playing {queueTrack.Title} - YT:{queueTrack.YouTubeUri} | SY:{queueTrack.SpotifyUri} ON {gMc.DiscordGuild.Name}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__3", "").Replace("<", ""), ConsoleColor.Yellow);

                  int timeSpanAdvanceInt = 0;
                  int runAsyncInt = 0;
                  while (!ffmpegCopyTask.IsCompleted)
                  {
                     //maybe problem
                     await Task.Delay(500, cancellationToken);

                     try
                     {
                        if (timeSpanAdvanceInt % 1 == 0)
                        {
                           discordEmbedBuilder.Description = TimeLineStringBuilderWhilePlaying(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);

                           if (CancellationTokenItemList.First(x => x.DiscordGuild == gMc.DiscordGuild).IsRepeat)
                           {
                              discordComponent[4] = new DiscordButtonComponent(ButtonStyle.Success, "IsRepeat", "Turn repeat off!", false, discordComponentEmojisRepeat);
                           }
                           else
                           {
                              discordComponent[4] = new DiscordButtonComponent(ButtonStyle.Danger, "IsRepeat", "Turn repeat on!", false, discordComponentEmojisRepeat);
                           }

                           await discordMessage.ModifyAsync(x => x.AddComponents(discordComponent).AddEmbed(discordEmbedBuilder.Build()));
                        }
                     }
                     catch (Exception ex)
                     {
                        CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__12", "").Replace("<", ""), ConsoleColor.Red);
                     }

                     if (voiceState.Channel.Users.All(x => x.Id != Bot.DiscordClient.CurrentUser.Id))
                     {
                        ffmpegStream.Close();
                        await StopMusicTask(gMc, true);
                        break;
                     }

                     if (cancellationToken.IsCancellationRequested)
                     {
                        ffmpegStream.Close();
                        break;
                     }

                     if (runAsyncInt % 10 == 0)
                     {
                        CwLogger.Write($"Playing Artist: '{queueTrack.Artist}' Title: '{queueTrack.Title}' on Guild: '{queueTrack.Gmc.DiscordGuild.Name}'", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.DarkYellow);
                     }

                     runAsyncInt++;
                     //maybe problem
                     await Task.Delay(500, cancellationToken);
                     timeSpanAdvanceInt++;
                  }

                  discordComponent[0] = new DiscordButtonComponent(ButtonStyle.Primary, "PreviousTrackStream", "Back!", true, discordComponentEmojisPrevious);
                  discordComponent[1] = new DiscordButtonComponent(ButtonStyle.Primary, "NextTrackStream", "Next!", true, discordComponentEmojisNext);
                  //discordComponent[2] = new DiscordButtonComponent(ButtonStyle.Danger, "StopTrackStream", "Stop!", true, discordComponentEmojisStop);
                  discordComponent[2] = new DiscordButtonComponent(ButtonStyle.Success, "ShuffleStream", "Shuffle!", true, discordComponentEmojisShuffle);
                  discordComponent[3] = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowQueueStream", "Show queue!", true, discordComponentEmojisQueue);

                  if (CancellationTokenItemList.First(x => x.DiscordGuild == gMc.DiscordGuild).IsRepeat)
                  {
                     discordComponent[4] = new DiscordButtonComponent(ButtonStyle.Success, "IsRepeat", "Turn repeat off!", true, discordComponentEmojisRepeat);
                  }
                  else
                  {
                     discordComponent[4] = new DiscordButtonComponent(ButtonStyle.Danger, "IsRepeat", "Turn repeat on!", true, discordComponentEmojisRepeat);
                  }

                  discordEmbedBuilder.Description = TimeLineStringBuilderAfterTrack(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);
                  await discordMessage.ModifyAsync(x => x.AddComponents(discordComponent).AddEmbed(discordEmbedBuilder.Build()));
               }

               if (CancellationTokenItemList.First(x => x.DiscordGuild == gMc.DiscordGuild).IsRepeat)
               {
                  isRepeat = true;
                  QueueTracks.Find(x => x == queueTrack)!.HasBeenPlayed = false;
               }

               if (!cancellationToken.IsCancellationRequested)
               {
                  CancellationTokenItemList.RemoveAll(x => x.CancellationTokenSource.Token == cancellationToken && x.DiscordGuild == gMc.DiscordGuild);
               }
            }
         }
         catch (Exception ex)
         {
            await Bot.DebugDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(ex.ToString())));
            await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Something went wrong!\n")));
         }
         finally
         {
            if (voiceTransmitSink != null)
            {
               //maybe problem
               await voiceTransmitSink.FlushAsync();
            }

            if (!cancellationToken.IsCancellationRequested)
            {
               List<QueueTrack> queueTracksTemp = QueueTracks.FindAll(x => x.Gmc.DiscordGuild == gMc.DiscordGuild);

               if (queueTracksTemp.All(x => x.HasBeenPlayed))
               {
                  await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Queue is empty!")));
                  QueueTracks.RemoveAll(x => x.Gmc.DiscordGuild == gMc.DiscordGuild);

                  List<CancellationTokenSource> cancellationTokenSourceList = new();

                  foreach (DcCancellationTokenItem item in CancellationTokenItemList.Where(x => x.DiscordGuild == gMc.DiscordGuild))
                  {
                     cancellationTokenSourceList.Add(item.CancellationTokenSource);
                  }

                  CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == gMc.DiscordGuild);

                  foreach (CancellationTokenSource item in cancellationTokenSourceList)
                  {
                     item.Cancel();
                     item.Dispose();
                  }

                  voiceNextConnection.Disconnect();
               }

               foreach (QueueTrack queueTrackItem in QueueTracks)
               {
                  if (queueTrackItem.Gmc.DiscordGuild == gMc.DiscordGuild && queueTrackItem.HasBeenPlayed == false)
                  {
                     CancellationTokenSource cancellationTokenSource = new();
                     CancellationToken token = cancellationTokenSource.Token;
                     CancellationTokenItemList.Add(new DcCancellationTokenItem(gMc.DiscordGuild, cancellationTokenSource, isRepeat));
                     gMc.DiscordMember = Bot.DiscordClient.CurrentUser.ConvertToMember(gMc.DiscordGuild).Result;
                     _ = Task.Run(() => PlayFromQueueAsyncTask(gMc, queueTrackItem, token));
                     break;
                  }
               }
            }
         }
      }

      internal static void PlayNextTrackFromQueue(Gmc gMc)
      {
         if (CancellationTokenItemList.First(x => x.DiscordGuild == gMc.DiscordGuild).IsRepeat)
         {
            gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Repeat is on, turn it off to skip Songs!")));
            return;
         }

         IEnumerable<QueueTrack> queueTracksTemp = QueueTracks.Where(x => x.Gmc.DiscordGuild == gMc.DiscordGuild);

         if (queueTracksTemp.All(x => x.HasBeenPlayed))
         {
            gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Nothing to skip!")));
            return;
         }

         List<CancellationTokenSource> cancellationTokenSourceList = new();
         foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == gMc.DiscordGuild))
         {
            cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
         }

         CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == gMc.DiscordGuild);

         foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
         {
            cancellationToken.Cancel();
            cancellationToken.Dispose();
         }

         CancellationTokenSource newCancellationTokenSource = new();
         CancellationToken newCancellationToken = newCancellationTokenSource.Token;

         foreach (QueueTrack queueTrack in QueueTracks.Where(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.HasBeenPlayed))
         {
            DcCancellationTokenItem newDcCancellationTokenItem = new(gMc.DiscordGuild, newCancellationTokenSource, false);
            CancellationTokenItemList.Add(newDcCancellationTokenItem);

            Task.Run(() => PlayFromQueueAsyncTask(gMc, queueTrack, newCancellationToken));
            break;
         }
      }

      internal static void PlayPreviousTrackFromQueue(Gmc gMc)
      {
         if (QueueTracks.All(x => x.Gmc.DiscordGuild != gMc.DiscordGuild))
         {
            gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Queue empty!")));
            return;
         }

         QueueTrack item = QueueTracks.First(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.HasBeenPlayed);

         int indexOfPlaying = QueueTracks.FindIndex(x => x.Gmc.DiscordGuild == item.Gmc.DiscordGuild && ((x.SpotifyUri == item.SpotifyUri && x.SpotifyUri != null && item.SpotifyUri != null) || (x.YouTubeUri == item.YouTubeUri && x.YouTubeUri != null && item.YouTubeUri != null) || x.LocalPath == item.LocalPath)) - 1;
         int indexOfLast = QueueTracks.FindIndex(x => x.Gmc.DiscordGuild == item.Gmc.DiscordGuild && ((x.SpotifyUri == item.SpotifyUri && x.SpotifyUri != null && item.SpotifyUri != null) || (x.YouTubeUri == item.YouTubeUri && x.YouTubeUri != null && item.YouTubeUri != null) || x.LocalPath == item.LocalPath)) - 2;
         if (indexOfLast == -1)
         {
            gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Wrong universe!")));
            return;
         }

         QueueTrack playingQueueTrack = QueueTracks[indexOfPlaying];
         playingQueueTrack.HasBeenPlayed = false;
         QueueTrack lastQueueTrack = QueueTracks[indexOfLast];
         lastQueueTrack.HasBeenPlayed = false;

         List<CancellationTokenSource> cancellationTokenSourceList = new();
         foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == gMc.DiscordGuild))
         {
            cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
         }

         CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == gMc.DiscordGuild);

         foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
         {
            cancellationToken.Cancel();
            cancellationToken.Dispose();
         }

         CancellationTokenSource newCancellationTokenSource = new();
         CancellationToken newCancellationToken = newCancellationTokenSource.Token;

         foreach (QueueTrack queueTrack in QueueTracks.Where(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.HasBeenPlayed))
         {
            DcCancellationTokenItem newDcCancellationTokenItem = new(gMc.DiscordGuild, newCancellationTokenSource, false);
            CancellationTokenItemList.Add(newDcCancellationTokenItem);

            Task.Run(() => PlayFromQueueAsyncTask(gMc, queueTrack, newCancellationToken));
            break;
         }
      }

      internal static List<PlaylistVideo> ShufflePlayListForYouTube(List<PlaylistVideo> playlistVideos)
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

      internal static List<FullTrack> ShufflePlayListForSpotify(List<FullTrack> fullTracks)
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

      internal static async Task ShuffleQueueTracksAsyncTask(Gmc gMc, DiscordMessage discordMessage)
      {
         if (QueueTracks.Any(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.IsAdded))
         {
            int queueItemsInt = QueueTracks.Count(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && x.IsAdded);
            int queueItemsNotAddedInt = QueueTracks.Count(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.HasBeenPlayed);
            await discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Queue is being created! " + $"{queueItemsInt}/" + $"{queueItemsNotAddedInt} Please wait!")));
         }
         else
         {
            List<QueueTrack> queueTracksMixed = new(ShuffleQueueTracks(QueueTracks.FindAll(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.HasBeenPlayed)));

            QueueTracks.RemoveAll(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.HasBeenPlayed);

            foreach (QueueTrack queueTrack in queueTracksMixed)
            {
               QueueTracks.Add(queueTrack);
            }

            if (queueTracksMixed.Count == 0)
            {
               await discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Its late i have to leave!")));
            }
            else
            {
               if (discordMessage != null)
               {
                  await discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription("Queue has been altered!")));
               }
               else
               {
                  await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription("Queue has been altered!")));
               }
            }
         }
      }

      internal static List<QueueTrack> ShuffleQueueTracks(List<QueueTrack> queueTrack)
      {
         List<QueueTrack> queueTrackMixed = new();

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

      internal static async Task AddTracksToQueueAsyncTask(ulong requestDiscordUserId, string webLink, bool isShufflePlay)
      {
         Gmc gMc = Gmc.FromDiscordUserId(requestDiscordUserId);
         if (gMc == null)
         {
            gMc = Gmc.MemberFromId(requestDiscordUserId);
            await gMc.DiscordMember.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return;
         }

         if (QueueTracks.Any(x => x.Gmc.DiscordChannel == gMc.DiscordChannel && !x.IsAdded))
         {
            int addedCount = QueueTracks.Count(x => x.Gmc.DiscordChannel == gMc.DiscordChannel && x.IsAdded);
            int notAddedCount = QueueTracks.Count(x => x.Gmc.DiscordChannel == gMc.DiscordChannel && !x.IsAdded);
            await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("The queue is already being generated. Please wait until the first queue is generated! " + $"{addedCount}/ " + $"{notAddedCount} Please wait!")));
            return;
         }

         if (!NoMusicPlaying(gMc.DiscordGuild))
         {
            await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Queue is being created! Please be patient!")));
         }

         Uri webLinkUri;
         try
         {
            webLinkUri = new Uri(webLink);
         }
         catch
         {
            await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Please check your link, something is wrong! The https:// tag may be missing")));
            return;
         }

         List<FullTrack> fullTracks = new();
         int tracksAdded = 0;

         DeterminedStreamingService determinedStreamingService = new DeterminedStreamingService().IdentifyStreamingService(webLink);
         if (determinedStreamingService.Nothing)
         {
            await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Sag ICH!!!")));
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
                     QueueTracks.Add(new QueueTrack(gMc, new Uri(item.Url), item.Title, item.Author.ChannelTitle));
                     tracksAdded++;
                  }
               }
               else
               {
                  string selectedVideoId = StringCutter.RmAfter(StringCutter.RmAfter(webLink.Contains("youtu.be") ? StringCutter.RmUntil(webLink, "youtu.be/", "youtu.be/".Length) : StringCutter.RmUntil(webLink, "watch?v=", "watch?v=".Length), "&list=", 0), "&t=", 0);

                  Video videoData = await youtubeClient.Videos.GetAsync("https://www.youtube.com/watch?v=" + selectedVideoId);

                  QueueTracks.Add(new QueueTrack(gMc, new Uri(videoData.Url), videoData.Title, videoData.Author.ChannelTitle));
                  tracksAdded++;
               }
            }
            catch (Exception ex)
            {
               await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error!\n" + ex.Message)));
            }

            if (NoMusicPlaying(gMc.DiscordGuild))
            {
               if (tracksAdded == 1)
               {
                  await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"{tracksAdded} track is now added to the queue!")));
               }
               else
               {
                  await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"{tracksAdded} tracks are now added to the queue!")));
               }
            }
            else
            {
               if (tracksAdded == 1)
               {
                  await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Music is already playing or will at any moment! {tracksAdded} track is now added to the queue!")));
               }
               else
               {
                  await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Music is already playing or will at any moment! {tracksAdded} tracks are now added to the queue!")));
               }
            }
         }
         else if (determinedStreamingService.IsSpotify)
         {
            SpotifyClient spotifyClient = GetSpotifyClientConfig();

            if (determinedStreamingService.IsSpotifyPlaylist)
            {
               string playlistId = StringCutter.RmAfter(StringCutter.RmUntil(webLink, "/playlist/", "/playlist/".Length), "?si", 0);

               PlaylistGetItemsRequest playlistGetItemsRequest = new()
               {
                  Offset = 0
               };

               List<PlaylistTrack<IPlayableItem>> iPlayableItems = spotifyClient.Playlists.GetItems(playlistId, playlistGetItemsRequest).Result.Items;

               if (iPlayableItems is
                   {
                      Count: >= 100
                   })
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

               if (iPlayableItems != null)
               {
                  foreach (PlaylistTrack<IPlayableItem> iPlayableItem in iPlayableItems)
                  {
                     fullTracks.Add(iPlayableItem.Track as FullTrack);
                  }
               }
            }
            else if (determinedStreamingService.IsSpotifyAlbum)
            {
               string albumId = StringCutter.RmAfter(StringCutter.RmUntil(StringCutter.RmUntil(webLink, "/album/", "/album/".Length), ":album:", ":album:".Length), "?si", 0);
               Paging<SimpleTrack> simpleTracks = await spotifyClient.Albums.GetTracks(albumId);

               if (simpleTracks.Items != null)
               {
                  foreach (SimpleTrack simpleTrack in simpleTracks.Items)
                  {
                     FullTrack fullTrack = await spotifyClient.Tracks.Get(simpleTrack.Id);
                     fullTracks.Add(fullTrack);
                  }
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
               QueueTrack queueTrack = new(gMc, item);
               if (queueTrack.SpotifyUri != null)
               {
                  queueTracks.Add(queueTrack);
                  QueueTracks.Add(queueTrack);
               }
            }

            _ = Task.Run(async () =>
            {
               DiscordMessage discordMessage;
               if (fullTracks.Count == 1)
               {
                  discordMessage = await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription($"Generating queue for {fullTracks.Count} track!")));
               }
               else
               {
                  discordMessage = await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription($"Generating queue for {fullTracks.Count} tracks!")));
               }

               foreach (QueueTrack item in queueTracks)
               {
                  if (QueueTracks.All(x => x.Gmc.DiscordChannel != gMc.DiscordChannel))
                  {
                     await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Queue generation stopped!")));
                     return;
                  }

                  SpotifyQueueAddSearchAsync(item);
                  tracksAdded++;
                  //to much requests will result in timeout
                  await Task.Delay(150);
               }

               await discordMessage.DeleteAsync();
               if (NoMusicPlaying(gMc.DiscordGuild))
               {
                  if (tracksAdded == 1)
                  {
                     await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"{tracksAdded} track is now added to the queue!")));
                  }
                  else
                  {
                     await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"{tracksAdded} tracks are now added to the queue!")));
                  }
               }
               else
               {
                  if (tracksAdded == 1)
                  {
                     await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Music is already playing or will at any moment! {tracksAdded} track is now added to the queue!")));
                  }
                  else
                  {
                     await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Music is already playing or will at any moment! {tracksAdded} tracks are now added to the queue!")));
                  }
               }
            });

            _ = Task.Run(() =>
            {
               int counter = 0;

               foreach (QueueTrack item in queueTracks)
               {
                  FullTrack fullTrack = item.FullTrack;
                  counter++;

                  SpotifyTasks spotifyTasks = new()
                  {
                     DiscordUserId = gMc.DiscordMember.Id,
                     DiscordGuildId = gMc.DiscordGuild.Id,
                     DiscordChannelId = gMc.DiscordChannel.Id
                  };

                  if (fullTrack != null)
                  {
                     spotifyTasks.DurationInMs = fullTrack.DurationMs;
                     spotifyTasks.TrackId = fullTrack.Id;
                     spotifyTasks.ExternalId = fullTrack.ExternalIds.First().Value;
                     spotifyTasks.Title = fullTrack.Name;
                     spotifyTasks.Album = fullTrack.Album.Name;
                     spotifyTasks.AlbumArtist = fullTrack.Artists.FirstOrDefault()?.Name;

                     TimeSpan fullTrackDuration = TimeSpan.FromMilliseconds(fullTrack.DurationMs);
                     string fullTrackDurationString = fullTrackDuration.Hours == 0 ? $"{fullTrackDuration.Minutes:00}:{fullTrackDuration.Seconds:00}" : $"{fullTrackDuration.Hours:00}:{fullTrackDuration.Minutes:00}:{fullTrackDuration.Seconds:00}";

                     spotifyTasks.Comment = fullTrackDurationString;
                     spotifyTasks.Genre = "";
                     spotifyTasks.TrackNumber = fullTrack.TrackNumber;
                     spotifyTasks.Subtitle = "";

                     spotifyTasks.ReleaseYear = fullTrack.Album.ReleaseDate.Length > 4 ? Convert.ToDateTime(fullTrack.Album.ReleaseDate).Year.ToString() : fullTrack.Album.ReleaseDate;
                  }

                  SpotifyTasks.Insert(spotifyTasks);
                  CwLogger.Write($"{counter:00000} Added {spotifyTasks.TrackId}, {spotifyTasks.Title} to the database...", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.DarkCyan);
               }
            });
         }
         else if (determinedStreamingService.IsLocal)
         {
            Uri networkDriveUri = new(@"M:\FLAC\");
            List<string> allFiles = Directory.GetFiles(networkDriveUri.AbsolutePath).ToList();
            foreach (string file in allFiles)
            {
               try
               {
                  File metaTagFileToPlay = File.Create(@$"{file}");
                  if (metaTagFileToPlay.Tag.Title != null)
                  {
                     QueueTrack queueTrack = new(gMc, file, metaTagFileToPlay.Tag.Title, metaTagFileToPlay.Tag.Album);
                     QueueTracks.Add(queueTrack);
                  }
               }
               catch
               {
                  //ignore
               }
            }
         }

         if (NoMusicPlaying(gMc.DiscordGuild))
         {
            while (!QueueTracks.FirstOrDefault(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.HasBeenPlayed)!.IsAdded)
            {
               await Task.Delay(100);
            }

            await PlayFromQueueTask(gMc, QueueTracks.FirstOrDefault(x => x.Gmc.DiscordGuild == gMc.DiscordGuild && !x.HasBeenPlayed));
         }
      }

      internal static void SpotifyQueueAddSearchAsync(QueueTrack queueTrack)
      {
         Task.Run(() =>
         {
            QueueTrack editQueueTrack = QueueTracks.Find(x => x.FullTrack == queueTrack.FullTrack);

            try
            {
               if (editQueueTrack != null)
               {
                  bool fileExists = false;
                  try
                  {
                     if (queueTrack.SpotifyUri != null)
                     {
                        fileExists = System.IO.File.Exists(@$"M:\Spotify2mp3\{queueTrack.FullTrack.ExternalIds.First().Value}-{queueTrack.FullTrack.Id}.mp3");
                     }
                  }
                  catch
                  {
                     //ignore
                  }

                  if (!fileExists)
                  {
                     editQueueTrack.YouTubeUri = SearchYoutubeFromSpotify(queueTrack.FullTrack);
                  }

                  CwLogger.Write($"{queueTrack.Gmc.DiscordGuild.Name}   |   {queueTrack.Title} - {queueTrack.Artist}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace("<>c__DisplayClass11_0", "Queue Add"), ConsoleColor.DarkCyan);
                  editQueueTrack.IsAdded = true;
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

      internal static Uri SearchYoutubeFromSpotify(FullTrack fullTrack)
      {
         List<VideoResultFromYtSearch> results = new();
         YoutubeClient youtubeClient = new();

         string artist = fullTrack.Artists[0].Name;
         string trackName = fullTrack.Name;
         string externalIds = fullTrack.ExternalIds.Values.FirstOrDefault();
         int durationMs = fullTrack.DurationMs;
         List<SimpleArtist> artistsArray = fullTrack.Artists;

         IReadOnlyList<VideoSearchResult> resultsFromYt = youtubeClient.Search.GetVideosAsync($"{artist} {trackName}").CollectAsync(5).Result;

         foreach (VideoSearchResult item in resultsFromYt)
         {
            if (item.Duration != null)
            {
               results.Add(new VideoResultFromYtSearch(item, new TimeSpan(0), 0));
            }
         }

         resultsFromYt = youtubeClient.Search.GetVideosAsync($"{externalIds}").CollectAsync(5).Result;

         foreach (VideoSearchResult item in resultsFromYt)
         {
            if (item.Duration != null)
            {
               results.Add(new VideoResultFromYtSearch(item, new TimeSpan(0), 1));
            }
         }

         TimeSpan t1 = TimeSpan.FromMilliseconds(durationMs);
         bool artistInChannel = false;
         foreach (VideoResultFromYtSearch result in results)
         {
            string[] blacklist =
            {
               "instrumental",
               "bass boosted",
               "mix",
               "live",
               "reagiert"
            };

            foreach (string item in blacklist)
            {
               if ((!trackName.ToLower().Contains(item.ToLower()) && result.VideoSearchResult.Title.ToLower().Contains(item.ToLower())) || (trackName.ToLower().Contains(item.ToLower()) && !result.VideoSearchResult.Title.ToLower().Contains(item.ToLower())))
               {
                  result.Hits--;
               }
            }

            if (result.VideoSearchResult.Title.ToLower().Contains(trackName.ToLower()))
            {
               result.Hits++;
            }

            foreach (SimpleArtist item in artistsArray)
            {
               if (result.VideoSearchResult.Author.ChannelTitle.ToLower().Contains(item.Name.ToLower()) || item.Name.ToLower().Contains(result.VideoSearchResult.Author.ChannelTitle.ToLower()))
               {
                  result.Hits++;
                  artistInChannel = true;
               }
            }

            if (!artistInChannel && result.VideoSearchResult.Title.ToLower().Contains(artist.ToLower()))
            {
               result.Hits++;
            }


            if (result.VideoSearchResult.Duration != null)
            {
               TimeSpan t2 = TimeSpan.FromMilliseconds(result.VideoSearchResult.Duration.Value.TotalMilliseconds);
               result.OffsetTimeSpan = t2 - t1;
            }
         }

         List<VideoResultFromYtSearch> exact = results.FindAll(x => x.OffsetTimeSpan == TimeSpan.FromMilliseconds(0));
         List<VideoResultFromYtSearch> positive = results.FindAll(x => x.OffsetTimeSpan > TimeSpan.FromMilliseconds(0));
         List<VideoResultFromYtSearch> negative = results.FindAll(x => x.OffsetTimeSpan < TimeSpan.FromMilliseconds(0));

         if (exact.Any())
         {
            exact = exact.OrderBy(search => search.Hits).ToList();
            exact.Reverse();
            exact[0].Hits += 2;
         }

         positive.Sort((ps1, ps2) => TimeSpan.Compare(ps1.OffsetTimeSpan, ps2.OffsetTimeSpan));

         negative.Sort((ps1, ps2) => TimeSpan.Compare(ps1.OffsetTimeSpan, ps2.OffsetTimeSpan));

         List<VideoResultFromYtSearch> topResults = new();


         if (positive.Any())
         {
            topResults.Add(positive[0]);
         }

         if (negative.Any())
         {
            negative[0].OffsetTimeSpan *= -1;
            topResults.Add(negative[0]);
         }

         if (topResults.Any())
         {
            topResults.Sort((ps1, ps2) => TimeSpan.Compare(ps1.OffsetTimeSpan, ps2.OffsetTimeSpan));
            topResults[0].Hits += 2;
         }

         results.Clear();

         results.AddRange(exact);
         results.AddRange(positive);
         results.AddRange(negative);

         results = results.OrderBy(search => search.Hits).ToList();
         results.Reverse();

         /*DiscordEmbedBuilder discordEmbedBuilder = new()
         {
            Title = $"Spotify <{externalIds}>"
         };
         discordEmbedBuilder.AddField(new DiscordEmbedField($"{t1:mm\\:ss}   |   {artist}   -   {trackName}", externalIds));
   
         int i = 1;
         foreach (VideoResultFromYTSearch result in results)
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField($"Result number {i}", $"{result.Hits} hints   |   {result.OffsetTimeSpan:mm\\:ss} TimeSpanOffset"));
   
            discordEmbedBuilder.AddField(new DiscordEmbedField("" + $"{TimeSpan.FromMilliseconds(result.VideoSearchResult.Duration.Value.TotalMilliseconds):mm\\:ss}   |   " + $"{result.VideoSearchResult.Author.ChannelTitle}   -   " + $"{result.VideoSearchResult.Title}", result.VideoSearchResult.Url));
            i++;
         }
         try
         {
            _ = await Bot.DebugDiscordChannel.SendMessageAsync(discordEmbedBuilder.Build());
         }
         catch (Exception ex)
         {
            Console.WriteLine(ex);
         }*/

         int i = 1;

         string something = $"\n\n {fullTrack.ExternalUrls.FirstOrDefault()} \n{t1:mm\\:ss}   |   {artist}   -   {trackName}\n";
         foreach (VideoResultFromYtSearch result in results)
         {
            something += $"Result number {i}\n";
            something += $"{result.Hits} hints   |   {result.OffsetTimeSpan:mm\\:ss} TimeSpanOffset\n";

            if (result.VideoSearchResult.Duration != null)
            {
               something += $"{TimeSpan.FromMilliseconds(result.VideoSearchResult.Duration.Value.TotalMilliseconds):mm\\:ss}   |   " + $"{result.VideoSearchResult.Author.ChannelTitle}   -   " + $"{result.VideoSearchResult.Title}\n";
            }

            something += result.VideoSearchResult.Url + "\n";
            i++;
         }

         try
         {
            Uri path = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SchattenclownBot");
            Uri filepath = new($"{path}/MusicDebug.log");
            StreamWriter streamWriter = new(filepath.AbsolutePath, true);
            streamWriter.Write(something);
            streamWriter.Close();
         }
         catch
         {
            try
            {
               Uri path = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SchattenclownBot");
               Uri filepath = new($"{path}/MusicDebug2.log");
               StreamWriter streamWriter = new(filepath.AbsolutePath, true);
               streamWriter.Write(something);
               streamWriter.Close();
            }
            catch
            {
               try
               {
                  Uri path = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SchattenclownBot");
                  Uri filepath = new($"{path}/MusicDebug3.log");
                  StreamWriter streamWriter = new(filepath.AbsolutePath, true);
                  streamWriter.Write(something);
                  streamWriter.Close();
               }
               catch (Exception ex)
               {
                  CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">b__0_0>d", "").Replace("<", ""), ConsoleColor.Red);
               }
            }
         }

         return new Uri(results.FirstOrDefault()?.VideoSearchResult.Url ?? string.Empty);
      }

      internal static Task StopMusicTask(Gmc gMc, bool sendStopped)
      {
         bool nothingToStop = true;
         List<CancellationTokenSource> cancellationTokenSourceList = new();
         foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == gMc.DiscordGuild))
         {
            nothingToStop = false;
            cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
         }

         CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == gMc.DiscordGuild);

         foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
         {
            cancellationToken.Cancel();
            cancellationToken.Dispose();
         }

         if (sendStopped)
         {
            gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(nothingToStop ? "Nothing to stop!" : "Stopped the music!")));
         }

         QueueTracks.RemoveAll(x => x.Gmc.DiscordGuild == gMc.DiscordGuild);

         try
         {
            VoiceNextExtension voiceNext = Bot.DiscordClient.GetVoiceNext();
            VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(gMc.DiscordGuild);
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

      internal static string TimeLineStringBuilderWhilePlaying(int timeSpanAdvanceInt, TimeSpan totalTimeSpan, CancellationToken cancellationToken)
      {
         TimeSpan playerAdvanceTimeSpan = TimeSpan.FromSeconds(timeSpanAdvanceInt);

         string playerAdvanceString = PlayerAdvance(timeSpanAdvanceInt, totalTimeSpan);

         string descriptionString = "⏹️";
         if (cancellationToken.IsCancellationRequested)
         {
            descriptionString = "▶️";
         }

         if (totalTimeSpan.Hours != 0)
         {
            descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Hours:#00}:{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
         }
         else
         {
            descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
         }

         return descriptionString;
      }

      internal static string TimeLineStringBuilderAfterTrack(int timeSpanAdvanceInt, TimeSpan totalTimeSpan, CancellationToken cancellationToken)
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

         if (totalTimeSpan.Hours != 0)
         {
            descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Hours:#00}:{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
         }
         else
         {
            descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
         }

         return descriptionString;
      }

      internal static string PlayerAdvance(int timeSpanAdvanceInt, TimeSpan totalTimeSpan)
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

      internal static AcoustId.Root AcoustIdFromFingerPrint(Uri filePathUri)
      {
         string[] fingerPrintDuration = default;
         string[] fingerPrintFingerprint = default;
         ProcessStartInfo fingerPrintCalculationProcessStartInfo = new()
         {
            FileName = "..\\..\\..\\Model\\Executables\\fpcalc\\fpcalc.exe",
            Arguments = filePathUri.LocalPath,
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

      internal static DiscordEmbedBuilder CustomDiscordEmbedBuilder(DiscordEmbedBuilder discordEmbedBuilder, QueueTrack queueTrack, Uri filePathUri, VideoData audioDownloadMetaData, File metaTagFileToPlay)
      {
         if (metaTagFileToPlay == null)
         {
            bool needThumbnail = true;
            bool needAlbum = true;
            string albumTitle = "";
            string recordingMbId = "";

            if (queueTrack.YouTubeUri != null)
            {
               discordEmbedBuilder.WithUrl(queueTrack.YouTubeUri.AbsoluteUri);
            }

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

               if (audioDownloadMetaData == null)
               {
                  discordEmbedBuilder.Title = fullTrack.Name;
                  discordEmbedBuilder.WithAuthor(fullTrack.Artists.First().Name);
                  discordEmbedBuilder.WithUrl(queueTrack.SpotifyUri.AbsoluteUri);
               }

               if (audioDownloadMetaData != null)
               {
                  discordEmbedBuilder.Title = audioDownloadMetaData.Title;
                  discordEmbedBuilder.WithAuthor(audioDownloadMetaData.Creator);
               }
            }

            string tempPath = "";

            if (queueTrack.SpotifyUri != null)
            {
               tempPath = @$"M:\Spotify2mp3\{queueTrack.FullTrack.ExternalIds.First().Value}-{queueTrack.FullTrack.Id}.mp3";
            }
            else if (queueTrack.LocalPath != null)
            {
               tempPath = queueTrack.LocalPath;
            }
            else if (filePathUri.LocalPath != "")
            {
               tempPath = filePathUri.LocalPath;
            }

            if (tempPath != "")
            {
               try
               {
                  File tagFile = File.Create(@$"{tempPath}");

                  if (tagFile.Tag.Title != null)
                  {
                     discordEmbedBuilder.AddField(new DiscordEmbedField("Title", tagFile.Tag.Title, true));
                  }

                  if (tagFile.Tag.FirstAlbumArtist != null)
                  {
                     discordEmbedBuilder.AddField(new DiscordEmbedField("FirstAlbumArtist", tagFile.Tag.FirstAlbumArtist, true));
                  }

                  if (tagFile.Tag.Album != null)
                  {
                     discordEmbedBuilder.AddField(new DiscordEmbedField("Album", tagFile.Tag.Album, true));
                  }

                  discordEmbedBuilder.AddField(tagFile.Properties.Duration.Hours == 0 ? new DiscordEmbedField("Duration", $"{tagFile.Properties.Duration.Minutes:#00}:{tagFile.Properties.Duration.Seconds:#00}", true) : new DiscordEmbedField("Duration", $"{tagFile.Properties.Duration.Hours:#00}:{tagFile.Properties.Duration.Minutes:#00}:{tagFile.Properties.Duration.Seconds:#00}", true));


                  discordEmbedBuilder.AddField(new DiscordEmbedField("AudioBitRate", tagFile.Properties.AudioBitrate.ToString(), true));

                  discordEmbedBuilder.AddField(new DiscordEmbedField("Year", tagFile.Tag.Year.ToString(), true));

                  if (queueTrack.LocalPath != null)
                  {
                     filePathUri = new Uri(tempPath);
                  }
               }
               catch
               {
                  //ignore
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

            if (needThumbnail && audioDownloadMetaData != null)
            {
               discordEmbedBuilder.WithThumbnail(audioDownloadMetaData.Thumbnails[18].Url);
               try
               {
                  Bitmap bitmapAlbumCover = new(new HttpClient().GetStreamAsync(audioDownloadMetaData.Thumbnails[18].Url).Result);
                  Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
                  discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
               }
               catch (Exception e)
               {
                  Console.WriteLine(e);
               }
            }

            if (recordingMbId != "")
            {
               discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainz", $"[[<:MusicBrainz:1050439464452894720> Open](https://musicbrainz.org/recording/{recordingMbId})]", true));
            }

            if (audioDownloadMetaData != null)
            {
               if (albumTitle != "")
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("Album", albumTitle, true));
               }

               if (audioDownloadMetaData != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("Uploader", audioDownloadMetaData.Uploader, true));
               }
            }
         }
         else
         {
            if (metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId == null && metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId == null)
            {
               if (metaTagFileToPlay.Tag.MusicBrainzArtistId != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzArtistId", metaTagFileToPlay.Tag.MusicBrainzArtistId, true));
               }

               if (metaTagFileToPlay.Tag.MusicBrainzDiscId != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzDiscId", metaTagFileToPlay.Tag.MusicBrainzDiscId, true));
               }

               if (metaTagFileToPlay.Tag.MusicBrainzReleaseArtistId != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseArtistId", metaTagFileToPlay.Tag.MusicBrainzReleaseArtistId, true));
               }

               if (metaTagFileToPlay.Tag.MusicBrainzReleaseCountry != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseCountry", metaTagFileToPlay.Tag.MusicBrainzReleaseCountry, true));
               }

               if (metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseGroupId", metaTagFileToPlay.Tag.MusicBrainzReleaseGroupId, true));
               }

               if (metaTagFileToPlay.Tag.MusicBrainzReleaseId != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseId", metaTagFileToPlay.Tag.MusicBrainzReleaseId, true));
               }

               if (metaTagFileToPlay.Tag.MusicBrainzReleaseStatus != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseStatus", metaTagFileToPlay.Tag.MusicBrainzReleaseStatus, true));
               }

               if (metaTagFileToPlay.Tag.MusicBrainzReleaseType != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseType", metaTagFileToPlay.Tag.MusicBrainzReleaseType, true));
               }

               if (metaTagFileToPlay.Tag.MusicBrainzTrackId != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzTrackId", metaTagFileToPlay.Tag.MusicBrainzTrackId, true));
               }

               if (metaTagFileToPlay.Tag.MusicIpId != null)
               {
                  discordEmbedBuilder.AddField(new DiscordEmbedField("MusicIpId", metaTagFileToPlay.Tag.MusicIpId, true));
               }
            }

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
         }

         return discordEmbedBuilder;
      }

      internal static bool NoMusicPlaying(DiscordGuild discordGuild)
      {
         return CancellationTokenItemList.All(cancellationTokenItem => cancellationTokenItem.DiscordGuild != discordGuild);
      }

      internal static SpotifyClient GetSpotifyClientConfig()
      {
         SpotifyClientConfig spotifyClientConfig = SpotifyClientConfig.CreateDefault();
         ClientCredentialsRequest clientCredentialsRequest = new(Bot.Connections.SpotifyToken.ClientId, Bot.Connections.SpotifyToken.ClientSecret);
         ClientCredentialsTokenResponse clientCredentialsTokenResponse = new OAuthClient(spotifyClientConfig).RequestToken(clientCredentialsRequest).Result;
         SpotifyClient spotifyClient = new(clientCredentialsTokenResponse.AccessToken);
         return spotifyClient;
      }
   }
}