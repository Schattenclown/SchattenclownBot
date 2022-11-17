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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
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
   private static readonly List<QueueCreating> QueueCreatingList = new();
   public static readonly List<PlayingStream> PlayingStreamList = new();
   public static readonly List<QueueItem> QueueItemList = new();
   public static readonly List<QueueItem> PlayedQueueItemList = new();

   public static async void NextTrackRequestApi(API aPI)
   {
      CwLogger.Write(aPI.RequestTimeStamp + " " + aPI.RequesterIp + " " + aPI.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__5", "").Replace("<", ""), ConsoleColor.DarkYellow);
      API.DELETE(aPI.CommandRequestId);

      DiscordGuild interactionDiscordGuild = default;
      DiscordMember interactionDiscordMember = default;
      DiscordChannel interactionDiscordChannel = default;

      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPI.RequestDiscordUserId))
         {
            interactionDiscordGuild = guildItem;
            interactionDiscordMember = memberItem;
            interactionDiscordChannel = interactionDiscordMember.VoiceState.Channel;
            break;
         }

      if (interactionDiscordMember == null)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
            foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == aPI.RequestDiscordUserId))
            {
               await memberItem.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return;
            }
      }


      if (interactionDiscordGuild == null || interactionDiscordMember == null || interactionDiscordChannel == null)
         return;
      if (QueueItemList.All(x => x.DiscordGuild != interactionDiscordGuild))
      {
         interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Nothing to skip!")));
         return;
      }

      List<CancellationTokenSource> cancellationTokenSourceList = new();
      foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == interactionDiscordGuild)) cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);

      CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == interactionDiscordGuild);

      foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
      {
         cancellationToken.Cancel();
         cancellationToken.Dispose();
      }

      CancellationTokenSource newCancellationTokenSource = new();
      CancellationToken newCancellationToken = newCancellationTokenSource.Token;

      foreach (QueueItem queueItem in QueueItemList.Where(x => x.DiscordGuild == interactionDiscordGuild))
      {
         DcCancellationTokenItem newDcCancellationTokenItem = new(interactionDiscordGuild, newCancellationTokenSource);
         CancellationTokenItemList.Add(newDcCancellationTokenItem);
         Task.Run(() => PlayFromQueueAsyncTask(null, Bot.DiscordClient, interactionDiscordGuild, interactionDiscordMember, interactionDiscordChannel, queueItem, newCancellationToken, false), newCancellationToken);
         break;
      }
   }

   public static async void PreviousTrackRequestApi(API aPI)
   {
      CwLogger.Write(aPI.RequestTimeStamp + " " + aPI.RequesterIp + " " + aPI.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name.Replace(">d__6", "").Replace("<", ""), ConsoleColor.DarkYellow);
      API.DELETE(aPI.CommandRequestId);

      DiscordGuild interactionDiscordGuild = default;
      DiscordMember interactionDiscordMember = default;
      DiscordChannel interactionDiscordChannel = default;

      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPI.RequestDiscordUserId))
         {
            interactionDiscordGuild = guildItem;
            interactionDiscordMember = memberItem;
            interactionDiscordChannel = interactionDiscordMember.VoiceState.Channel;
            break;
         }

      if (interactionDiscordMember == null)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
            foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == aPI.RequestDiscordUserId))
            {
               await memberItem.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return;
            }
      }

      if (QueueItemList.All(x => x.DiscordGuild != interactionDiscordGuild))
      {
         await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Wrong universe!")));
         return;
      }

      foreach (QueueItem queueItem in QueueItemList.Where(x => x.DiscordGuild == interactionDiscordGuild))
      {
         int indexOfPlaying = PlayedQueueItemList.FindIndex(x => x.DiscordGuild == queueItem.DiscordGuild && ((x.SpotifyUri == queueItem.SpotifyUri && x.SpotifyUri != null && queueItem.SpotifyUri != null) || (x.YouTubeUri == queueItem.YouTubeUri && x.YouTubeUri != null && queueItem.YouTubeUri != null))) - 1;

         int indexOfLast = PlayedQueueItemList.FindIndex(x => x.DiscordGuild == queueItem.DiscordGuild && ((x.SpotifyUri == queueItem.SpotifyUri && x.SpotifyUri != null && queueItem.SpotifyUri != null) || (x.YouTubeUri == queueItem.YouTubeUri && x.YouTubeUri != null && queueItem.YouTubeUri != null))) - 2;

         if (indexOfLast >= 0)
         {
            QueueItemList.Insert(0, PlayedQueueItemList.ElementAt(indexOfPlaying));
            QueueItemList.Insert(0, PlayedQueueItemList.ElementAt(indexOfLast));
         }
         else
         {
            await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Wrong universe!")));
            return;
         }

         break;
      }

      List<CancellationTokenSource> cancellationTokenSourceList = new();
      foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == interactionDiscordGuild)) cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);

      CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == interactionDiscordGuild);

      foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
      {
         cancellationToken.Cancel();
         cancellationToken.Dispose();
      }

      CancellationTokenSource newCancellationTokenSource = new();
      CancellationToken newCancellationToken = newCancellationTokenSource.Token;

      foreach (QueueItem queueItem in QueueItemList.Where(x => x.DiscordGuild == interactionDiscordGuild))
      {
         DcCancellationTokenItem newDcCancellationTokenItem = new(interactionDiscordGuild, newCancellationTokenSource);
         CancellationTokenItemList.Add(newDcCancellationTokenItem);
         Task.Run(() => PlayFromQueueAsyncTask(null, Bot.DiscordClient, interactionDiscordGuild, interactionDiscordMember, interactionDiscordChannel, queueItem, newCancellationToken, false), newCancellationToken);
         break;
      }
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
   private static void PlayCommand(InteractionContext interactionContext, [Option("Link", "Link!")] string webLink)
   {
      AddSongsToQueueAsync(interactionContext, webLink, null, false);
   }

   public static void API_PlayRequest(API api)
   {
      AddSongsToQueueAsync(null, null, api, false);
   }

   public static void API_ShufflePlayRequest(API api)
   {
      AddSongsToQueueAsync(null, null, api, true);
   }

   private static async Task AddSongsToQueueAsync(InteractionContext interactionContext, string webLink, API aPI, bool isShufflePlay)
   {
      DiscordGuild interactionDiscordGuild = default;
      DiscordMember interactionDiscordMember = default;
      DiscordChannel interactionDiscordChannel = default;

      if (aPI != null)
      {
         webLink = aPI.Data;

         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
            foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == aPI.RequestDiscordUserId && x.VoiceState != null))
            {
               interactionDiscordGuild = guildItem;
               interactionDiscordMember = memberItem;
               interactionDiscordChannel = interactionDiscordMember.VoiceState.Channel;
               break;
            }

         if (interactionDiscordMember == null)
         {
            foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
               foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == aPI.RequestDiscordUserId))
               {
                  await memberItem.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
                  return;
               }
         }

      }
      else if (interactionContext != null)
      {
         interactionDiscordGuild = interactionContext.Guild;
         interactionDiscordChannel = interactionContext.Channel;
         interactionDiscordMember = interactionContext.Member;

         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Working on it."));

         if (interactionContext.Member.VoiceState == null)
         {
            await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
            return;
         }
      }

      if (interactionDiscordGuild == null || interactionDiscordMember == null || interactionDiscordChannel == null)
         return;

      if (QueueCreatingList.Exists(x => x.DiscordGuild == interactionDiscordGuild))
      {
         await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("The queue is already being generated. Please wait until the first queue is generated! " + $"{QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount}/" + $"{QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAmount} Please wait!")));
         return;
      }

      if (!NoMusicPlaying(interactionDiscordGuild)) await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Queue is being created! Please be patient!")));

      Uri webLinkUri;
      try
      {
         webLinkUri = new Uri(webLink);
      }
      catch
      {
         await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Please check your link, something is wrong! The https:// tag may be missing")));
         return;
      }

      bool isYouTube = false;
      bool isYouTubePlaylist = false;
      bool isYouTubePlaylistWithIndex = false;
      bool isSpotify = false;
      bool isSpotifyPlaylist = false;
      bool isSpotifyAlbum = false;
      int tracksAdded = 0;

      if (webLink.Contains("watch?v=") || webLink.Contains("youtu.be") || webLink.Contains("&list=") || webLink.Contains("playlist?list="))
      {
         isYouTube = true;

         if (webLink.Contains("&list=") || webLink.Contains("playlist?list="))
         {
            isYouTubePlaylist = true;

            if (webLink.Contains("&index="))
               isYouTubePlaylistWithIndex = true;
         }
      }
      else if (webLink.Contains("/track/") || webLink.Contains("/playlist/") || webLink.Contains("/album/") || webLink.Contains(":album:"))
      {
         isSpotify = true;

         if (webLink.Contains("/playlist/"))
            isSpotifyPlaylist = true;
         else if (webLink.Contains("/album/") || webLink.Contains(":album:"))
            isSpotifyAlbum = true;
      }
      else
      {
         await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Sag ICH!!!")));
         return;
      }

      if (isYouTube)
      {
         try
         {
            if (isYouTubePlaylist)
            {
               int playlistSelectedVideoIndex = 1;
               string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveAfterWord(StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "&list=", "&list=".Length), "&index=", 0), "&ab_channel=", 0), "&start_radio=", 0);
               bool isYouTubeMix = webLinkUri.AbsoluteUri.Contains("&ab_channel=") || webLinkUri.AbsoluteUri.Contains("&start_radio=");
               YoutubeClient youtubeClient = new();

               QueueCreatingList.Add(new QueueCreating(interactionDiscordGuild, 0, 0));

               if (isYouTubePlaylistWithIndex)
               {
                  playlistSelectedVideoIndex = Convert.ToInt32(StringCutter.RemoveAfterWord(StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "&index=", "&index=".Length), "&ab_channel=", 0), "&start_radio=", 0));
                  string firstVideoId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "watch?v=", "watch?v=".Length), "&list=", 0);
                  Video firstVideo = await youtubeClient.Videos.GetAsync(firstVideoId);

                  await PlayQueueAsyncTask(interactionContext, interactionDiscordGuild, interactionDiscordMember, interactionDiscordChannel, new Uri(firstVideo.Url), null);
                  PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, new Uri(firstVideo.Url), null));
                  tracksAdded++;
               }

               List<PlaylistVideo> playlistVideos = new(await youtubeClient.Playlists.GetVideosAsync(playlistId));

               if (isShufflePlay)
               {
                  List<PlaylistVideo> queueItemListMixed = new();
                  List<PlaylistVideo> queueItemList = playlistVideos;

                  int queueLength = queueItemList.Count;
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

                  foreach (int randomInt in intListMixed) queueItemListMixed.Add(queueItemList[randomInt]);

                  playlistVideos = queueItemListMixed;
               }

               if (playlistSelectedVideoIndex != 1 && !isYouTubeMix)
                  playlistVideos.RemoveRange(0, playlistSelectedVideoIndex);
               else if (isYouTubePlaylistWithIndex) playlistVideos.RemoveRange(0, 1);

               QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAmount = playlistVideos.Count;

               int startIndex = 0;
               if (NoMusicPlaying(interactionDiscordGuild))
               {
                  await PlayQueueAsyncTask(interactionContext, interactionDiscordGuild, interactionDiscordMember, interactionDiscordChannel, new Uri(playlistVideos[startIndex].Url), null);
                  PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, new Uri(playlistVideos[startIndex].Url), null));
                  QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                  tracksAdded++;
                  startIndex++;
               }

               CancellationTokenSource cancellationTokenSource = CancellationTokenItemList.First(x => x.DiscordGuild == interactionDiscordGuild).CancellationTokenSource;

               while (startIndex < playlistVideos.Count)
               {
                  try
                  {
                     if (!interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser) && cancellationTokenSource.IsCancellationRequested)
                        break;

                     QueueItemList.Add(new QueueItem(interactionDiscordGuild, new Uri(playlistVideos[startIndex].Url), null));
                     PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, new Uri(playlistVideos[startIndex].Url), null));
                     QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                     tracksAdded++;
                  }
                  catch (Exception ex)
                  {
                     await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error adding " + playlistVideos[startIndex].Url + " " + ex.Message)));
                  }

                  startIndex++;
               }
            }
            else
            {
               //https://youtu.be/EW6c8o5ctRI
               string selectedVideoId;
               if (webLink.Contains("youtu.be"))
                  selectedVideoId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "youtu.be/", "youtu.be/".Length), "&list=", 0);
               else
                  selectedVideoId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "watch?v=", "watch?v=".Length), "&list=", 0);

               Uri selectedVideoUri = new("https://www.youtube.com/watch?v=" + selectedVideoId);
               QueueCreatingList.Add(new QueueCreating(interactionDiscordGuild, 1, 0));


               if (NoMusicPlaying(interactionDiscordGuild))
               {
                  await PlayQueueAsyncTask(interactionContext, interactionDiscordGuild, interactionDiscordMember, interactionDiscordChannel, selectedVideoUri, null);
                  PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, selectedVideoUri, null));
                  QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                  tracksAdded++;
               }
               else
               {
                  try
                  {
                     QueueItemList.Add(new QueueItem(interactionDiscordGuild, selectedVideoUri, null));
                     PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, selectedVideoUri, null));
                     QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                     tracksAdded++;
                  }
                  catch (Exception ex)
                  {
                     await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error adding " + selectedVideoUri + " " + ex.Message)));
                  }
               }
            }
         }
         catch (Exception ex)
         {
            QueueCreatingList.RemoveAll(x => x.DiscordGuild == interactionDiscordGuild);
            await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error!\n" + ex.Message)));
         }
      }
      else if (isSpotify)
      {
         SpotifyClient spotifyClient = GetSpotifyClientConfig();

         if (isSpotifyPlaylist)
         {
            string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/playlist/", "/playlist/".Length), "?si", 0);

            PlaylistGetItemsRequest playlistGetItemsRequest = new()
            {
               Offset = 0
            };

            List<PlaylistTrack<IPlayableItem>> playlistTrackList = spotifyClient.Playlists.GetItems(playlistId, playlistGetItemsRequest).Result.Items;

            if (playlistTrackList is { Count: >= 100 })
               try
               {
                  while (true)
                  {
                     playlistGetItemsRequest.Offset += 100;

                     List<PlaylistTrack<IPlayableItem>> playlistTrackListSecond = spotifyClient.Playlists.GetItems(playlistId, playlistGetItemsRequest).Result.Items;

                     if (playlistTrackListSecond != null)
                     {
                        playlistTrackList.AddRange(playlistTrackListSecond);

                        if (playlistTrackListSecond.Count < 100)
                           break;
                     }
                  }
               }
               catch
               {
                  // ignored
               }

            await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Generating queue for {playlistTrackList.Count} tracks!")));
            if (isShufflePlay)
            {
               List<PlaylistTrack<IPlayableItem>> queueItemListMixed = new();
               List<PlaylistTrack<IPlayableItem>> queueItemList = playlistTrackList;

               int queueLength = queueItemList.Count;
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

               foreach (int randomInt in intListMixed) queueItemListMixed.Add(queueItemList[randomInt]);

               playlistTrackList = queueItemListMixed;
            }

            if (playlistTrackList != null)
            {
               QueueCreatingList.Add(new QueueCreating(interactionDiscordGuild, playlistTrackList.Count, 0));

               if (playlistTrackList.Count != 0)
               {
                  int startIndex = 0;
                  if (NoMusicPlaying(interactionDiscordGuild))
                  {
                     FullTrack playlistTrack = playlistTrackList[startIndex].Track as FullTrack;

                     int tries = 0;
                     while (5 > tries)
                        try
                        {
                           FullTrack fullTrack = spotifyClient.Tracks.Get(playlistTrack!.Id).Result;
                           Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
                           await PlayQueueAsyncTask(interactionContext, interactionDiscordGuild, interactionDiscordMember, interactionDiscordChannel, youTubeUri, new Uri("https://open.spotify.com/track/" + playlistTrack!.Id));
                           PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + playlistTrack!.Id)));
                           QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                           tracksAdded++;
                           tries = 6;
                        }
                        catch (Exception ex)
                        {
                           if (!ex.Message.Contains("Service unavailable") && interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser))
                           {
                              await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error adding " + "https://open.spotify.com/track/" + playlistTrack!.Id + " " + ex.Message)));
                              tries = 6;
                           }

                           tries++;
                        }

                     startIndex++;
                  }

                  CancellationTokenSource cancellationTokenSource = CancellationTokenItemList.First(x => x.DiscordGuild == interactionDiscordGuild).CancellationTokenSource;
                  while (startIndex < playlistTrackList.Count)
                  {
                     FullTrack playlistTrack = playlistTrackList[startIndex].Track as FullTrack;

                     int tries = 0;
                     while (5 > tries)
                     {
                        if (!interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser) && cancellationTokenSource.IsCancellationRequested)
                           break;

                        try
                        {
                           FullTrack fullTrack = spotifyClient.Tracks.Get(playlistTrack!.Id).Result;
                           Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
                           QueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + playlistTrack!.Id)));
                           PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + playlistTrack!.Id)));
                           QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                           tracksAdded++;
                           tries = 6;
                        }
                        catch (Exception ex)
                        {
                           if (!ex.Message.Contains("Service unavailable") && interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser))
                           {
                              await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error adding " + "https://open.spotify.com/track/" + playlistTrack!.Id + " " + ex.Message)));

                              tries = 6;
                           }

                           tries++;
                        }
                     }

                     startIndex++;
                  }
               }
            }
         }
         else if (isSpotifyAlbum)
         {
            string albumId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(StringCutter.RemoveUntilWord(webLink, "/album/", "/album/".Length), ":album:", ":album:".Length), "?si", 0);
            List<SimpleTrack> simpleTrackList = spotifyClient.Albums.GetTracks(albumId).Result.Items;

            if (isShufflePlay)
            {
               List<SimpleTrack> queueItemListMixed = new();
               List<SimpleTrack> queueItemList = simpleTrackList;

               int queueLength = queueItemList.Count;
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

               foreach (int randomInt in intListMixed) queueItemListMixed.Add(queueItemList[randomInt]);

               simpleTrackList = queueItemListMixed;
            }

            if (simpleTrackList != null && simpleTrackList.Count != 0)
            {
               QueueCreatingList.Add(new QueueCreating(interactionDiscordGuild, simpleTrackList.Count, 0));

               int startIndex = 0;

               if (NoMusicPlaying(interactionDiscordGuild))
               {
                  SimpleTrack simpleTrack = simpleTrackList[startIndex];

                  int tries = 0;
                  while (5 > tries)
                     try
                     {
                        FullTrack fullTrack = spotifyClient.Tracks.Get(simpleTrack!.Id).Result;
                        Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
                        await PlayQueueAsyncTask(interactionContext, interactionDiscordGuild, interactionDiscordMember, interactionDiscordChannel, youTubeUri, new Uri("https://open.spotify.com/track/" + simpleTrack!.Id));
                        PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + simpleTrack!.Id)));
                        QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                        tracksAdded++;
                        tries = 6;
                     }
                     catch (Exception ex)
                     {
                        if (!ex.Message.Contains("Service unavailable") && interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser))
                        {
                           await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error adding " + "https://open.spotify.com/track/" + simpleTrack!.Id + " " + ex.Message)));

                           tries = 6;
                        }

                        tries++;
                     }

                  startIndex++;
               }

               CancellationTokenSource cancellationTokenSource = CancellationTokenItemList.First(x => x.DiscordGuild == interactionDiscordGuild).CancellationTokenSource;
               while (startIndex < simpleTrackList.Count)
               {
                  SimpleTrack simpleTrack = simpleTrackList[startIndex];

                  int tries = 0;
                  while (5 > tries)
                  {
                     if (!interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser) && cancellationTokenSource.IsCancellationRequested)
                        break;

                     try
                     {
                        FullTrack fullTrack = spotifyClient.Tracks.Get(simpleTrack!.Id).Result;
                        Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
                        QueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + simpleTrack!.Id)));
                        PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + simpleTrack!.Id)));
                        QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                        tracksAdded++;
                        tries = 6;
                     }
                     catch (Exception ex)
                     {
                        if (!ex.Message.Contains("Service unavailable") && interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser))
                        {
                           await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error adding " + "https://open.spotify.com/track/" + simpleTrack!.Id + " " + ex.Message)));

                           tries = 6;
                        }

                        tries++;
                     }

                     startIndex++;
                  }
               }
            }
            else
            {
               string trackId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/track/", "/track/".Length), "?si", 0);
               QueueCreatingList.Add(new QueueCreating(interactionDiscordGuild, 1, 0));

               if (NoMusicPlaying(interactionDiscordGuild))
               {
                  FullTrack fullTrack = spotifyClient.Tracks.Get(trackId).Result;

                  int tries = 0;
                  while (5 > tries)
                     try
                     {
                        Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
                        await PlayQueueAsyncTask(interactionContext, interactionDiscordGuild, interactionDiscordMember, interactionDiscordChannel, youTubeUri, new Uri("https://open.spotify.com/track/" + trackId));
                        PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + trackId)));
                        QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                        tracksAdded++;
                        tries = 6;
                     }
                     catch (Exception ex)
                     {
                        if (!ex.Message.Contains("Service unavailable") && interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser))
                        {
                           await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error adding " + "https://open.spotify.com/track/" + fullTrack!.Id + " " + ex.Message)));

                           tries = 6;
                        }

                        tries++;
                     }
               }
               else
               {
                  FullTrack fullTrack = spotifyClient.Tracks.Get(trackId).Result;
                  CancellationTokenSource cancellationTokenSource = CancellationTokenItemList.First(x => x.DiscordGuild == interactionDiscordGuild).CancellationTokenSource;

                  int tries = 0;
                  while (5 > tries)
                  {
                     if (!interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser) && cancellationTokenSource.IsCancellationRequested)
                        break;

                     try
                     {
                        Uri youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
                        QueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + trackId)));
                        PlayedQueueItemList.Add(new QueueItem(interactionDiscordGuild, youTubeUri, new Uri("https://open.spotify.com/track/" + trackId)));
                        QueueCreatingList.Find(x => x.DiscordGuild == interactionDiscordGuild)!.QueueAddedAmount++;
                        tracksAdded++;
                        tries = 6;
                     }
                     catch (Exception ex)
                     {
                        if (!ex.Message.Contains("Service unavailable") && interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser))
                        {
                           await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Error adding " + "https://open.spotify.com/track/" + fullTrack!.Id + " " + ex.Message)));
                           tries = 6;
                        }

                        tries++;
                     }
                  }
               }
            }
         }
      }

      QueueCreatingList.RemoveAll(x => x.DiscordGuild == interactionDiscordGuild);
      await Task.Delay(500);

      CancellationTokenSource cancellationTokenSourceEnd = CancellationTokenItemList.First(x => x.DiscordGuild == interactionDiscordGuild).CancellationTokenSource;
      if (!interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser) || (!interactionDiscordChannel.Users.Contains(Bot.DiscordClient.CurrentUser) && cancellationTokenSourceEnd.IsCancellationRequested))
         return;

      if (NoMusicPlaying(interactionDiscordGuild))
      {
         if (tracksAdded == 1)
            await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"{tracksAdded} track is now added to the queue!")));
         else
            await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"{tracksAdded} tracks are now added to the queue!")));
      }
      else
      {
         if (tracksAdded == 1)
            await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Music is already playing or will at any moment! {tracksAdded} track is now added to the queue!")));
         else
            await interactionDiscordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription($"Music is already playing or will at any moment! {tracksAdded} tracks are now added to the queue!")));
      }
   }

   public class VideoResultFromYTSearch
   {
      public VideoSearchResult VideoSearchResult { get; set; }
      public TimeSpan OffsetTimeSpan { get; set; }
      public int Hits { get; set; }

      public VideoResultFromYTSearch(VideoSearchResult videoSearchResult, TimeSpan offsetTimeSpan, int hits)
      {
         VideoSearchResult = videoSearchResult;
         OffsetTimeSpan = offsetTimeSpan;
         Hits = hits;
      }
   }

   private static async Task<Uri> SearchYoutubeFromSpotify(FullTrack fullTrack)
   {
      List<VideoResultFromYTSearch> results = new();
      YoutubeClient youtubeClient = new();

      string artists = fullTrack.Artists.Aggregate("", (current, artist) => current + " " + artist.Name);

      IReadOnlyList<VideoSearchResult> videoSearchResults = await youtubeClient.Search.GetVideosAsync($"{artists} - {fullTrack.Name} - {fullTrack.ExternalIds.Values.FirstOrDefault()}").CollectAsync(5);

      /*         foreach (VideoSearchResult result in videoSearchResults)
               {
                  if (result.Title.ToLower() == fullTrack.Name.ToLower() && fullTrack.Artists.Any(x => x.Name.ToLower() == result.Author.ChannelTitle.Replace(" - Topic", "").ToLower()))
                  {
                     results.Add(new VideoResultFromYTSearch(result, new TimeSpan(0), 0));
                  }
               }*/

      try
      {
         results.Add(new VideoResultFromYTSearch(youtubeClient.Search.GetVideosAsync($"{artists} {fullTrack.Name}").CollectAsync(1).Result[0], new TimeSpan(0), 0));
      }
      catch
      {
      }

      try
      {
         results.Add(new VideoResultFromYTSearch(youtubeClient.Search.GetVideosAsync($"{fullTrack.ExternalIds.Values.FirstOrDefault()}").CollectAsync(1).Result[0], new TimeSpan(0), 0));
      }
      catch
      {
      }

      TimeSpan t1 = TimeSpan.FromMilliseconds(fullTrack.DurationMs);
      foreach (VideoResultFromYTSearch result in results)
      {
         if (result.VideoSearchResult.Title.ToLower().Contains(fullTrack.Name.ToLower()))
            result.Hits++;

         foreach (SimpleArtist artist in fullTrack.Artists)
         {
            if (result.VideoSearchResult.Author.ChannelTitle.ToLower().Contains(artist.Name.ToLower()))
               result.Hits++;

            if (result.VideoSearchResult.Title.ToLower().Contains(artist.Name.ToLower()))
               result.Hits++;
         }

         TimeSpan t2 = TimeSpan.FromMilliseconds(result.VideoSearchResult.Duration.Value.TotalMilliseconds);
         result.OffsetTimeSpan = t2 - t1;
      }

      results.Sort((ps1, ps2) => TimeSpan.Compare(ps1.OffsetTimeSpan, ps2.OffsetTimeSpan));

      results.FirstOrDefault().Hits++;

      results.OrderBy(search => search.Hits);

      DiscordEmbedBuilder discordEmbedBuilder = new()
      {
         Title = $"Spotify <{fullTrack.ExternalIds.Values.FirstOrDefault()}>"
      };
      discordEmbedBuilder.AddField(new DiscordEmbedField($"{t1:mm\\:ss}   |   {artists}   -   {fullTrack.Name}", fullTrack.ExternalUrls.Values.FirstOrDefault()));

      int i = 1;
      foreach (VideoResultFromYTSearch result in results)
      {
         discordEmbedBuilder.AddField(new DiscordEmbedField($"Result number {i}", $"{result.Hits} hints   |   {result.OffsetTimeSpan:mm\\:ss} TimeSpanOffset"));

         discordEmbedBuilder.AddField(new DiscordEmbedField("" + $"{TimeSpan.FromMilliseconds(result.VideoSearchResult.Duration.Value.TotalMilliseconds):mm\\:ss}   |   " + $"{result.VideoSearchResult.Author.ChannelTitle}   -   " + $"{result.VideoSearchResult.Title}", result.VideoSearchResult.Url));
         i++;
      }

      await Bot.DebugDiscordChannel.SendMessageAsync(discordEmbedBuilder.Build());

      return new Uri(results.FirstOrDefault().VideoSearchResult.Url);
   }

   private static Task PlayQueueAsyncTask(InteractionContext interactionContext, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel discordChannel, Uri youtubeUri, Uri spotifyUri)
   {
      CancellationTokenSource tokenSource = new();
      CancellationToken cancellationToken = tokenSource.Token;
      DcCancellationTokenItem dcCancellationTokenKeyPair = new(discordGuild, tokenSource);
      CancellationTokenItemList.Add(dcCancellationTokenKeyPair);

      QueueItem queueItem = new(discordGuild, youtubeUri, spotifyUri);

      QueueItemList.Add(queueItem);

      try
      {
         Task.Run(() => PlayFromQueueAsyncTask(interactionContext, Bot.DiscordClient, discordGuild, discordMember, discordChannel, queueItem, cancellationToken, true), cancellationToken);
      }
      catch
      {
         CancellationTokenItemList.Remove(dcCancellationTokenKeyPair);
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
      DiscordMessage initialDiscordMessage = null;
      if (isInitialMessage)
         if (interactionContext != null)
            try
            {
               if (QueueCreatingList.Count > 0)
               {
                  int queueAmount = QueueCreatingList.Find(x => x.DiscordGuild == discordGuild)!.QueueAmount;
                  if (queueAmount > 1)
                     initialDiscordMessage = await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Queuing up {queueAmount} titles, please be patient! {voiceNextConnection.TargetChannel.Mention}!"));
                  else
                     initialDiscordMessage = await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Queuing up title/s, please be patient! {voiceNextConnection.TargetChannel.Mention}!"));
               }
            }
            catch
            {
               //prob. deleted while searching
               initialDiscordMessage = await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Queue is being created! {voiceNextConnection.TargetChannel.Mention}!"));
            }

      VoiceTransmitSink voiceTransmitSink = voiceNextConnection.GetTransmitSink();
      voiceTransmitSink.VolumeModifier = 0.2;

      try
      {
         QueueItemList.Remove(queueItem);

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

         DiscordEmbedBuilder discordEmbedBuilder = new();

         if (queueItem.IsYouTube && !queueItem.IsSpotify)
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField("YouTube", $"[[-🔗-]({queueItem.YouTubeUri.AbsoluteUri})]", true));
         }
         else if (queueItem.IsYouTube && queueItem.IsSpotify)
         {
            discordEmbedBuilder.AddField(new DiscordEmbedField("Spotify", $"[[-🔗-]({queueItem.SpotifyUri.AbsoluteUri})]", true));
            discordEmbedBuilder.AddField(new DiscordEmbedField("YouTube", $"[[-🔗-]({queueItem.YouTubeUri.AbsoluteUri})]", true));
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
            await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().WithContent($"{audioDownload.ErrorOutput[1]} `{queueItem.YouTubeUri.AbsoluteUri}`"));
         }
         else
         {
            discordEmbedBuilder = CustomDiscordEmbedBuilder(discordEmbedBuilder, queueItem, new Uri(audioDownload.Data), audioDownloadMetaData, null);
            DiscordMessage discordMessage = await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponent).AddEmbed(discordEmbedBuilder.Build()));

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
               PlayingStreamList.Add(new PlayingStream(discordGuild, ffmpegStream));
               Task ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink);

               int timeSpanAdvanceInt = 0;
               bool didOnce = false;
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

                  if (!QueueCreatingList.Exists(x => x.DiscordGuild == discordGuild) && !didOnce)
                  {
                     if (initialDiscordMessage != null)
                        await initialDiscordMessage.ModifyAsync("Queue generation is complete!");

                     discordComponent[2] = new DiscordButtonComponent(ButtonStyle.Success, "ShuffleStream", "Shuffle!", false, discordComponentEmojisShuffle);
                     discordComponent[3] = new DiscordButtonComponent(ButtonStyle.Secondary, "ShowQueueStream", "Show queue!", false, discordComponentEmojisQueue);
                     didOnce = true;
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

            if (!cancellationToken.IsCancellationRequested) CancellationTokenItemList.RemoveAll(x => x.CancellationTokenSource.Token == cancellationToken && x.DiscordGuild == discordGuild);
         }
      }
      catch (Exception exc)
      {
         DiscordChannel debugLog = await discordClient.GetChannelAsync(928938948221366334);
         await debugLog.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(exc.ToString())));
         await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Something went wrong!\n")));

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
               await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Queue is empty!")));

               List<CancellationTokenSource> cancellationTokenSourceList = new();

               foreach (DcCancellationTokenItem item in CancellationTokenItemList.Where(x => x.DiscordGuild == discordGuild)) cancellationTokenSourceList.Add(item.CancellationTokenSource);
               CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == discordGuild);

               foreach (CancellationTokenSource item in cancellationTokenSourceList)
               {
                  item.Cancel();
                  item.Dispose();
               }

               voiceNextConnection.Disconnect();
            }

            foreach (QueueItem queueListItem in QueueItemList)
               if (queueListItem.DiscordGuild == discordGuild)
               {
                  CancellationTokenSource cancellationTokenSource = new();
                  CancellationToken token = cancellationTokenSource.Token;
                  CancellationTokenItemList.Add(new DcCancellationTokenItem(discordGuild, cancellationTokenSource));

                  if (interactionContext != null)
                     Task.Run(() => PlayFromQueueAsyncTask(interactionContext, interactionContext.Client, interactionContext.Guild, interactionContext.Client.CurrentUser.ConvertToMember(interactionContext.Guild).Result, interactionContext.Channel, queueListItem, token, false));
                  else
                     Task.Run(() => PlayFromQueueAsyncTask(interactionContext, discordClient, discordGuild, discordMember, interactionChannel, queueListItem, token, false));
                  break;
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

   public static string TimeLineStringBuilderAfterTrack(int timeSpanAdvanceInt, TimeSpan totalTimeSpan, CancellationToken cancellationToken)
   {
      TimeSpan playerAdvanceTimeSpan = TimeSpan.FromSeconds(timeSpanAdvanceInt);

      string durationString = playerAdvanceTimeSpan.Hours != 0 ? $"{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}" : $"{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}";

      if (!cancellationToken.IsCancellationRequested) return $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";

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

   private static string PlayerAdvance(int timeSpanAdvanceInt, TimeSpan totalTimeSpan)
   {
      string[] strings = new string[15];
      string playerAdvanceString = "";

      double thisIsOneHundredPercent = totalTimeSpan.TotalSeconds;
      double dotPositionInPercent = 100.0 / thisIsOneHundredPercent * timeSpanAdvanceInt;
      double dotPositionInInt = 15.0 / 100.0 * dotPositionInPercent;

      for (int i = 0; i < strings.Length; i++)
         if (Convert.ToInt32(dotPositionInInt) == i)
            strings[i] = "🔘";
         else
            strings[i] = "▬";

      foreach (string item in strings) playerAdvanceString += item;

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

   public static DiscordEmbedBuilder CustomDiscordEmbedBuilder(DiscordEmbedBuilder discordEmbedBuilder, QueueItem queueItem, Uri filePathUri, VideoData audioDownloadMetaData, File metaTagFileToPlay)
   {
      if (metaTagFileToPlay == null)
      {
         bool needThumbnail = true;
         bool needAlbum = true;
         string albumTitle = "";
         string recordingMbId = "";
         discordEmbedBuilder.Title = audioDownloadMetaData.Title;
         discordEmbedBuilder.WithAuthor(audioDownloadMetaData.Creator);
         discordEmbedBuilder.WithUrl(queueItem.YouTubeUri.AbsoluteUri);

         if (queueItem.IsSpotify)
         {
            SpotifyClient spotifyClient = GetSpotifyClientConfig();
            string trackId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(queueItem.SpotifyUri.AbsoluteUri, "/track/", "/track/".Length), "?si", 0);
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
                  albumTitle = rightAlbum.Title;
            }

            recordingMbId = acoustIdRoot.Results[0].Recordings[0].Id;
            IRecording iRecording = new Query().LookupRecordingAsync(new Guid(recordingMbId)).Result;

            string genres = "";
            if (iRecording.Genres != null)
               foreach (char genre in genres)
               {
                  genres += genre;

                  if (genres.Last() != genre)
                     genres += ", ";
               }

            if (genres != "")
               discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));

            if (rightAlbum.Id != null && needThumbnail)
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

         if (needThumbnail)
         {
            discordEmbedBuilder.WithThumbnail(audioDownloadMetaData.Thumbnails[18].Url);

            Bitmap bitmapAlbumCover = new(new HttpClient().GetStreamAsync(audioDownloadMetaData.Thumbnails[18].Url).Result);
            Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
            discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
         }

         if (recordingMbId != "")
            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainz", $"[[-🔗-](https://musicbrainz.org/recording/{recordingMbId})]", true));
         if (albumTitle != "")
            discordEmbedBuilder.AddField(new DiscordEmbedField("Album", albumTitle, true));
         discordEmbedBuilder.AddField(new DiscordEmbedField("Uploader", audioDownloadMetaData.Uploader, true));
      }
      else
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

   [SlashCommand("DrivePlay" + Bot.isDevBot, "Just plays some random music!")]
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
         CancellationToken cancellationToken = tokenSource.Token;
         DcCancellationTokenItem dcCancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
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
            interactionContext.Client.Logger.LogError(exc.Message);
         else
            discordClient.Logger.LogError(exc.Message);
      }
   }

   [SlashCommand("Stop" + Bot.isDevBot, "Stop the music!")]
   private async Task DriveStopCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

      if (interactionContext.Member.VoiceState == null)
      {
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be connected!"));
         return;
      }

      await StopMusicTask(interactionContext, true, interactionContext.Client, interactionContext.Guild, interactionContext.Channel);
   }

   [SlashCommand("Skip" + Bot.isDevBot, "Skip this track!")]
   private async Task SkipCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
      await NextTrackTask(interactionContext);
   }

   [SlashCommand("Next" + Bot.isDevBot, "Skip this track!")]
   private async Task NextCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
      await NextTrackTask(interactionContext);
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
      if (QueueCreatingList.Exists(x => x.DiscordGuild == discordMessage.Channel.Guild))
      {
         await discordMessage.ModifyAsync(x => x.WithContent("Queue is being created! " + $"{QueueCreatingList.Find(queueCreating => queueCreating.DiscordGuild == discordMessage.Channel.Guild)!.QueueAddedAmount}/" + $"{QueueCreatingList.Find(queueCreating => queueCreating.DiscordGuild == discordMessage.Channel.Guild)!.QueueAmount} Please wait!"));
      }
      else
      {
         List<QueueItem> queueItemListMixed = new();
         List<QueueItem> queueItemList = PlayedQueueItemList.FindAll(x => x.DiscordGuild == discordMessage.Channel.Guild);

         int queueLength = queueItemList.Count;
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

         foreach (int randomInt in intListMixed) queueItemListMixed.Add(queueItemList[randomInt]);

         QueueItemList.RemoveAll(x => x.DiscordGuild == discordMessage.Channel.Guild);
         foreach (QueueItem queueItem in queueItemListMixed) QueueItemList.Add(queueItem);

         PlayedQueueItemList.RemoveAll(x => x.DiscordGuild == discordMessage.Channel.Guild);
         foreach (QueueItem queueItem in queueItemListMixed) PlayedQueueItemList.Add(queueItem);

         if (queueItemListMixed.Count == 0)
            await discordMessage.ModifyAsync(x => x.WithContent("Its late i have to leave!"));
         else
            await discordMessage.ModifyAsync(x => x.WithContent("Queue has been altered!"));
      }
   }

   public static async Task API_ShufflePlaylist(API aPI)
   {
      DiscordGuild discordGuild = default;
      DiscordMember interactionDiscordMember = default;
      foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
         foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.VoiceState != null && x.Id == aPI.RequestDiscordUserId))
         {
            interactionDiscordMember = memberItem;
            discordGuild = guildItem;
            break;
         }

      if (interactionDiscordMember == null)
      {
         foreach (DiscordGuild guildItem in Bot.DiscordClient.Guilds.Values)
            foreach (DiscordMember memberItem in guildItem.Members.Values.Where(x => x.Id == aPI.RequestDiscordUserId))
            {
               await memberItem.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected!")));
               return;
            }
      }
      if (interactionDiscordMember == null)
         return;

      if (QueueCreatingList.Exists(x => x.DiscordGuild == discordGuild))
      {
         await interactionDiscordMember.VoiceState.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Queue is being created! " + $"{QueueCreatingList.Find(queueCreating => queueCreating.DiscordGuild == discordGuild)!.QueueAddedAmount}/" + $"{QueueCreatingList.Find(queueCreating => queueCreating.DiscordGuild == discordGuild)!.QueueAmount} Please wait!")));
      }
      else
      {
         List<QueueItem> queueItemListMixed = new();
         List<QueueItem> queueItemList = PlayedQueueItemList.FindAll(x => x.DiscordGuild == discordGuild);

         int queueLength = queueItemList.Count;
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

         foreach (int randomInt in intListMixed) queueItemListMixed.Add(queueItemList[randomInt]);

         QueueItemList.RemoveAll(x => x.DiscordGuild == discordGuild);
         foreach (QueueItem queueItem in queueItemListMixed) QueueItemList.Add(queueItem);

         PlayedQueueItemList.RemoveAll(x => x.DiscordGuild == discordGuild);
         foreach (QueueItem queueItem in queueItemListMixed) PlayedQueueItemList.Add(queueItem);

         if (queueItemListMixed.Count == 0)
            await interactionDiscordMember.VoiceState.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Its late i have to leave!")));
         else
            await interactionDiscordMember.VoiceState.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Green).WithDescription("Queue has been altered!")));
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

      await StopMusicTask(null, false, interactionContext.Client, interactionContext.Guild, interactionContext.Channel);

      CancellationTokenSource tokenSource = new();
      CancellationToken cancellationToken = tokenSource.Token;
      DcCancellationTokenItem dcCancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
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

               if (QueueItemList.Any(x => x.DiscordGuild == eventArgs.Guild))
               {
                  eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("'Youtube' music is running! This interaction is locked!")));
                  return Task.CompletedTask;
               }

               bool nothingToStop = true;
               List<CancellationTokenSource> cancellationTokenSourceList = new();
               foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
               {
                  nothingToStop = false;
                  cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
               }

               CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

               foreach (CancellationTokenSource cancellationTokenItem in cancellationTokenSourceList)
               {
                  cancellationTokenItem.Cancel();
                  cancellationTokenItem.Dispose();
               }

               QueueItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);
               QueueCreatingList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

               eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(nothingToStop ? "Nothing to stop!" : "Stopped the music!")));

               CancellationTokenSource tokenSource = new();
               CancellationToken cancellationToken = tokenSource.Token;
               DcCancellationTokenItem dcCancellationTokenKeyPair = new(eventArgs.Guild, tokenSource);
               CancellationTokenItemList.Add(dcCancellationTokenKeyPair);

               try
               {
                  Task.Run(() => DrivePlayTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, cancellationToken, false), cancellationToken);
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

               StopMusicTask(null, true, client, eventArgs.Guild, eventArgs.Channel);

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
                  int indexOfPlaying = PlayedQueueItemList.FindIndex(x => x.DiscordGuild == queueItem.DiscordGuild && ((x.SpotifyUri == queueItem.SpotifyUri && x.SpotifyUri != null && queueItem.SpotifyUri != null) || (x.YouTubeUri == queueItem.YouTubeUri && x.YouTubeUri != null && queueItem.YouTubeUri != null))) - 1;

                  int indexOfLast = PlayedQueueItemList.FindIndex(x => x.DiscordGuild == queueItem.DiscordGuild && ((x.SpotifyUri == queueItem.SpotifyUri && x.SpotifyUri != null && queueItem.SpotifyUri != null) || (x.YouTubeUri == queueItem.YouTubeUri && x.YouTubeUri != null && queueItem.YouTubeUri != null))) - 2;

                  if (indexOfLast >= 0)
                  {
                     QueueItemList.Insert(0, PlayedQueueItemList.ElementAt(indexOfPlaying));
                     QueueItemList.Insert(0, PlayedQueueItemList.ElementAt(indexOfLast));
                  }
                  else
                  {
                     eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Wrong universe!")));
                     return Task.CompletedTask;
                  }

                  break;
               }

               List<CancellationTokenSource> cancellationTokenSourceList = new();
               foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild)) cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);

               CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

               foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
               {
                  cancellationToken.Cancel();
                  cancellationToken.Dispose();
               }

               CancellationTokenSource newCancellationTokenSource = new();
               CancellationToken newCancellationToken = newCancellationTokenSource.Token;

               foreach (QueueItem queueItem in QueueItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
               {
                  DcCancellationTokenItem newDcCancellationTokenItem = new(eventArgs.Guild, newCancellationTokenSource);
                  CancellationTokenItemList.Add(newDcCancellationTokenItem);

                  Task.Run(() => PlayFromQueueAsyncTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, queueItem, newCancellationToken, false), newCancellationToken);
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

               if (QueueItemList.All(x => x.DiscordGuild != eventArgs.Guild))
               {
                  eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("Nothing to skip!")));
                  return Task.CompletedTask;
               }

               List<CancellationTokenSource> cancellationTokenSourceList = new();
               foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild)) cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);

               CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

               foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
               {
                  cancellationToken.Cancel();
                  cancellationToken.Dispose();
               }

               CancellationTokenSource newCancellationTokenSource = new();
               CancellationToken newCancellationToken = newCancellationTokenSource.Token;

               foreach (QueueItem queueItem in QueueItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
               {
                  DcCancellationTokenItem newDcCancellationTokenItem = new(eventArgs.Guild, newCancellationTokenSource);
                  CancellationTokenItemList.Add(newDcCancellationTokenItem);

                  Task.Run(() => PlayFromQueueAsyncTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, queueItem, newCancellationToken, false), newCancellationToken);
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

               StopMusicTask(null, true, client, eventArgs.Guild, eventArgs.Channel);

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

               DiscordMessage discordMessage = eventArgs.Channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(("Shuffle requested!")))).Result;

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
                        break;

                     Video videoData = youtubeClient.Videos.GetAsync(queueItemList[i].YouTubeUri.AbsoluteUri).Result;

                     if (queueItemList[i].IsSpotify)
                        descriptionString += "[🔗[YouTube]" + $"({queueItemList[i].YouTubeUri.AbsoluteUri})] " + "[🔗[Spotify]" + $"({queueItemList[i].SpotifyUri.AbsoluteUri})]  " + videoData.Title + " - " + videoData.Author + "\n";
                     else
                        descriptionString += "[🔗[YouTube]" + $"({queueItemList[i].YouTubeUri.AbsoluteUri})] " + videoData.Title + " - " + videoData.Author + "\n";
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

   private static Task StopMusicTask(InteractionContext interactionContext, bool sendStopped, DiscordClient client, DiscordGuild discordGuild, DiscordChannel discordChannel)
   {
      bool nothingToStop = true;
      List<CancellationTokenSource> cancellationTokenSourceList = new();
      foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == discordGuild))
      {
         nothingToStop = false;
         cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
      }

      CancellationTokenItemList.RemoveAll(x => x.DiscordGuild == discordGuild);

      foreach (CancellationTokenSource cancellationToken in cancellationTokenSourceList)
      {
         cancellationToken.Cancel();
         cancellationToken.Dispose();
      }

      QueueItemList.RemoveAll(x => x.DiscordGuild == discordGuild);
      QueueCreatingList.RemoveAll(x => x.DiscordGuild == discordGuild);

      if (interactionContext != null)
         interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(nothingToStop ? "Nothing to stop!" : "Stopped the music!"));
      else if (sendStopped)
         discordChannel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription(nothingToStop ? "Nothing to stop!" : "Stopped the music!")));

      if (client != null)
         try
         {
            VoiceNextExtension voiceNext = client.GetVoiceNext();
            VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(interactionContext != null ? interactionContext.Guild : discordGuild);
            if (voiceNextConnection != null)
               voiceNextConnection.Disconnect();
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
            if (eventArgs.User == client.CurrentUser && eventArgs.After != null && eventArgs.Before.Channel != eventArgs.After.Channel)
            {
               bool nothingToStop = true;
               List<CancellationTokenSource> cancellationTokenSourceList = new();
               foreach (DcCancellationTokenItem cancellationTokenItem in CancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
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

               QueueItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);
               QueueCreatingList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

               //eventArgs.Channel.SendMessageAsync(nothingToStop ? "Nothing to stop!" : "Stopped the music!");
               VoiceNextExtension voiceNext = client.GetVoiceNext();
               VoiceNextConnection voiceNextConnection = voiceNext.GetConnection(eventArgs.Guild);
               if (voiceNextConnection != null)
                  voiceNextConnection.Disconnect();
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
            if (eventArgs.User == client.CurrentUser)
               await StopMusicTask(null, false, client, eventArgs.Guild, eventArgs.Before.Channel);
      }
      catch
      {
         // ignored
      }
   }
}