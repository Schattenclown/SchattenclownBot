/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.VoiceNext;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Model.Discord.AppCommands.Music.Objects;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot.Model.Discord.AppCommands.Music;

internal class PlayMusicDrive : ApplicationCommandsModule
{
   private static readonly List<DC_CancellationTokenItem> CancellationTokenItemList = new();
   public static readonly List<QueueTrack> QueueTracks = new();
   public static readonly List<QueueTrack> CompareQueueTracks = new();

   [SlashCommand("Skip" + TwitchAPI.isDevBot, "Skip this track!")]
   private async Task SkipCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
      await NextTrackTask(interactionContext);
   }

   [SlashCommand("Next" + TwitchAPI.isDevBot, "Skip this track!")]
   private async Task NextCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
      await NextTrackTask(interactionContext);
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
         {
            return;
         }

         VoiceNextConnection voiceNextConnection = voiceNextExtension.GetConnection(discordGuild);
         DiscordVoiceState discordMemberVoiceState = interactionContext != null ? interactionContext.Member?.VoiceState : discordMember?.VoiceState;

         if (discordMemberVoiceState?.Channel == null)
         {
            return;
         }

         voiceNextConnection ??= await voiceNextExtension.ConnectAsync(discordMemberVoiceState.Channel);

         if (isInitialMessage)
         {
            if (interactionContext != null)
            {
               await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!"));
            }
            else
            {
               await interactionDiscordChannel.SendMessageAsync($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!");
            }
         }

         while (!cancellationToken.IsCancellationRequested)
         {
            Uri networkDriveUri = new(@"M:\");
            string[] allFiles = Directory.GetFiles(networkDriveUri.AbsolutePath);

            Random random = new();
            int randomInt = random.Next(0, allFiles.Length - 1);
            string selectedFileToPlay = allFiles[randomInt];

            File metaTagFileToPlay = File.Create(@$"{selectedFileToPlay}");
            DiscordEmbedBuilder discordEmbedBuilder = CustomDiscordEmbedBuilder(new DiscordEmbedBuilder(), null, null, null, metaTagFileToPlay);

            try
            {
               DiscordMessage discordMessage = await interactionDiscordChannel.SendMessageAsync(discordEmbedBuilder.Build());

               DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
               DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
               DiscordComponent[] discordComponents = new DiscordComponent[2];
               discordComponents[0] = new DiscordButtonComponent(ButtonStyle.Primary, "next_track", "Next!", false, discordComponentEmojisNext);
               discordComponents[1] = new DiscordButtonComponent(ButtonStyle.Danger, "stop_track", "Stop!", false, discordComponentEmojisStop);

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

               discordEmbedBuilder.Description = TimeLineStringBuilderAfterTrack(timeSpanAdvanceInt, metaTagFileToPlay.Properties.Duration, cancellationToken);
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
         {
            interactionContext.Client.Logger.LogError(exc.Message);
         }
         else
         {
            discordClient.Logger.LogError(exc.Message);
         }
      }
   }

   [SlashCommand("DrivePlay" + TwitchAPI.isDevBot, "Just plays some random music!")]
   private async Task DrivePlayCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

      if (interactionContext.Member.VoiceState == null)
      {
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be connected!"));
         return;
      }

      if (QueueItemList.Any(x => x.DiscordGuild == interactionContext.Guild))
      {
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing! This interaction is locked!"));
         return;
      }

      if (NoMusicPlaying(interactionContext.Guild))
      {
         CancellationTokenSource tokenSource = new();
         CancellationToken cancellationToken = tokenSource.SpotifyToken;
         DC_CancellationTokenItem dcCancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
         CancellationTokenItemList.Add(dcCancellationTokenKeyPair);

         try
         {
            Task.Run(() => DrivePlayTask(interactionContext, null, null, null, null, cancellationToken, true), cancellationToken);
         }
         catch
         {
            CancellationTokenItemList.Remove(dcCancellationTokenKeyPair);
         }
      }
      else
      {
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is already playing!"));
      }
   }

   private static async Task NextTrackTask(InteractionContext interactionContext)
   {
      if (interactionContext.Member.VoiceState == null)
      {
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be connected!"));
         return;
      }

      if (QueueItemList.Any(x => x.DiscordGuild == interactionContext.Guild))
      {
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("'Youtube' music is running! This interaction is locked!"));
         return;
      }

      await StopMusicTask(new GMC(interactionContext.Guild, interactionContext.Channel, interactionContext.Member), false);

      CancellationTokenSource tokenSource = new();
      CancellationToken cancellationToken = tokenSource.SpotifyToken;
      DC_CancellationTokenItem dcCancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
      CancellationTokenItemList.Add(dcCancellationTokenKeyPair);

      try
      {
         Task.Run(() => DrivePlayTask(interactionContext, null, null, null, null, cancellationToken, false), cancellationToken);
      }
      catch (Exception ex)
      {
         CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
         CancellationTokenItemList.Remove(dcCancellationTokenKeyPair);
      }
   }

   internal static Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
   {
      eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

      switch (eventArgs.Id)
      {
         case "NextTrack":
         {
            DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
            if (discordMember.VoiceState == null)
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return Task.CompletedTask;
            }

            bool nothingToStop = true;
            List<CancellationTokenSource> cancellationTokenSourceList = new();
            foreach (DC_CancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
            {
               nothingToStop = false;
               cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
            }

            eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(nothingToStop ? "Nothing to stop!" : "Stopped the music!")));

            CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

            foreach (CancellationTokenSource cancellationTokenItem in cancellationTokenSourceList)
            {
               cancellationTokenItem.Cancel();
               cancellationTokenItem.Dispose();
            }

            QueueTrack queueTrack = default;
            foreach (QueueTrack item in QueueTracks.Where(x => x.GMC.DiscordGuild == eventArgs.Guild && x.HasBeenPlayed == false))
            {
               queueTrack = item;
               break;
            }

            CancellationTokenSource tokenSource = new();
            CancellationToken cancellationToken = tokenSource.SpotifyToken;
            DC_CancellationTokenItem dcCancellationTokenKeyPair = new(eventArgs.Guild, tokenSource);
            CancellationTokenItemList.Add(dcCancellationTokenKeyPair);

            try
            {
               Task.Run(() => PlayFromQueueAsyncTask(new GMC(eventArgs.Guild, eventArgs.Channel, discordMember), queueTrack, cancellationToken), cancellationToken);
            }
            catch (Exception ex)
            {
               CwLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
               CancellationTokenItemList.Remove(dcCancellationTokenKeyPair);
            }

            break;
         }
         case "StopTrack":
         {
            DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
            if (discordMember.VoiceState == null)
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return Task.CompletedTask;
            }

            StopMusicTask(new GMC(eventArgs.Guild, eventArgs.Channel, discordMember), false);

            break;
         }
         case "PreviousTrackStream":
         {
            DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
            if (discordMember.VoiceState == null)
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return Task.CompletedTask;
            }

            if (QueueItemList.All(x => x.DiscordGuild != eventArgs.Guild))
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Nothing to skip!")));
               return Task.CompletedTask;
            }

            foreach (QueueItem queueItem in QueueItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
            {
               int indexOfPlaying = CompareQueueItemList.FindIndex(x => x.DiscordGuild == queueItem.DiscordGuild && ((x.SpotifyUri == queueItem.SpotifyUri && x.SpotifyUri != null && queueItem.SpotifyUri != null) || (x.YouTubeUri == queueItem.YouTubeUri && x.YouTubeUri != null && queueItem.YouTubeUri != null))) - 1;

               int indexOfLast = CompareQueueItemList.FindIndex(x => x.DiscordGuild == queueItem.DiscordGuild && ((x.SpotifyUri == queueItem.SpotifyUri && x.SpotifyUri != null && queueItem.SpotifyUri != null) || (x.YouTubeUri == queueItem.YouTubeUri && x.YouTubeUri != null && queueItem.YouTubeUri != null))) - 2;

               if (indexOfLast >= 0)
               {
                  QueueItemList.Insert(0, CompareQueueItemList.ElementAt(indexOfPlaying));
                  QueueItemList.Insert(0, CompareQueueItemList.ElementAt(indexOfLast));
               }
               else
               {
                  eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Wrong universe!")));
                  return Task.CompletedTask;
               }

               break;
            }

            List<CancellationTokenSource> cancellationTokenSourceList = new();
            foreach (DC_CancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
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
            CancellationToken newCancellationToken = newCancellationTokenSource.SpotifyToken;

            foreach (QueueItem queueItem in QueueItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
            {
               DC_CancellationTokenItem newDcCancellationTokenItem = new(eventArgs.Guild, newCancellationTokenSource);
               CancellationTokenItemList.Add(newDcCancellationTokenItem);

               //Task.Run(() => PlayFromQueueAsyncTask(eventArgs.Guild, discordMember, eventArgs.Channel, queueItem, newCancellationToken, false), newCancellationToken);
               break;
            }

            break;
         }
         case "NextTrackStream":
         {
            DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
            if (discordMember.VoiceState == null)
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return Task.CompletedTask;
            }

            if (QueueTracks.All(x => x.GMC.DiscordGuild != eventArgs.Guild && x.HasBeenPlayed))
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Nothing to skip!")));
               return Task.CompletedTask;
            }

            List<CancellationTokenSource> cancellationTokenSourceList = new();
            foreach (DC_CancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
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
            CancellationToken newCancellationToken = newCancellationTokenSource.SpotifyToken;

            foreach (QueueTrack queueTrack in QueueTracks.Where(x => x.GMC.DiscordGuild == eventArgs.Guild && !x.HasBeenPlayed))
            {
               DC_CancellationTokenItem newDcCancellationTokenItem = new(eventArgs.Guild, newCancellationTokenSource);
               CancellationTokenItemList.Add(newDcCancellationTokenItem);

               Task.Run(() => PlayFromQueueAsyncTask(new GMC(eventArgs.Guild, eventArgs.Channel, discordMember), queueTrack, newCancellationToken), newCancellationToken);
               break;
            }

            break;
         }
         case "StopTrackStream":
         {
            DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
            if (discordMember.VoiceState == null)
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return Task.CompletedTask;
            }

            StopMusicTask(new GMC(eventArgs.Guild, eventArgs.Channel, discordMember), false);

            break;
         }
         case "ShuffleStream":
         {
            DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
            if (discordMember.VoiceState == null)
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return Task.CompletedTask;
            }

            DiscordMessage discordMessage = eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Shuffle requested!"))).Result;

            ShufflePlaylist(discordMessage);
            break;
         }
         case "ShowQueueStream":
         {
            DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
            if (discordMember.VoiceState == null)
            {
               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return Task.CompletedTask;
            }

            DiscordMessage discordMessage = eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Loading!"))).Result;

            if (QueueItemList.All(x => x.DiscordGuild != eventArgs.Guild))
            {
               discordMessage.ModifyAsync("Queue is empty!");
            }
            else
            {
               string descriptionString = "";
               DiscordEmbedBuilder discordEmbedBuilder = new();
               YoutubeClient youtubeClient = new();

               List<QueueItem> queueItemList = QueueItemList.FindAll(x => x.DiscordGuild == eventArgs.Channel.Guild);

               for (int i = 0; i < 10; i++)
               {
                  if (queueItemList.Count == i)
                  {
                     break;
                  }

                  Video videoData = youtubeClient.Videos.GetAsync(queueItemList[i].YouTubeUri.AbsoluteUri).Result;

                  if (queueItemList[i].IsSpotify)
                  {
                     descriptionString += "[🔗[YouTube]" + $"({queueItemList[i].YouTubeUri.AbsoluteUri})] " + "[🔗[Spotify]" + $"({queueItemList[i].SpotifyUri.AbsoluteUri})]  " + videoData.Title + " - " + videoData.Author + "\n";
                  }
                  else
                  {
                     descriptionString += "[🔗[YouTube]" + $"({queueItemList[i].YouTubeUri.AbsoluteUri})] " + videoData.Title + " - " + videoData.Author + "\n";
                  }
               }

               discordEmbedBuilder.Title = $"{queueItemList.Count} Track/s in queue!";
               discordEmbedBuilder.WithDescription(descriptionString);
               discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder));
            }

            break;
         }
      }

      return Task.CompletedTask;
   }
}*/

