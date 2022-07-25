using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using MetaBrainz.MusicBrainz;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
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
using YoutubeDLSharp.Options;
// ReSharper disable UnusedMember.Local
#pragma warning disable CS4014

// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands
{
    internal class PlayMusic : ApplicationCommandsModule
    {
        private static readonly List<KeyValuePair<DiscordGuild, CancellationTokenSource>> TokenList = new();
        private static readonly List<KeyValuePair<DiscordGuild, string>> QueueList = new();

        [SlashCommand("Play", "Just plays some random music!")]
        private async Task PlayCommand(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (interactionContext.Member.VoiceState == null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
                return;
            }

            if (QueueList.Any(x => x.Key == interactionContext.Guild))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Youtube music is playing! This interaction is locked!"));
                return;
            }

            bool musicAlreadyPlaying = false;
            foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> dummy in TokenList.Where(x => x.Key == interactionContext.Guild))
            {
                musicAlreadyPlaying = true;
                break;
            }

            if (!musicAlreadyPlaying)
            {
                CancellationTokenSource tokenSource = new();
                CancellationToken cancellationToken = tokenSource.Token;
                KeyValuePair<DiscordGuild, CancellationTokenSource> tokenKeyPair = new(interactionContext.Guild, tokenSource);
                TokenList.Add(tokenKeyPair);

                try
                {
                    Task.Run(() => PlayTask(interactionContext, null, null, null, null, cancellationToken, false, true), cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    TokenList.Remove(tokenKeyPair);
                }
            }
            else
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing already!"));
        }

        private static async Task PlayTask(InteractionContext interactionContext, DiscordClient client, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionChannel, CancellationToken cancellationToken, bool isNextSongRequest, bool isInitialMessage)
        {
            try
            {
                VoiceNextExtension voiceNext = interactionContext != null ? interactionContext.Client.GetVoiceNext() : client.GetVoiceNext();

                if (voiceNext == null)
                    return;

                VoiceNextConnection voiceNextConnection = interactionContext != null ? voiceNext.GetConnection(interactionContext.Guild) : voiceNext.GetConnection(discordGuild);
                DiscordVoiceState voiceState = interactionContext != null ? interactionContext.Member?.VoiceState : discordMember?.VoiceState;

                if (voiceState?.Channel == null)
                    return;

                voiceNextConnection ??= await voiceNext.ConnectAsync(voiceState.Channel);

                if (isNextSongRequest)
                {
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Skipped song in {voiceNextConnection.TargetChannel.Mention}!"));
                    else
                        await interactionChannel.SendMessageAsync($"Skipped song in {voiceNextConnection.TargetChannel.Mention}!");
                }
                else if (isInitialMessage)
                {
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!"));
                    else
                        await interactionChannel.SendMessageAsync($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!");
                }

                while (!cancellationToken.IsCancellationRequested)
                {
                    Uri uri = new(@"M:\");
                    string[] allFiles = Directory.GetFiles(uri.AbsolutePath);

                    Random random = new();
                    int randomInt = random.Next(0, allFiles.Length - 1);
                    string selectedFileToPlay = allFiles[randomInt];

                    #region MetaTags
                    TagLib.File tagLibSelectedFileToPlay = TagLib.File.Create(@$"{selectedFileToPlay}");
                    MusicBrainz.Root musicBrainz = null;
                    if (tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseId != null)
                    {
                        Uri coverArtUrl = new($"https://coverartarchive.org/release/{tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseId}");
                        HttpClient httpClient = new();
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                        try
                        {
                            string httpClientContent = await httpClient.GetStringAsync(coverArtUrl);
                            musicBrainz = MusicBrainz.CreateObj(httpClientContent);
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
                            Title = tagLibSelectedFileToPlay.Tag.Title
                        };
                        discordEmbedBuilder.WithAuthor(tagLibSelectedFileToPlay.Tag.JoinedPerformers);
                        if (tagLibSelectedFileToPlay.Tag.Album != null)
                            discordEmbedBuilder.AddField(new DiscordEmbedField("Album", tagLibSelectedFileToPlay.Tag.Album, true));
                        if (tagLibSelectedFileToPlay.Tag.JoinedGenres != null)
                            discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", tagLibSelectedFileToPlay.Tag.JoinedGenres, true));

                        HttpClient httpClient = new();
                        Stream streamForBitmap = null;
                        if (musicBrainz != null)
                        {
                            discordEmbedBuilder.WithThumbnail(musicBrainz.Images.FirstOrDefault().ImageString);
                            streamForBitmap = await httpClient.GetStreamAsync(musicBrainz.Images.FirstOrDefault().ImageString);
                            discordEmbedBuilder.WithUrl(musicBrainz.Release);
                        }
                        else if (tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseGroupId != null)
                        {
                            discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release-group/{tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseGroupId}/front");
                            streamForBitmap = await httpClient.GetStreamAsync($"https://coverartarchive.org/release-group/{tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseGroupId}/front");
                        }

                        if (streamForBitmap != null)
                        {
                            Bitmap bitmapAlbumCover = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Bitmap(streamForBitmap) : null;
                            if (bitmapAlbumCover != null)
                            {
                                Color dominantColor = ColorMath.GetDominantColor(bitmapAlbumCover);
                                discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
                            }
                        }
                        else
                        {
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzArtistId != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzArtistId", tagLibSelectedFileToPlay.Tag.MusicBrainzArtistId));
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzDiscId != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzDiscId", tagLibSelectedFileToPlay.Tag.MusicBrainzDiscId));
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseArtistId != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseArtistId", tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseArtistId));
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseCountry != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseCountry", tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseCountry));
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseGroupId != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseGroupId", tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseGroupId));
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseId != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseId", tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseId));
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseStatus != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseStatus", tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseStatus));
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseType != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseType", tagLibSelectedFileToPlay.Tag.MusicBrainzReleaseType));
                            if (tagLibSelectedFileToPlay.Tag.MusicBrainzTrackId != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzTrackId", tagLibSelectedFileToPlay.Tag.MusicBrainzTrackId));
                            if (tagLibSelectedFileToPlay.Tag.MusicIpId != null)
                                discordEmbedBuilder.AddField(new DiscordEmbedField("MusicIpId", tagLibSelectedFileToPlay.Tag.MusicIpId));
                        }
                        #endregion

                        DiscordMessage discordMessage = interactionContext != null ? await interactionContext.Channel.SendMessageAsync(discordEmbedBuilder.Build()) : await interactionChannel.SendMessageAsync(discordEmbedBuilder.Build());

                        DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
                        DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
                        DiscordComponent[] discordComponents = new DiscordComponent[2];
                        discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song", "Next!", false, discordComponentEmojisNext);
                        discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song", "Stop!", false, discordComponentEmojisStop);

                        await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents));

                        ProcessStartInfo processStartInfo = new()
                        {
                            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/ffmpeg" : "..\\..\\..\\ffmpeg\\ffmpeg.exe",
                            Arguments = $@"-i ""{selectedFileToPlay}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        };
                        Process ffmpegProcess = Process.Start(processStartInfo);
                        Stream ffmpegStream = ffmpegProcess.StandardOutput.BaseStream;

                        VoiceTransmitSink voiceTransmitSink = voiceNextConnection.GetTransmitSink();
                        voiceTransmitSink.VolumeModifier = 0.2;

                        Task ffmpegCopyTask = ffmpegStream.CopyToAsync(voiceTransmitSink);

                        int counter = 0;
                        TimeSpan timeSpan = new(0, 0, 0, 0);
                        string playerAdvance = "";
                        while (!ffmpegCopyTask.IsCompleted)
                        {
                            #region TimeLineAlgo
                            if (counter % 10 == 0)
                            {
                                timeSpan = TimeSpan.FromSeconds(counter);

                                string[] strings = new string[15];
                                double thisIsOneHundredPercent = tagLibSelectedFileToPlay.Properties.Duration.TotalSeconds;

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

                                descriptionString += $" {playerAdvance} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{tagLibSelectedFileToPlay.Properties.Duration.Hours:#00}:{tagLibSelectedFileToPlay.Properties.Duration.Minutes:#00}:{tagLibSelectedFileToPlay.Properties.Duration.Seconds:#00}] 🔉";
                                discordEmbedBuilder.Description = descriptionString;
                                await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithEmbed(discordEmbedBuilder.Build()));
                            }
                            #endregion

                            if (cancellationToken.IsCancellationRequested)
                            {
                                ffmpegStream.Close();
                                break;
                            }

                            counter++;
                            await Task.Delay(1000);
                        }

                        #region MoteTimeLineAlgo
                        //algorithms to create the timeline
                        string durationString = $"{tagLibSelectedFileToPlay.Properties.Duration.Hours:#00}:{tagLibSelectedFileToPlay.Properties.Duration.Minutes:#00}:{tagLibSelectedFileToPlay.Properties.Duration.Seconds:#00}";

                        if (!cancellationToken.IsCancellationRequested)
                            discordEmbedBuilder.Description = $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
                        else
                        {
                            string descriptionString = "⏹️";
                            if (cancellationToken.IsCancellationRequested)
                                descriptionString = "▶️";

                            descriptionString += $" {playerAdvance} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{tagLibSelectedFileToPlay.Properties.Duration.Hours:#00}:{tagLibSelectedFileToPlay.Properties.Duration.Minutes:#00}:{tagLibSelectedFileToPlay.Properties.Duration.Seconds:#00}] 🔉";
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
                    client.Logger.LogError(exc.Message);
            }
        }

        [SlashCommand("PlayYouTube", "Play a youtube video or playlist!")]
        private async Task PlayYouTube(InteractionContext interactionContext, [Option("YouTubeLink", "YouTube link!")] string youtubeUriString)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (interactionContext.Member.VoiceState == null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
                return;
            }

            bool hasWatchTag = false;
            bool isPlaylist = false;
            if (youtubeUriString.Contains("&list=") || youtubeUriString.Contains("playlist?list="))
            {
                isPlaylist = true;

                if (youtubeUriString.Contains("watch?v=") && youtubeUriString.Contains("&index="))
                {
                    hasWatchTag = true;
                }
            }
            else if (!youtubeUriString.Contains("watch?v="))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("I don´t think so!"));
                return;
            }

            bool musicAlreadyPlaying = false;
            foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> dummy in TokenList.Where(x => x.Key == interactionContext.Guild))
            {
                musicAlreadyPlaying = true;
                break;
            }

            if (isPlaylist)
            {
                string playlistIndex = "";
                string singleVideoUrl = "";
                string playlistUrl = "";

                if (hasWatchTag)
                {
                    string watchId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(youtubeUriString, "watch?v=", "watch?v=".Length), "&list=", 0);
                    string playlistId = StringCutter.RemoveAfterWord(StringCutter.RemoveUntilWord(youtubeUriString, "&list=", "&list=".Length), "&index=", 0);
                    playlistIndex = StringCutter.RemoveUntilWord(youtubeUriString, "&index=", "&index=".Length);
                    singleVideoUrl = "https://www.youtube.com/watch?v=" + watchId;
                    playlistUrl = "https://www.youtube.com/playlist?list=" + playlistId;
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
                    YoutubeDLSharp.Metadata.VideoData[] videoUrls;
                    if (hasWatchTag)
                    {
                        OptionSet optionSet = new()
                        {
                            PlaylistStart = Convert.ToInt32(playlistIndex)
                        };
                        videoUrls = youtubeDl.RunVideoDataFetch(playlistUrl, new CancellationToken(), true, optionSet).Result.Data.Entries;
                    }
                    else
                    {
                        videoUrls = youtubeDl.RunVideoDataFetch(youtubeUriString).Result.Data.Entries;
                    }

                    if (videoUrls != null)
                    {
                        if (!musicAlreadyPlaying)
                        {
                            if (hasWatchTag)
                                await PlayYoutubeRunAsync(interactionContext, singleVideoUrl);
                            else
                                await PlayYoutubeRunAsync(interactionContext, videoUrls[0].Url);
                        }
                        else
                        {
                            if (hasWatchTag)
                            {
                                KeyValuePair<DiscordGuild, string> queueKeyPair = new(interactionContext.Guild, singleVideoUrl);
                                QueueList.Add(queueKeyPair);
                            }
                            else
                            {
                                KeyValuePair<DiscordGuild, string> queueKeyPair = new(interactionContext.Guild, videoUrls[0].Url);
                                QueueList.Add(queueKeyPair);
                            }
                            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing already! Your songs are in the queue now!"));
                        }

                        for (int i = 1; i < videoUrls.Length; i++)
                        {
                            KeyValuePair<DiscordGuild, string> queueKeyPair = new(interactionContext.Guild, videoUrls[i].Url);
                            QueueList.Add(queueKeyPair);
                        }
                    }
                }
                catch
                {
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Cant play that!"));
                }
            }
            else
            {
                if (!musicAlreadyPlaying)
                {
                    await PlayYoutubeRunAsync(interactionContext, youtubeUriString);
                }
                else
                {
                    KeyValuePair<DiscordGuild, string> queueKeyPair = new(interactionContext.Guild, youtubeUriString);
                    QueueList.Add(queueKeyPair);

                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing already! Your song is in the queue!"));
                }
            }
        }

        private Task PlayYoutubeRunAsync(InteractionContext interactionContext, string youtubeUriString)
        {
            CancellationTokenSource tokenSource = new();
            CancellationToken cancellationToken = tokenSource.Token;
            KeyValuePair<DiscordGuild, CancellationTokenSource> tokenKeyPair = new(interactionContext.Guild, tokenSource);
            TokenList.Add(tokenKeyPair);
            KeyValuePair<DiscordGuild, string> queueKeyPair = new(interactionContext.Guild, youtubeUriString);
            QueueList.Add(queueKeyPair);

            try
            {
                Task.Run(() => PlayYouTubeTask(interactionContext, null, null, null, null, youtubeUriString, cancellationToken, false, true), cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TokenList.Remove(tokenKeyPair);
            }

            return Task.CompletedTask;
        }

        private static async Task PlayYouTubeTask(InteractionContext interactionContext, DiscordClient client, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionChannel, string youtubeUriString, CancellationToken cancellationToken, bool isNextSongRequest, bool isInitialMessage)
        {
            try
            {
                foreach (KeyValuePair<DiscordGuild, string> queueKeyPairItem in QueueList)
                {
                    if (queueKeyPairItem.Key == (interactionContext != null ? interactionContext.Guild : discordGuild) && queueKeyPairItem.Value == youtubeUriString)
                    {
                        QueueList.Remove(queueKeyPairItem);
                        break;
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

                if (isNextSongRequest)
                {
                    if (interactionContext != null)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Skipped song in {voiceNextConnection.TargetChannel.Mention}!"));
                    else
                        await interactionChannel.SendMessageAsync($"Skipped song in {voiceNextConnection.TargetChannel.Mention}!");
                }
                else if (isInitialMessage)
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
                RunResult<string> audioDownload = await youtubeDl.RunAudioDownload(youtubeUriString, AudioConversionFormat.Opus, new CancellationToken(), null, null, optionSet);

                try
                {
                    bool wyldFunctionSuccess = false;
                    DiscordEmbedBuilder discordEmbedBuilder = new();
                    MetaBrainz.MusicBrainz.Interfaces.Entities.IRecording musicBrainzTags = null;

                    if (audioDownload.ErrorOutput.Length <= 1)
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
                            const string apiKey = "Y2Ap7JHhdH";
                            string url = "http://api.acoustid.org/v2/lookup?client=" + apiKey + "&duration=" + fingerPrintDuration[1] + "&fingerprint=" + fingerPrintFingerprint[1] + "&meta=recordings+recordingIds+releases+releaseIds+ReleaseGroups+releaseGroupIds+tracks+compress+userMeta+sources";

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
                                string genres = "empty";

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
                                    if (compareDateTimeOne > compareDateTimeTwo)
                                    {
                                        rightAlbum = compareItem;
                                        compareDateTimeOne = compareDateTimeTwo;
                                    }
                                }

                                musicBrainzTags = await musicBrainzQuery.LookupRecordingAsync(new Guid(recordingMbId));
                                //MetaBrainz.MusicBrainz.Interfaces.Entities.IArtist artist = await musicBrainzQuery.LookupArtistAsync(new Guid(rightArtist.Id));

                                #region discordEmbedBuilder
                                discordEmbedBuilder.Title = musicBrainzTags.Title;
                                if (rightArtist != null)
                                    discordEmbedBuilder.WithAuthor(rightArtist.Name);

                                discordEmbedBuilder.AddField(new DiscordEmbedField("Album", rightAlbum.Title, true));
                                discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", genres, true));

                                HttpClient httpClientForBitmap = new();
                                if (rightAlbum.Id != null)
                                {
                                    try
                                    {
                                        discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release/{rightAlbum.Id}/front");
                                        var streamForBitmap = await httpClientForBitmap.GetStreamAsync($"https://coverartarchive.org/release/{rightAlbum.Id}/front");

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
                        youtubeUriString = $"{audioDownload.ErrorOutput[1]} `{youtubeUriString}`";
                    }

                    DiscordMessage discordMessage = interactionContext != null ? await interactionContext.Channel.SendMessageAsync("Loading!") : await interactionChannel.SendMessageAsync("Loading!");

                    DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
                    DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
                    DiscordComponent[] discordComponents = new DiscordComponent[2];
                    discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song_yt", "Next!", false, discordComponentEmojisNext);
                    discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song_yt", "Stop!", false, discordComponentEmojisStop);

                    if (wyldFunctionSuccess)
                        await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUriString).AddEmbed(discordEmbedBuilder.Build()));
                    else
                        await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUriString));

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
                            if (wyldFunctionSuccess && musicBrainzTags.Length != null)
                            {
                                #region TimeLineAlgo
                                if (counter % 10 == 0)
                                {
                                    timeSpan = TimeSpan.FromSeconds(counter);

                                    string[] strings = new string[15];
                                    double thisIsOneHundredPercent = musicBrainzTags.Length.Value.TotalSeconds;

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

                                    if (musicBrainzTags.Length != null) descriptionString += $" {playerAdvance} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{musicBrainzTags.Length.Value.Hours:#00}:{musicBrainzTags.Length.Value.Minutes:#00}:{musicBrainzTags.Length.Value.Seconds:#00}] 🔉";
                                    discordEmbedBuilder.Description = descriptionString;
                                    await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(youtubeUriString).WithEmbed(discordEmbedBuilder.Build()));
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

                        if (wyldFunctionSuccess && musicBrainzTags.Length != null)
                        {
                            #region MoteTimeLineAlgo
                            string durationString = $"{musicBrainzTags.Length.Value.Hours:#00}:{musicBrainzTags.Length.Value.Minutes:#00}:{musicBrainzTags.Length.Value.Seconds:#00}";

                            if (!cancellationToken.IsCancellationRequested)
                                discordEmbedBuilder.Description = $"▶️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{durationString}/{durationString}] 🔉";
                            else
                            {
                                string descriptionString = "⏹️";
                                if (cancellationToken.IsCancellationRequested)
                                    descriptionString = "▶️";

                                descriptionString += $" {playerAdvance} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{musicBrainzTags.Length.Value.Hours:#00}:{musicBrainzTags.Length.Value.Minutes:#00}:{musicBrainzTags.Length.Value.Seconds:#00}] 🔉";
                                discordEmbedBuilder.Description = descriptionString;
                            }

                            await discordMessage.ModifyAsync(x => x.WithContent(youtubeUriString).WithEmbed(discordEmbedBuilder.Build()));
                            #endregion
                        }
                        else
                            await discordMessage.ModifyAsync(x => x.WithContent(youtubeUriString));

                        if (!cancellationToken.IsCancellationRequested)
                        {
                            foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> tokenKeyPair in TokenList.Where(x => x.Key == (interactionContext != null ? interactionContext.Guild : discordGuild)))
                            {
                                TokenList.Remove(tokenKeyPair);
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
                    if (!QueueList.Any(x => x.Key == (interactionContext != null ? interactionContext.Guild : discordGuild)))
                    {
                        //Queue ist empty
                    }

                    foreach (KeyValuePair<DiscordGuild, string> queueKeyPairItem in QueueList)
                    {
                        if (queueKeyPairItem.Key == (interactionContext != null ? interactionContext.Guild : discordGuild))
                        {
                            CancellationTokenSource tokenSource = new();
                            CancellationToken newCancellationToken = tokenSource.Token;
                            KeyValuePair<DiscordGuild, CancellationTokenSource> tokenKeyPair = new(interactionContext != null ? interactionContext.Guild : discordGuild, tokenSource);
                            TokenList.Add(tokenKeyPair);
                            if (interactionContext != null)
                                Task.Run(() => PlayYouTubeTask(interactionContext, interactionContext.Client, interactionContext.Guild, interactionContext.Client.CurrentUser.ConvertToMember(interactionContext.Guild).Result, interactionContext.Channel, queueKeyPairItem.Value, newCancellationToken, false, false));
                            else
                                Task.Run(() => PlayYouTubeTask(interactionContext, client, discordGuild, discordMember, interactionChannel, queueKeyPairItem.Value, newCancellationToken, false, false));
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
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (interactionContext.Member.VoiceState == null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
                return;
            }

            if (QueueList.Any(x => x.Key == interactionContext.Guild))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Youtube music is playing! This interaction is locked!"));
                return;
            }

            CancellationTokenSource tokenSource = null;
            foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == interactionContext.Guild))
            {
                tokenSource = keyValuePairItem.Value;
                TokenList.Remove(keyValuePairItem);
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
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await PlayMusic.NextSongTask(interactionContext);
        }

        [SlashCommand("Next", "Skip this song!")]
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

            if (QueueList.Any(x => x.Key == interactionContext.Guild))
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Youtube music is playing! This interaction is locked!"));
                return;
            }

            CancellationTokenSource tokenSource = null;
            foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == interactionContext.Guild))
            {
                tokenSource = keyValuePairItem.Value;
                TokenList.Remove(keyValuePairItem);
                break;
            }

            if (tokenSource != null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Skipping!"));
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
            KeyValuePair<DiscordGuild, CancellationTokenSource> tokenKeyPair = new(interactionContext.Guild, tokenSource);
            TokenList.Add(tokenKeyPair);

            try
            {
                Task.Run(() => PlayTask(interactionContext, null, null, null, null, cancellationToken, true, false), cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TokenList.Remove(tokenKeyPair);
            }
        }

        internal static Task ButtonPressEvent(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

            switch (eventArgs.Id)
            {
                case "next_song":
                    {
                        if (QueueList.Any(x => x.Key == eventArgs.Guild))
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
                        foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == eventArgs.Guild))
                        {
                            tokenSource = keyValuePairItem.Value;
                            TokenList.Remove(keyValuePairItem);
                            break;
                        }

                        if (tokenSource != null)
                        {
                            eventArgs.Channel.SendMessageAsync("Skipping!");
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
                        KeyValuePair<DiscordGuild, CancellationTokenSource> tokenKeyPair = new(eventArgs.Guild, tokenSource);
                        TokenList.Add(tokenKeyPair);

                        try
                        {
                            Task.Run(() => PlayTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, cancellationToken, true, false), cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            TokenList.Remove(tokenKeyPair);
                        }

                        break;
                    }
                case "stop_song":
                    {
                        if (QueueList.Any(x => x.Key == eventArgs.Guild))
                        {
                            eventArgs.Channel.SendMessageAsync("Youtube music is playing! This interaction is locked!");
                            return Task.CompletedTask;
                        }

                        CancellationTokenSource tokenSource = null;
                        foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == eventArgs.Guild))
                        {
                            tokenSource = keyValuePairItem.Value;
                            TokenList.Remove(keyValuePairItem);
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

                        if (QueueList.Count == 0)
                        {
                            eventArgs.Channel.SendMessageAsync("Nothing to skip!");
                            return Task.CompletedTask;
                        }

                        CancellationTokenSource tokenSource = null;
                        foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == eventArgs.Guild))
                        {
                            tokenSource = keyValuePairItem.Value;
                            TokenList.Remove(keyValuePairItem);
                            break;
                        }

                        if (tokenSource != null)
                        {
                            eventArgs.Channel.SendMessageAsync("Skipping!");
                            tokenSource.Cancel();
                            tokenSource.Dispose();
                        }

                        CancellationTokenSource nextYtTokenSource = new();
                        CancellationToken nextYtCancellationToken = nextYtTokenSource.Token;
                        KeyValuePair<DiscordGuild, CancellationTokenSource> nextKeyPairItem = new();

                        try
                        {
                            foreach (KeyValuePair<DiscordGuild, string> queueKeyPairItem in QueueList)
                            {
                                if (queueKeyPairItem.Key == eventArgs.Guild)
                                {
                                    nextKeyPairItem = new KeyValuePair<DiscordGuild, CancellationTokenSource>(eventArgs.Guild, nextYtTokenSource);
                                    TokenList.Add(nextKeyPairItem);

                                    Task.Run(() => PlayYouTubeTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, queueKeyPairItem.Value, nextYtCancellationToken, true, false), nextYtCancellationToken);

                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            if (nextKeyPairItem.Value != null)
                                TokenList.Remove(nextKeyPairItem);
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
                        foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == eventArgs.Guild))
                        {
                            tokenSource = keyValuePairItem.Value;
                            TokenList.Remove(keyValuePairItem);
                            break;
                        }

                        QueueList.Clear();

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
                        foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == eventArgs.Guild))
                        {
                            tokenSource = keyValuePairItem.Value;
                            TokenList.Remove(keyValuePairItem);
                            break;
                        }

                        if (tokenSource != null)
                        {
                            await discordMember.VoiceState.Channel.SendMessageAsync("Stopped the music!");
                            tokenSource.Cancel();
                            tokenSource.Dispose();
                            QueueList.Clear();
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
                        foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == eventArgs.Guild))
                        {
                            tokenSource = keyValuePairItem.Value;
                            TokenList.Remove(keyValuePairItem);
                            break;
                        }


                        if (tokenSource != null)
                        {
                            tokenSource.Cancel();
                            tokenSource.Dispose();
                            QueueList.Clear();
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