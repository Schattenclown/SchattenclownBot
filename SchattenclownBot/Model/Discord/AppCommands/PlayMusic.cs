// Copyright (c) Schattenclown

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

using Microsoft.Extensions.Logging;

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

// ReSharper disable UnusedMember.Local
#pragma warning disable CS4014

// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands;

internal class QueueItem
{
	public DiscordGuild DiscordGuild { get; set; }
	public Uri YouTubeUri { get; set; }
	public Uri SpotifyUri { get; set; }
	public bool IsYouTube { get; set; }
	public bool IsSpotify { get; set; }

	internal QueueItem(DiscordGuild discordGuild, Uri youTubeUri, Uri spotifyUri)
	{
		this.DiscordGuild = discordGuild;
		this.YouTubeUri = youTubeUri;
		this.SpotifyUri = spotifyUri;
		this.IsYouTube = this.YouTubeUri != null;
		this.IsSpotify = this.SpotifyUri != null;
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
		this.DiscordGuild = discordGuild;
		this.CancellationTokenSource = cancellationTokenSource;
	}

	internal CancellationTokenItem()
	{

	}
}

internal class QueueCreating
{
	internal DiscordGuild DiscordGuild { get; set; }
	internal int QueueAmount { get; set; }
	internal int QueueAddedAmount { get; set; }

	internal QueueCreating(DiscordGuild discordGuild, int queueAmount, int queueAddedAmount)
	{
		this.DiscordGuild = discordGuild;
		this.QueueAmount = queueAmount;
		this.QueueAddedAmount = queueAddedAmount;
	}
}

internal class PlayingStream
{
	public DiscordGuild DiscordGuild { get; set; }
	public Stream Stream { get; set; }
	internal PlayingStream(DiscordGuild discordGuild, Stream stream)
	{
		this.DiscordGuild = discordGuild;
		this.Stream = stream;
	}
}

internal class PlayMusic : ApplicationCommandsModule
{
	private static readonly List<CancellationTokenItem> s_cancellationTokenItemList = new();
	private static readonly List<QueueCreating> s_queueCreatingList = new();
	public static readonly List<PlayingStream> PlayingStreamList = new();
	public static readonly List<QueueItem> QueueItemList = new();

	public async static void NextRequestAPI(API aPI)
	{
		CWLogger.Write(aPI.RequestTimeStamp + " " + aPI.RequesterIP + " " + aPI.RequestDiscordUserId, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.DarkYellow);
		API.DELETE(aPI.CommandRequestID);

		var discordGuild = await Bot.DiscordClient.GetGuildAsync(928930967140331590);
		var discordChannel = await Bot.DiscordClient.GetChannelAsync(928937150546853919);
		var discordUser = await Bot.DiscordClient.GetUserAsync(444152594898878474);
		var discordMember = await discordUser.ConvertToMember(discordGuild);

		if (QueueItemList.All(x => x.DiscordGuild != discordGuild))
		{
			discordChannel.SendMessageAsync("Nothing to skip!");
			return;
		}

		List<CancellationTokenSource> cancellationTokenSourceList = new();
		foreach (var cancellationTokenItem in s_cancellationTokenItemList.Where(x => x.DiscordGuild == discordGuild))
		{
			cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
		}

		s_cancellationTokenItemList.RemoveAll(x => x.DiscordGuild == discordGuild);

		foreach (var cancellationToken in cancellationTokenSourceList)
		{
			cancellationToken.Cancel();
			cancellationToken.Dispose();
		}

		CancellationTokenSource newCancellationTokenSource = new();
		var newCancellationToken = newCancellationTokenSource.Token;

		foreach (var queueItem in QueueItemList.Where(x => x.DiscordGuild == discordGuild))
		{
			CancellationTokenItem newCancellationTokenItem = new(discordGuild, newCancellationTokenSource);
			s_cancellationTokenItemList.Add(newCancellationTokenItem);
			Task.Run(() => PlayFromQueueAsyncTask(null, Bot.DiscordClient, discordGuild, discordMember, discordChannel, queueItem, false, newCancellationToken), newCancellationToken);
			break;
		}
	}

	public static async Task TestTask() =>
		//YoutubeClient youtubeClient = new YoutubeClient();
		//var something = await youtubeClient.Playlists.GetVideosAsync("RDTBQurAxh2hA");
		await Task.Delay(1000);

	private static bool NoMusicPlaying(DiscordGuild discordGuild) => s_cancellationTokenItemList.All(cancellationTokenItem => cancellationTokenItem.DiscordGuild != discordGuild);

	public static SpotifyClient GetSpotifyClientConfig()
	{
		var spotifyClientConfig = SpotifyClientConfig.CreateDefault();
		ClientCredentialsRequest clientCredentialsRequest = new(Bot.Connections.Token.ClientId, Bot.Connections.Token.ClientSecret);
		var clientCredentialsTokenResponse = new OAuthClient(spotifyClientConfig).RequestToken(clientCredentialsRequest).Result;
		SpotifyClient spotifyClient = new(clientCredentialsTokenResponse.AccessToken);
		return spotifyClient;
	}

	[SlashCommand("Play" + Bot.IS_DEV_BOT, "Play Spotify or YouTube links!")]
	private static async Task PlayCommand(InteractionContext interactionContext, [Option("Link", "Link!")] string webLink)
	{
		await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

		if (interactionContext.Member.VoiceState == null)
		{
			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be connected!"));
			return;
		}

		if (s_queueCreatingList.Exists(x => x.DiscordGuild == interactionContext.Guild))
		{
			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("The queue is already being generated. Please wait until the first queue is generated! " +
															   $"{s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount}/" +
															   $"{s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAmount} Please wait!"));
			return;
		}
		else if (!NoMusicPlaying(interactionContext.Guild))
		{
			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Queue is being created! Please be patient!"));
		}

		Uri webLinkUri;
		try
		{
			webLinkUri = new Uri(webLink);
		}
		catch
		{
			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Please check your link, something is wrong! The https:// tag may be missing"));
			return;
		}

		var isYouTube = false;
		var isYouTubePlaylist = false;
		var isYouTubePlaylistWithIndex = false;
		var isSpotify = false;
		var isSpotifyPlaylist = false;
		var isSpotifyAlbum = false;
		var tracksAdded = 0;

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
			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Sag ICH!!!"));
			return;
		}

		if (isYouTube)
		{
			try
			{
				if (isYouTubePlaylist)
				{
					var playlistSelectedVideoIndex = 1;
					var playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveAfterWord(StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "&list=", "&list=".Length), "&index=", 0), "&ab_channel=", 0), "&start_radio=", 0);
					var isYouTubeMix = webLinkUri.AbsoluteUri.Contains("&ab_channel=") || webLinkUri.AbsoluteUri.Contains("&start_radio=");
					YoutubeClient youtubeClient = new();

					s_queueCreatingList.Add(new QueueCreating(interactionContext.Guild, 0, 0));

					if (isYouTubePlaylistWithIndex)
					{
						playlistSelectedVideoIndex = Convert.ToInt32(StringCutter.RemoveAfterWord(StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "&index=", "&index=".Length), "&ab_channel=", 0), "&start_radio=", 0));
						var firstVideoId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "watch?v=", "watch?v=".Length), "&list=", 0);
						var firstVideo = await youtubeClient.Videos.GetAsync(firstVideoId);

						await PlayQueueAsyncTask(interactionContext, new Uri(firstVideo.Url), null);
						tracksAdded++;
					}

					List<PlaylistVideo> playlistVideos = new(await youtubeClient.Playlists.GetVideosAsync(playlistId));

					if (playlistSelectedVideoIndex != 1 && !isYouTubeMix)
					{
						playlistVideos.RemoveRange(0, playlistSelectedVideoIndex);
					}
					else if (isYouTubePlaylistWithIndex)
					{
						playlistVideos.RemoveRange(0, 1);
					}

					s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAmount = playlistVideos.Count;

					var startIndex = 0;
					if (NoMusicPlaying(interactionContext.Guild))
					{
						await PlayQueueAsyncTask(interactionContext, new Uri(playlistVideos[startIndex].Url), null);
						s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
						tracksAdded++;
						startIndex++;
					}

					var cancellationTokenSource = s_cancellationTokenItemList.First(x => x.DiscordGuild == interactionContext.Guild).CancellationTokenSource;

					while (startIndex < playlistVideos.Count)
					{
						try
						{
							if (cancellationTokenSource.IsCancellationRequested)
								break;

							QueueItemList.Add(new QueueItem(interactionContext.Guild, new Uri(playlistVideos[startIndex].Url), null));
							s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
							tracksAdded++;
						}
						catch (Exception ex)
						{
							await interactionContext.Channel.SendMessageAsync("Error adding " + playlistVideos[startIndex].Url + " " + ex.Message);
						}
						startIndex++;
					}
				}
				else
				{
					//https://youtu.be/EW6c8o5ctRI
					var selectedVideoId = webLink.Contains("youtu.be")
						? StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "youtu.be/", "youtu.be/".Length), "&list=", 0)
						: StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "watch?v=", "watch?v=".Length), "&list=", 0);
					Uri selectedVideoUri = new("https://www.youtube.com/watch?v=" + selectedVideoId);
					s_queueCreatingList.Add(new QueueCreating(interactionContext.Guild, 1, 0));


					if (NoMusicPlaying(interactionContext.Guild))
					{
						await PlayQueueAsyncTask(interactionContext, selectedVideoUri, null);
						s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
						tracksAdded++;
					}
					else
					{
						try
						{
							QueueItemList.Add(new QueueItem(interactionContext.Guild, selectedVideoUri, null));
							s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
							tracksAdded++;
						}
						catch (Exception ex)
						{
							await interactionContext.Channel.SendMessageAsync("Error adding " + selectedVideoUri + " " + ex.Message);
							return;
						}
					}
				}
			}
			catch (Exception ex)
			{
				s_queueCreatingList.RemoveAll(x => x.DiscordGuild == interactionContext.Guild);
				await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error!\n" + ex.Message));
			}
		}
		// ReSharper disable once ConditionIsAlwaysTrueOrFalse
		else if (isSpotify)
		{
			var spotifyClient = GetSpotifyClientConfig();

			if (isSpotifyPlaylist)
			{
				var playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/playlist/", "/playlist/".Length), "?si", 0);

				PlaylistGetItemsRequest playlistGetItemsRequest = new()
				{
					Offset = 0
				};

				var playlistTrackList = spotifyClient.Playlists.GetItems(playlistId, playlistGetItemsRequest).Result.Items;

				if (playlistTrackList.Count >= 100)
				{
					try
					{
						while (true)
						{
							playlistGetItemsRequest.Offset += 100;

							var playlistTrackListSecound = spotifyClient.Playlists.GetItems(playlistId, playlistGetItemsRequest).Result.Items;

							playlistTrackList.AddRange(playlistTrackListSecound);

							if (playlistTrackListSecound.Count < 100)
								break;
						}
					}
					catch
					{

					}
				}

				if (playlistTrackList != null)
				{
					s_queueCreatingList.Add(new QueueCreating(interactionContext.Guild, playlistTrackList.Count, 0));

					if (playlistTrackList.Count != 0)
					{
						var startIndex = 0;
						if (NoMusicPlaying(interactionContext.Guild))
						{
							var playlistTrack = playlistTrackList[startIndex].Track as FullTrack;

							try
							{
								var fullTrack = spotifyClient.Tracks.Get(playlistTrack!.Id).Result;
								var youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
								await PlayQueueAsyncTask(interactionContext, youTubeUri, new Uri("https://open.spotify.com/track/" + playlistTrack!.Id));
								s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
								tracksAdded++;
							}
							catch (Exception ex)
							{
								await interactionContext.Channel.SendMessageAsync("Error adding " + "https://open.spotify.com/track/" + playlistTrack!.Id + " " + ex.Message);
							}

							startIndex++;
						}
						var cancellationTokenSource = s_cancellationTokenItemList.First(x => x.DiscordGuild == interactionContext.Guild).CancellationTokenSource;

						while (startIndex < playlistTrackList.Count)
						{
							var playlistTrack = playlistTrackList[startIndex].Track as FullTrack;

							if (cancellationTokenSource.IsCancellationRequested)
								break;

							try
							{
								var fullTrack = spotifyClient.Tracks.Get(playlistTrack!.Id).Result;
								var youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
								QueueItemList.Add(new QueueItem(interactionContext.Guild, youTubeUri, new Uri("https://open.spotify.com/track/" + playlistTrack!.Id)));
								s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
								tracksAdded++;
							}
							catch (Exception ex)
							{
								await interactionContext.Channel.SendMessageAsync("Error adding " + "https://open.spotify.com/track/" + playlistTrack!.Id + " " + ex.Message);
							}

							startIndex++;
						}
					}
				}
			}
			else if (isSpotifyAlbum)
			{
				var albumId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(StringCutter.RemoveUntilWord(webLink, "/album/", "/album/".Length), ":album:", ":album:".Length), "?si", 0);
				var simpleTrackList = spotifyClient.Albums.GetTracks(albumId).Result.Items;

				if (simpleTrackList != null && simpleTrackList.Count != 0)
				{
					s_queueCreatingList.Add(new QueueCreating(interactionContext.Guild, simpleTrackList.Count, 0));

					var startIndex = 0;

					if (NoMusicPlaying(interactionContext.Guild))
					{
						var simpleTrack = simpleTrackList[startIndex];
						try
						{
							var fullTrack = spotifyClient.Tracks.Get(simpleTrack!.Id).Result;
							var youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
							await PlayQueueAsyncTask(interactionContext, youTubeUri, new Uri("https://open.spotify.com/track/" + simpleTrack!.Id));
							s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
							tracksAdded++;
						}
						catch (Exception ex)
						{
							await interactionContext.Channel.SendMessageAsync("Error adding " + "https://open.spotify.com/track/" + simpleTrack!.Id + " " + ex.Message);
						}

						startIndex++;
					}

					while (startIndex < simpleTrackList.Count)
					{
						var simpleTrack = simpleTrackList[startIndex];
						try
						{
							var fullTrack = spotifyClient.Tracks.Get(simpleTrack!.Id).Result;
							var youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
							QueueItemList.Add(new QueueItem(interactionContext.Guild, youTubeUri, new Uri("https://open.spotify.com/track/" + simpleTrack!.Id)));
							s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
							tracksAdded++;
						}
						catch (Exception ex)
						{
							await interactionContext.Channel.SendMessageAsync("Error adding " + "https://open.spotify.com/track/" + simpleTrack!.Id + " " + ex.Message);
						}

						startIndex++;
					}
				}
			}
			else
			{
				var trackId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(webLink, "/track/", "/track/".Length), "?si", 0);
				s_queueCreatingList.Add(new QueueCreating(interactionContext.Guild, 1, 0));

				if (NoMusicPlaying(interactionContext.Guild))
				{
					var fullTrack = spotifyClient.Tracks.Get(trackId).Result;

					try
					{
						var youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
						await PlayQueueAsyncTask(interactionContext, youTubeUri, new Uri("https://open.spotify.com/track/" + trackId));
						s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
						tracksAdded++;
					}
					catch (Exception ex)
					{
						await interactionContext.Channel.SendMessageAsync("Error adding " + "https://open.spotify.com/track/" + fullTrack!.Id + " " + ex.Message);
					}

				}
				else
				{
					var fullTrack = spotifyClient.Tracks.Get(trackId).Result;

					try
					{
						var youTubeUri = await SearchYoutubeFromSpotify(fullTrack);
						QueueItemList.Add(new QueueItem(interactionContext.Guild, youTubeUri, new Uri("https://open.spotify.com/track/" + trackId)));
						s_queueCreatingList.Find(x => x.DiscordGuild == interactionContext.Guild)!.QueueAddedAmount++;
						tracksAdded++;
					}
					catch (Exception ex)
					{
						await interactionContext.Channel.SendMessageAsync("Error adding " + "https://open.spotify.com/track/" + fullTrack!.Id + " " + ex.Message);
					}
				}
			}
		}

		s_queueCreatingList.RemoveAll(x => x.DiscordGuild == interactionContext.Guild);
		await Task.Delay(500);

		if (NoMusicPlaying(interactionContext.Guild))
		{
			if (tracksAdded == 1)
				await interactionContext.Channel.SendMessageAsync($"{tracksAdded} track is now added to the queue!");
			else
				await interactionContext.Channel.SendMessageAsync($"{tracksAdded} tracks are now added to the queue!");
		}
		else
		{
			if (tracksAdded == 1)
				await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Music is already playing or will at any moment! {tracksAdded} track is now added to the queue!"));
			else
				await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Music is already playing or will at any moment! {tracksAdded} tracks are now added to the queue!"));
		}
	}

	private static async Task<Uri> SearchYoutubeFromSpotify(FullTrack fullTrack)
	{
		YoutubeClient youtubeClient = new();

		var artists = fullTrack.Artists.Aggregate("", (current, artist) => current + artist.Name);

		var videoSearchResults = await youtubeClient.Search.GetVideosAsync($"{artists} - {fullTrack.Name} - {fullTrack.ExternalIds.Values.FirstOrDefault()}").CollectAsync(5);
		VideoSearchResult rightItem = null;
		foreach (var item in videoSearchResults)
		{
			if (item.Title == fullTrack.Name && fullTrack.Artists.Any(x => x.Name == item.Author.ChannelTitle))
			{
				rightItem = item;
			}
		}

		if (rightItem != null)
			return new Uri(rightItem.Url);

		if (videoSearchResults.Count == 0)
		{
			videoSearchResults = await youtubeClient.Search.GetVideosAsync($"{artists} - {fullTrack.Name}").CollectAsync(3);
		}

		return new Uri(videoSearchResults[0].Url);
	}

	private static Task PlayQueueAsyncTask(InteractionContext interactionContext, Uri youtubeUri, Uri spotifyUri)
	{
		CancellationTokenSource tokenSource = new();
		var cancellationToken = tokenSource.Token;
		CancellationTokenItem cancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
		s_cancellationTokenItemList.Add(cancellationTokenKeyPair);

		QueueItem queueItem = new(interactionContext.Guild, youtubeUri, spotifyUri);

		QueueItemList.Add(queueItem);

		try
		{
			Task.Run(() => PlayFromQueueAsyncTask(interactionContext, null, null, null, null, queueItem, true, cancellationToken), cancellationToken);
		}
		catch
		{
			s_cancellationTokenItemList.Remove(cancellationTokenKeyPair);
		}

		return Task.CompletedTask;
	}

	private static async Task PlayFromQueueAsyncTask(InteractionContext interactionContext, DiscordClient discordClient, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionChannel, QueueItem queueItem, bool isInitialMessage, CancellationToken cancellationToken)
	{
		discordGuild ??= interactionContext.Guild;
		discordClient ??= interactionContext.Client;
		interactionChannel ??= interactionContext.Channel;

		var voiceNext = discordClient.GetVoiceNext();
		if (voiceNext == null)
			return;

		var voiceNextConnection = voiceNext.GetConnection(discordGuild);
		var voiceState = interactionContext != null ? interactionContext.Member?.VoiceState : discordMember?.VoiceState;
		if (voiceState?.Channel == null)
			return;

		voiceNextConnection ??= await voiceNext.ConnectAsync(voiceState.Channel);
		DiscordMessage initialDiscordMessage = null;
		if (isInitialMessage)
		{
			if (interactionContext != null)
			{
				try
				{
					if (s_queueCreatingList.Count > 0)
					{
						var queueAmount = s_queueCreatingList.Find(x => x.DiscordGuild == discordGuild)!.QueueAmount;
						initialDiscordMessage = queueAmount > 1
							? await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Queuing up {queueAmount} titles, please be patient! {voiceNextConnection.TargetChannel.Mention}!"))
							: await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Queuing up title/s, please be patient! {voiceNextConnection.TargetChannel.Mention}!"));
					}
				}
				catch
				{
					//prob. deleted while searching
					initialDiscordMessage = await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Queue is being created! {voiceNextConnection.TargetChannel.Mention}!"));
				}
			}
		}

		var voiceTransmitSink = voiceNextConnection.GetTransmitSink();
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
			var audioDownload = await youtubeDl.RunAudioDownload(queueItem.YouTubeUri.AbsoluteUri, AudioConversionFormat.Mp3, new CancellationToken(), null, null, optionSet);
			var audioDownloadMetaData = youtubeDl.RunVideoDataFetch(queueItem.YouTubeUri.AbsoluteUri).Result.Data;
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

			DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
			DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
			DiscordComponentEmoji discordComponentEmojisShuffle = new("🔀");
			DiscordComponentEmoji discordComponentEmojisQueue = new("⏬");
			var discordComponents = new DiscordComponent[4];
			discordComponents[0] = new DiscordButtonComponent(ButtonStyle.Primary, "next_song_stream", "Next!", false, discordComponentEmojisNext);
			discordComponents[1] = new DiscordButtonComponent(ButtonStyle.Danger, "stop_song_stream", "Stop!", false, discordComponentEmojisStop);
			discordComponents[2] = new DiscordButtonComponent(ButtonStyle.Success, "shuffle_stream", "Shuffle!", false, discordComponentEmojisShuffle);
			discordComponents[3] = new DiscordButtonComponent(ButtonStyle.Secondary, "showQueue_stream", "Show queue!", false, discordComponentEmojisQueue);

			if (audioDownload.ErrorOutput.Length > 1)
			{
				await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().WithContent($"{audioDownload.ErrorOutput[1]} `{queueItem.YouTubeUri.AbsoluteUri}`"));
			}
			else
			{
				discordEmbedBuilder = CustomDiscordEmbedBuilder(discordEmbedBuilder, queueItem, new Uri(audioDownload.Data), audioDownloadMetaData, null);
				var discordMessage = await interactionChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()));

				ProcessStartInfo ffmpegProcessStartInfo = new()
				{
					FileName = "..\\..\\..\\Model\\Executables\\ffmpeg\\ffmpeg.exe",
					Arguments = $@"-i ""{audioDownload.Data}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
					RedirectStandardOutput = true,
					UseShellExecute = false
				};
				var ffmpegProcess = Process.Start(ffmpegProcessStartInfo);
				if (ffmpegProcess != null)
				{
					var ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;
					PlayingStreamList.Add(new PlayingStream(discordGuild, ffmpegStream));
					var ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink, cancellationToken: cancellationToken);

					var timeSpanAdvanceInt = 0;
					var didOnce = false;
					while (!ffmpegCopyTask.IsCompleted)
					{
						await Task.Delay(1000, cancellationToken);

						try
						{
							if (timeSpanAdvanceInt % 10 == 0)
							{
								discordEmbedBuilder.Description = TimeLineStringBuilderWhilePlaying(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);
								await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()));
							}
						}
						catch (Exception ex)
						{
							CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
						}

						if (cancellationToken.IsCancellationRequested)
						{
							ffmpegStream.Close();
							break;
						}

						if (!s_queueCreatingList.Exists(x => x.DiscordGuild == discordGuild) && !didOnce)
						{
							if (initialDiscordMessage != null)
								await initialDiscordMessage.ModifyAsync("Queue generation is complete!");

							discordComponents[2] = new DiscordButtonComponent(ButtonStyle.Success, "shuffle_stream", "Shuffle!", false, discordComponentEmojisShuffle);
							discordComponents[3] = new DiscordButtonComponent(ButtonStyle.Secondary, "showQueue_stream", "Show queue!", false, discordComponentEmojisQueue);
							didOnce = true;
						}

						timeSpanAdvanceInt++;
					}

					discordComponents[0] = new DiscordButtonComponent(ButtonStyle.Primary, "next_song_stream", "Skipped!", true, discordComponentEmojisNext);
					discordComponents[1] = new DiscordButtonComponent(ButtonStyle.Danger, "stop_song_stream", "Stop!", true, discordComponentEmojisStop);
					discordComponents[2] = new DiscordButtonComponent(ButtonStyle.Success, "shuffle_stream", "Shuffle!", true, discordComponentEmojisShuffle);
					discordComponents[3] = new DiscordButtonComponent(ButtonStyle.Secondary, "showQueue_stream", "Show queue!", true, discordComponentEmojisQueue);

					discordEmbedBuilder.Description = TimeLineStringBuilderAfterSong(timeSpanAdvanceInt, audioDownloadTimeSpan, cancellationToken);
					await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).AddEmbed(discordEmbedBuilder.Build()));
				}

				if (!cancellationToken.IsCancellationRequested)
				{
					s_cancellationTokenItemList.RemoveAll(x => x.CancellationTokenSource.Token == cancellationToken && x.DiscordGuild == discordGuild);
				}
			}
		}
		catch (Exception exc)
		{
			var debug_log = await discordClient.GetChannelAsync(928938948221366334);
			var something = await debug_log.SendMessageAsync(exc.ToString());
			await interactionChannel.SendMessageAsync("Something went wrong!\n");

			if (interactionContext != null)
				interactionContext.Client.Logger.LogError("{msg}", exc.Message);
			else
				discordClient.Logger.LogError("{msg}", exc.Message);
		}
		finally
		{
			await voiceTransmitSink.FlushAsync(cancellationToken);

			if (!cancellationToken.IsCancellationRequested)
			{
				if (QueueItemList.All(x => x.DiscordGuild != discordGuild))
				{
					await interactionChannel.SendMessageAsync("Queue is empty!");

					List<CancellationTokenSource> cancellationTokenSourceList = new();

					foreach (var item in s_cancellationTokenItemList.Where(x => x.DiscordGuild == discordGuild))
					{
						cancellationTokenSourceList.Add(item.CancellationTokenSource);
					}
					s_cancellationTokenItemList.RemoveAll(x => x.DiscordGuild == discordGuild);

					foreach (var item in cancellationTokenSourceList)
					{
						item.Cancel();
						item.Dispose();
					}

					voiceNextConnection.Disconnect();
				}

				foreach (var queueListItem in QueueItemList)
				{
					if (queueListItem.DiscordGuild == discordGuild)
					{
						CancellationTokenSource cancellationTokenSource = new();
						var token = cancellationTokenSource.Token;
						s_cancellationTokenItemList.Add(new CancellationTokenItem(discordGuild, cancellationTokenSource));

						if (interactionContext != null)
							Task.Run(() => PlayFromQueueAsyncTask(interactionContext, interactionContext.Client, interactionContext.Guild, interactionContext.Client.CurrentUser.ConvertToMember(interactionContext.Guild).Result,
							   interactionContext.Channel, queueListItem, false, token), cancellationToken);
						else
							Task.Run(() => PlayFromQueueAsyncTask(interactionContext, discordClient, discordGuild, discordMember, interactionChannel, queueListItem, false, token), cancellationToken);
						break;
					}
				}
			}
		}
	}

	public static string TimeLineStringBuilderWhilePlaying(int timeSpanAdvanceInt, TimeSpan totalTimeSpan, CancellationToken cancellationToken)
	{
		var playerAdvanceTimeSpan = TimeSpan.FromSeconds(timeSpanAdvanceInt);

		var playerAdvanceString = PlayerAdvance(timeSpanAdvanceInt, totalTimeSpan);

		var descriptionString = "⏹️";
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
		var playerAdvanceTimeSpan = TimeSpan.FromSeconds(timeSpanAdvanceInt);

		var durationString = playerAdvanceTimeSpan.Hours != 0 ? $"{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}" : $"{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}";

		if (!cancellationToken.IsCancellationRequested)
		{
			return $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
		}
		else
		{
			var descriptionString = "⏹️";
			if (cancellationToken.IsCancellationRequested)
				descriptionString = "▶️";
			var playerAdvanceString = PlayerAdvance(timeSpanAdvanceInt, totalTimeSpan);

			if (playerAdvanceTimeSpan.Hours != 0)
				descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Hours:#00}:{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Hours:#00}:{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";
			else
				descriptionString += $" {playerAdvanceString} [{playerAdvanceTimeSpan.Minutes:#00}:{playerAdvanceTimeSpan.Seconds:#00}/{totalTimeSpan.Minutes:#00}:{totalTimeSpan.Seconds:#00}] 🔉";

			return descriptionString;
		}
	}

	private static string PlayerAdvance(int timeSpanAdvanceInt, TimeSpan totalTimeSpan)
	{
		var strings = new string[15];
		var playerAdvanceString = "";

		var thisIsOneHundredPercent = totalTimeSpan.TotalSeconds;
		var dotPositionInPercent = 100.0 / thisIsOneHundredPercent * timeSpanAdvanceInt;
		var dotPositionInInt = 15.0 / 100.0 * dotPositionInPercent;

		for (var i = 0; i < strings.Length; i++)
		{
			strings[i] = Convert.ToInt32(dotPositionInInt) == i ? "🔘" : "▬";
		}

		foreach (var item in strings)
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
		var fingerPrintCalculationProcess = Process.Start(fingerPrintCalculationProcessStartInfo);
		if (fingerPrintCalculationProcess != null)
		{
			var fingerPrintCalculationOutput = fingerPrintCalculationProcess.StandardOutput.ReadToEndAsync().Result;
			var fingerPrintArgs = fingerPrintCalculationOutput.Split("\r\n");
			if (fingerPrintArgs.Length == 3)
			{
				fingerPrintDuration = fingerPrintArgs[0].Split('=');
				fingerPrintFingerprint = fingerPrintArgs[1].Split('=');
			}
		}

		AcoustId.Root acoustId = new();
		if (fingerPrintDuration != null)
		{
			var url = "http://api.acoustid.org/v2/lookup?client=" + Bot.Connections.AcoustIdApiKey + "&duration=" + fingerPrintDuration[1] + "&fingerprint=" + fingerPrintFingerprint[1] +
						 "&meta=recordings+recordingIds+releases+releaseIds+ReleaseGroups+releaseGroupIds+tracks+compress+userMeta+sources";

			var httpClientContent = new HttpClient().GetStringAsync(url).Result;
			acoustId = AcoustId.CreateObj(httpClientContent);
		}

		return acoustId;
	}

	public static DiscordEmbedBuilder CustomDiscordEmbedBuilder(DiscordEmbedBuilder discordEmbedBuilder, QueueItem queueItem, Uri filePathUri, VideoData audioDownloadMetaData, TagLib.File metaTagFileToPlay)
	{
		if (metaTagFileToPlay == null)
		{
			var needThumbnail = true;
			var needAlbum = true;
			var albumTitle = "";
			var recordingMbId = "";
			discordEmbedBuilder.Title = audioDownloadMetaData.Title;
			discordEmbedBuilder.WithAuthor(audioDownloadMetaData.Creator);
			discordEmbedBuilder.WithUrl(queueItem.YouTubeUri.AbsoluteUri);

			if (queueItem.IsSpotify)
			{
				var spotifyClient = GetSpotifyClientConfig();
				var trackId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(queueItem.SpotifyUri.AbsoluteUri, "/track/", "/track/".Length), "?si", 0);
				var fullTrack = spotifyClient.Tracks.Get(trackId).Result;
				if (fullTrack.Album.Images.Count > 0)
				{
					discordEmbedBuilder.WithThumbnail(fullTrack.Album.Images[0].Url);

					Bitmap bitmapAlbumCover = new(new HttpClient().GetStreamAsync(fullTrack.Album.Images[0].Url).Result);
					var dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
					discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
					needThumbnail = false;
				}

				if (fullTrack.Album.Name != "")
				{
					albumTitle = fullTrack.Album.Name;
					needAlbum = false;
				}
			}

			var acoustIdRoot = AcoustIdFromFingerPrint(filePathUri);
			if (acoustIdRoot.Results?.Count > 0 && acoustIdRoot.Results[0].Recordings?[0].Releases != null)
			{
				DateTime rightAlbumDateTime = new();
				AcoustId.Release rightAlbum = new();

				if (needAlbum)
				{
					foreach (var albumItem in acoustIdRoot.Results[0].Recordings[0].Releases)
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
				var iRecording = new Query().LookupRecordingAsync(new Guid(recordingMbId)).Result;

				var genres = "";
				if (iRecording.Genres != null)
				{
					foreach (var genre in genres)
					{
						genres += genre;

						if (genres.Last() != genre)
							genres += ", ";
					}
				}
				if (genres != "")
					discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));

				if (rightAlbum.Id != null && needThumbnail)
				{
					try
					{
						discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release/{rightAlbum.Id}/front");

						Bitmap bitmapAlbumCover0 = new(new HttpClient().GetStreamAsync($"https://coverartarchive.org/release/{rightAlbum.Id}/front").Result);
						var dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover0);
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
				var dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
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
					var httpClientContent = new HttpClient().GetStringAsync($"https://coverartarchive.org/release/{metaTagFileToPlay.Tag.MusicBrainzReleaseId}").Result;
					var musicBrainzObj = MusicBrainz.CreateObj(httpClientContent);

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
						var dominantColor = ColorMath.GetDominantColor(albumCoverBitmap);
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

	[SlashCommand("DrivePlay" + Bot.IS_DEV_BOT, "Just plays some random music!")]
	private static async Task DrivePlayCommand(InteractionContext interactionContext)
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
			var cancellationToken = tokenSource.Token;
			CancellationTokenItem cancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
			s_cancellationTokenItemList.Add(cancellationTokenKeyPair);

			try
			{
				Task.Run(() => DrivePlayTask(interactionContext, null, null, null, null, true, cancellationToken), cancellationToken);
			}
			catch
			{
				s_cancellationTokenItemList.Remove(cancellationTokenKeyPair);
			}
		}
		else
			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is already playing!"));
	}

	private static async Task DrivePlayTask(InteractionContext interactionContext, DiscordClient discordClient, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionDiscordChannel, bool isInitialMessage, CancellationToken cancellationToken)
	{
		discordClient ??= interactionContext.Client;
		discordGuild ??= interactionContext.Guild;
		interactionDiscordChannel ??= interactionContext.Channel;

		try
		{
			var voiceNextExtension = discordClient.GetVoiceNext();

			if (voiceNextExtension == null)
				return;

			var voiceNextConnection = voiceNextExtension.GetConnection(discordGuild);
			var discordMemberVoiceState = interactionContext != null ? interactionContext.Member?.VoiceState : discordMember?.VoiceState;

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
				var allFiles = Directory.GetFiles(networkDriveUri.AbsolutePath);

				Random random = new();
				var randomInt = random.Next(0, allFiles.Length - 1);
				var selectedFileToPlay = allFiles[randomInt];

				var metaTagFileToPlay = TagLib.File.Create(@$"{selectedFileToPlay}");
				var discordEmbedBuilder = CustomDiscordEmbedBuilder(new DiscordEmbedBuilder(), null, null, null, metaTagFileToPlay);

				try
				{
					var discordMessage = await interactionDiscordChannel.SendMessageAsync(discordEmbedBuilder.Build());

					DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
					DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
					var discordComponents = new DiscordComponent[2];
					discordComponents[0] = new DiscordButtonComponent(ButtonStyle.Primary, "next_song", "Next!", false, discordComponentEmojisNext);
					discordComponents[1] = new DiscordButtonComponent(ButtonStyle.Danger, "stop_song", "Stop!", false, discordComponentEmojisStop);

					await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents));

					ProcessStartInfo ffmpegProcessStartInfo = new()
					{
						FileName = "..\\..\\..\\Model\\Executables\\ffmpeg\\ffmpeg.exe",
						Arguments = $@"-i ""{selectedFileToPlay}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
						RedirectStandardOutput = true,
						UseShellExecute = false
					};
					var ffmpegProcess = Process.Start(ffmpegProcessStartInfo);
					var ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;

					var voiceTransmitSink = voiceNextConnection.GetTransmitSink();
					voiceTransmitSink.VolumeModifier = 0.2;

					var ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink, cancellationToken: cancellationToken);

					var timeSpanAdvanceInt = 0;
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
						await Task.Delay(1000, cancellationToken);
					}

					discordEmbedBuilder.Description = TimeLineStringBuilderAfterSong(timeSpanAdvanceInt, metaTagFileToPlay.Properties.Duration, cancellationToken);
					await discordMessage.ModifyAsync(x => x.WithEmbed(discordEmbedBuilder.Build()));

					await voiceTransmitSink.FlushAsync(cancellationToken);
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
				interactionContext.Client.Logger.LogError("{msg}", exc.Message);
			else
				discordClient.Logger.LogError("{msg}", exc.Message);
		}
	}

	[SlashCommand("Stop" + Bot.IS_DEV_BOT, "Stop the music!")]
	private static async Task DriveStopCommand(InteractionContext interactionContext)
	{
		await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

		if (interactionContext.Member.VoiceState == null)
		{
			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You must be connected!"));
			return;
		}

		await StopMusicTask(interactionContext, true, interactionContext.Client, interactionContext.Guild, interactionContext.Channel);
	}

	[SlashCommand("Skip" + Bot.IS_DEV_BOT, "Skip this song!")]
	private static async Task SkipCommand(InteractionContext interactionContext)
	{
		await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
		await NextSongTask(interactionContext);
	}

	[SlashCommand("Next" + Bot.IS_DEV_BOT, "Skip this song!")]
	private static async Task NextCommand(InteractionContext interactionContext)
	{
		await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
		await NextSongTask(interactionContext);
	}

	[SlashCommand("Shuffle" + Bot.IS_DEV_BOT, "Randomize the queue!")]
	private static async Task Shuffle(InteractionContext interactionContext)
	{
		await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

		var discordMessage = await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Shuffle requested!"));

		await ShufflePlaylist(discordMessage);
	}

	private static async Task ShufflePlaylist(DiscordMessage discordMessage)
	{
		if (s_queueCreatingList.Exists(x => x.DiscordGuild == discordMessage.Channel.Guild))
		{
			await discordMessage.ModifyAsync(x => x.WithContent($"Queue is being created! " +
																$"{s_queueCreatingList.Find(x => x.DiscordGuild == discordMessage.Channel.Guild)!.QueueAddedAmount}/" +
																$"{s_queueCreatingList.Find(x => x.DiscordGuild == discordMessage.Channel.Guild)!.QueueAmount} Please wait!"));
		}
		else
		{
			List<QueueItem> queueItemListMixed = new();
			var queueItemList = QueueItemList.FindAll(x => x.DiscordGuild == discordMessage.Channel.Guild);

			var queueLength = queueItemList.Count;
			List<int> intListMixed = new();

			for (var i = 0; i < queueLength; i++)
			{
				var foundNumber = false;

				do
				{
					var randomInt = new Random().Next(0, queueLength);
					if (!intListMixed.Contains(randomInt))
					{
						intListMixed.Add(randomInt);
						foundNumber = true;
					}
				} while (!foundNumber);
			}

			foreach (var randomInt in intListMixed)
			{
				queueItemListMixed.Add(queueItemList[randomInt]);
			}

			QueueItemList.RemoveAll(x => x.DiscordGuild == discordMessage.Channel.Guild);
			foreach (var queueItem in queueItemListMixed)
			{
				QueueItemList.Add(queueItem);
			}

			await discordMessage.ModifyAsync(x => x.WithContent("Queue has been altered!"));
		}
	}

	private static async Task NextSongTask(InteractionContext interactionContext)
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
		var cancellationToken = tokenSource.Token;
		CancellationTokenItem cancellationTokenKeyPair = new(interactionContext.Guild, tokenSource);
		s_cancellationTokenItemList.Add(cancellationTokenKeyPair);

		try
		{
			Task.Run(() => DrivePlayTask(interactionContext, null, null, null, null, false, cancellationToken), cancellationToken);
		}
		catch (Exception ex)
		{
			CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
			s_cancellationTokenItemList.Remove(cancellationTokenKeyPair);
		}
	}

	internal static async Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
	{
		await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

		switch (eventArgs.Id)
		{
			case "next_song":
			{
				var discordMember = await eventArgs.User.ConvertToMember(eventArgs.Guild);
				if (discordMember.VoiceState == null)
				{
					await eventArgs.Channel.SendMessageAsync("You must be connected!");
					return;
				}

				if (QueueItemList.Any(x => x.DiscordGuild == eventArgs.Guild))
				{
					await eventArgs.Channel.SendMessageAsync("'Youtube' music is running! This interaction is locked!");
					return;
				}

				var nothingToStop = true;
				List<CancellationTokenSource> cancellationTokenSourceList = new();
				foreach (var cancellationTokenItem in s_cancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
				{
					nothingToStop = false;
					cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
				}
				s_cancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

				foreach (var cancellationTokenItem in cancellationTokenSourceList)
				{
					cancellationTokenItem.Cancel();
					cancellationTokenItem.Dispose();
				}

				QueueItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);
				s_queueCreatingList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

				await eventArgs.Channel.SendMessageAsync(nothingToStop ? "Nothing to stop!" : "Stopped the music!");

				CancellationTokenSource tokenSource = new();
				var cancellationToken = tokenSource.Token;
				CancellationTokenItem cancellationTokenKeyPair = new(eventArgs.Guild, tokenSource);
				s_cancellationTokenItemList.Add(cancellationTokenKeyPair);

				try
				{
					_ = Task.Run(async () => await DrivePlayTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, false, cancellationToken), cancellationToken);
				}
				catch (Exception ex)
				{
					CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
					s_cancellationTokenItemList.Remove(cancellationTokenKeyPair);
				}

				break;
			}
			case "stop_song":
			{
				var discordMember = await eventArgs.User.ConvertToMember(eventArgs.Guild);
				if (discordMember.VoiceState == null)
				{
					await eventArgs.Channel.SendMessageAsync("You must be connected!");
					return;
				}

				await StopMusicTask(null, true, client, eventArgs.Guild, eventArgs.Channel);

				break;
			}
			case "next_song_stream":
			{
				var discordMember = await eventArgs.User.ConvertToMember(eventArgs.Guild);
				if (discordMember.VoiceState == null)
				{
					await eventArgs.Channel.SendMessageAsync("You must be connected!");
					return;
				}

				if (QueueItemList.All(x => x.DiscordGuild != eventArgs.Guild))
				{
					await eventArgs.Channel.SendMessageAsync("Nothing to skip!");
					return;
				}

				List<CancellationTokenSource> cancellationTokenSourceList = new();
				foreach (var cancellationTokenItem in s_cancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
				{
					cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
				}

				s_cancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

				foreach (var cancellationToken in cancellationTokenSourceList)
				{
					cancellationToken.Cancel();
					cancellationToken.Dispose();
				}

				CancellationTokenSource newCancellationTokenSource = new();
				var newCancellationToken = newCancellationTokenSource.Token;

				foreach (var queueItem in QueueItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
				{
					CancellationTokenItem newCancellationTokenItem = new(eventArgs.Guild, newCancellationTokenSource);
					s_cancellationTokenItemList.Add(newCancellationTokenItem);

					_ = Task.Run(async () => await PlayFromQueueAsyncTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, queueItem, false, newCancellationToken), newCancellationToken);
					break;
				}

				break;
			}
			case "stop_song_stream":
			{
				var discordMember = await eventArgs.User.ConvertToMember(eventArgs.Guild);
				if (discordMember.VoiceState == null)
				{
					await eventArgs.Channel.SendMessageAsync("You must be connected!");
					return;
				}

				await StopMusicTask(null, true, client, eventArgs.Guild, eventArgs.Channel);

				break;
			}
			case "shuffle_stream":
			{
				var discordMember = await eventArgs.User.ConvertToMember(eventArgs.Guild);
				if (discordMember.VoiceState == null)
				{
					await eventArgs.Channel.SendMessageAsync("You must be connected!");
				}

				var discordMessage = await eventArgs.Channel.SendMessageAsync("Shuffle requested!");

				ShufflePlaylist(discordMessage);
				break;
			}
			case "showQueue_stream":
			{
				var discordMember = await eventArgs.User.ConvertToMember(eventArgs.Guild);
				if (discordMember.VoiceState == null)
				{
					await eventArgs.Channel.SendMessageAsync("You must be connected!");
				}

				var discordMessage = await eventArgs.Channel.SendMessageAsync("Loading!");

				if (QueueItemList.All(x => x.DiscordGuild != eventArgs.Guild))
					await discordMessage.ModifyAsync("Queue is empty!");
				else
				{
					var descriptionString = "";
					DiscordEmbedBuilder discordEmbedBuilder = new();
					YoutubeClient youtubeClient = new();

					var queueItemList = QueueItemList.FindAll(x => x.DiscordGuild == eventArgs.Channel.Guild);

					for (var i = 0; i < 10; i++)
					{
						if (queueItemList.Count == i)
							break;

						var videoData = await youtubeClient.Videos.GetAsync(queueItemList[i].YouTubeUri.AbsoluteUri);

						if (queueItemList[i].IsSpotify)
							descriptionString += "[🔗[YouTube]" + $"({queueItemList[i].YouTubeUri.AbsoluteUri})] " + "[🔗[Spotify]" + $"({queueItemList[i].SpotifyUri.AbsoluteUri})]  " + videoData.Title + " - " + videoData.Author + "\n";
						else
							descriptionString += "[🔗[YouTube]" + $"({queueItemList[i].YouTubeUri.AbsoluteUri})] " + videoData.Title + " - " + videoData.Author + "\n";
					}

					discordEmbedBuilder.Title = $"{queueItemList.Count} Track/s in queue!";
					discordEmbedBuilder.WithDescription(descriptionString);
					await discordMessage.ModifyAsync(new DiscordMessageBuilder().AddEmbed(discordEmbedBuilder));
				}

				break;
			}
		}
		return;
	}

	private static Task StopMusicTask(InteractionContext interactionContext, bool sendStopped, DiscordClient client, DiscordGuild discordGuild, DiscordChannel discordChannel)
	{
		var nothingToStop = true;
		List<CancellationTokenSource> cancellationTokenSourceList = new();
		foreach (var cancellationTokenItem in s_cancellationTokenItemList.Where(x => x.DiscordGuild == discordGuild))
		{
			nothingToStop = false;
			cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
		}
		s_cancellationTokenItemList.RemoveAll(x => x.DiscordGuild == discordGuild);

		foreach (var cancellationToken in cancellationTokenSourceList)
		{
			cancellationToken.Cancel();
			cancellationToken.Dispose();
		}

		QueueItemList.RemoveAll(x => x.DiscordGuild == discordGuild);
		s_queueCreatingList.RemoveAll(x => x.DiscordGuild == discordGuild);

		if (interactionContext != null)
			interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(nothingToStop ? "Nothing to stop!" : "Stopped the music!"));
		else if (sendStopped)
			discordChannel.SendMessageAsync(nothingToStop ? "Nothing to stop!" : "Stopped the music!");

		if (client != null)
		{
			try
			{
				var voiceNext = client.GetVoiceNext();
				var voiceNextConnection = voiceNext.GetConnection(interactionContext != null ? interactionContext.Guild : discordGuild);
				voiceNextConnection?.Disconnect();
			}
			catch
			{
				//ignore
			}
		}

		return Task.CompletedTask;
	}

	internal static async Task PanicLeaveEvent(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
	{
		try
		{
			var discordMember = await client.CurrentUser.ConvertToMember(eventArgs.Guild);
			if (eventArgs.Before != null && eventArgs.After != null && discordMember.VoiceState != null)
			{
				if (eventArgs.User == client.CurrentUser && eventArgs.After != null && eventArgs.Before.Channel != eventArgs.After.Channel)
				{
					var nothingToStop = true;
					List<CancellationTokenSource> cancellationTokenSourceList = new();
					foreach (var cancellationTokenItem in s_cancellationTokenItemList.Where(x => x.DiscordGuild == eventArgs.Guild))
					{
						nothingToStop = false;
						cancellationTokenSourceList.Add(cancellationTokenItem.CancellationTokenSource);
					}
					s_cancellationTokenItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

					foreach (var cancellationToken in cancellationTokenSourceList)
					{
						cancellationToken.Cancel();
						cancellationToken.Dispose();
					}

					QueueItemList.RemoveAll(x => x.DiscordGuild == eventArgs.Guild);

					eventArgs.Channel.SendMessageAsync(nothingToStop ? "Nothing to stop!" : "Stopped the music!");

					var voiceNext = client.GetVoiceNext();
					var voiceNextConnection = voiceNext.GetConnection(eventArgs.Guild);
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
			var discordMember = await client.CurrentUser.ConvertToMember(eventArgs.Guild);
			if (discordMember.VoiceState == null)
			{
				if (eventArgs.User == client.CurrentUser)
				{
					await StopMusicTask(null, true, client, eventArgs.Guild, eventArgs.Before.Channel);
				}
			}
		}
		catch
		{
			// ignored
		}
	}
}
