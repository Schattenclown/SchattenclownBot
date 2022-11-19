using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using SchattenclownBot.Model.Discord.AppCommands.Music.Objects;

namespace SchattenclownBot.Model.Discord.AppCommands.Music;

internal class Events
{
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
               foreach (DC_CancellationTokenItem cancellationTokenItem in Main.CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
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

               Main.QueueTracks.RemoveAll(x => x.GMC.DiscordGuild == eventArgs.Guild);

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
               await Main.StopMusicTask(new GMC(eventArgs.Guild, eventArgs.User.ConvertToMember(eventArgs.Guild).Result, eventArgs.Channel), false);
            }
         }
      }
      catch
      {
         // ignored
      }
   }

   internal static Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
   {
      eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

      if (Main.QueueTracks.Any(x => x.GMC.DiscordGuild == eventArgs.Guild))
      {
         DiscordMember discordMember = eventArgs.User.ConvertToMember(eventArgs.Guild).Result;
         GMC gMC = new(eventArgs.Guild, discordMember, eventArgs.Channel);

         if (gMC.DiscordMember.VoiceState == null)
         {
            eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return Task.CompletedTask;
         }

         switch (eventArgs.Id)
         {
            case "PreviousTrackStream":
            {
               Main.PlayPreviousTrackFromQueue(gMC);
               break;
            }
            case "NextTrackStream":
            {
               Main.PlayNextTrackFromQueue(gMC);
               break;
            }
            case "StopTrackStream":
            {
               Main.StopMusicTask(new GMC(eventArgs.Guild, discordMember, eventArgs.Channel), false);
               break;
            }
            case "ShuffleStream":
            {
               gMC.DiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Shuffle requested!")));
               Main.ShuffleQueueTracksAsyncTask(gMC);
               break;
            }
            case "ShowQueueStream":
            {
               DiscordMessage discordMessage = eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Loading!"))).Result;

               if (Main.QueueTracks.All(x => x.GMC.DiscordGuild != gMC.DiscordGuild && x.HasBeenPlayed))
               {
                  discordMessage.ModifyAsync("Queue is empty!");
               }
               else
               {
                  string descriptionString = "";
                  DiscordEmbedBuilder discordEmbedBuilder = new();

                  List<QueueTrack> queueTracks = Main.QueueTracks.FindAll(x => x.GMC.DiscordChannel == gMC.DiscordChannel && !x.HasBeenPlayed);

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
}