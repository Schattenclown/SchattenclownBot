﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using SchattenclownBot.Model.Discord.AppCommands.Music.Objects;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   internal class Events
   {
      internal static async Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
      {
         await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

         if (Main.QueueTracks.Any(x => x.Gmc.DiscordGuild == eventArgs.Guild))
         {
            DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
            Gmc gMc = new(eventArgs.Guild, discordMember, eventArgs.Channel);

            if (gMc.DiscordMember.VoiceState == null)
            {
               await eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return;
            }

            switch (eventArgs.Id)
            {
               case "PreviousTrackStream":
               {
                  Main.PlayPreviousTrackFromQueue(gMc);
                  break;
               }
               case "NextTrackStream":
               {
                  Main.PlayNextTrackFromQueue(gMc);
                  break;
               }
               case "StopTrackStream":
               {
                  _ = Main.StopMusicTask(new Gmc(eventArgs.Guild, discordMember, eventArgs.Channel), false);
                  break;
               }
               case "ShuffleStream":
               {
                  DiscordMessage discordMessage = await gMc.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Shuffle requested!")));
                  _ = Main.ShuffleQueueTracksAsyncTask(gMc, discordMessage);
                  break;
               }
               case "ShowQueueStream":
               {
                  DiscordMessage discordMessage = eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Loading!"))).Result;

                  if (Main.QueueTracks.All(x => x.Gmc.DiscordGuild != gMc.DiscordGuild && x.HasBeenPlayed))
                  {
                     await discordMessage.ModifyAsync("Queue is empty!");
                  }
                  else
                  {
                     string descriptionString = "";
                     DiscordEmbedBuilder discordEmbedBuilder = new();

                     List<QueueTrack> queueTracks = Main.QueueTracks.FindAll(x => x.Gmc.DiscordChannel == gMc.DiscordChannel && !x.HasBeenPlayed);


                     //<a:twitch:1050340762459586560>
                     //<:spotify:1050436741393297470>
                     //<:youtube:1050436748578136184>
                     //<:MusicBrainz:1050439464452894720>

                     for (int i = 0; i < 15; i++)
                     {
                        if (queueTracks.Count == i)
                        {
                           break;
                        }

                        if (queueTracks[i].LocalPath == null)
                        {
                           if (queueTracks[i].FullTrack != null)
                           {
                              descriptionString += "[<:youtube:1050436748578136184> [YouTube]" + $"({queueTracks[i].YouTubeUri.AbsoluteUri})] " + "[<:spotify:1050436741393297470> [Spotify]" + $"({queueTracks[i].SpotifyUri.AbsoluteUri})]  " + queueTracks[i].Title + " - " + queueTracks[i].Artist + "\n";
                           }
                           else
                           {
                              descriptionString += "[<:youtube:1050436748578136184> [YouTube]" + $"({queueTracks[i].YouTubeUri.AbsoluteUri})] " + queueTracks[i].Title + " - " + queueTracks[i].Artist + "\n";
                           }
                        }
                        else
                        {
                           descriptionString += "[<:drive:1078293568151625729>] [Local] " + $"{queueTracks[i].LocalPath}) " + "\n";
                        }
                     }

                     discordEmbedBuilder.Title = $"{queueTracks.Count} Track/s in queue!";
                     discordEmbedBuilder.WithDescription(descriptionString);
                     await discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder));
                  }

                  break;
               }
            }
         }
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
                  foreach (DcCancellationTokenItem cancellationTokenItem in Main.CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
                  {
                     nothingToStop = false;
                     {
                        cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
                     }
                  }

                  Main.CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

                  foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
                  {
                     cancellationToken.Cancel();
                     cancellationToken.Dispose();
                  }

                  Main.QueueTracks.RemoveAll(x => x.Gmc.DiscordGuild == eventArgs.Guild);

                  await eventArgs.Channel.SendMessageAsync(nothingToStop ? "Queue void and Left!" : "Stopped the music!");
                  VoiceNextExtension voiceNext = client.GetVoiceNext();
                  VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(eventArgs.Guild);
                  voiceNextConnection?.Disconnect();
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
                  await Main.StopMusicTask(new Gmc(eventArgs.Guild, eventArgs.User.ConvertToMember(eventArgs.Guild).Result, eventArgs.Channel), false);
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