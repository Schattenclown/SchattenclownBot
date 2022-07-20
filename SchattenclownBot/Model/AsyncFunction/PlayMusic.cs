using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Model.Discord.Main;
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

namespace SchattenclownBot.Model.AsyncFunction
{
    internal class PlayMusic
    {
        private static List<KeyValuePair<DiscordGuild, CancellationTokenSource>> tokenList = new();
        public static async Task PlayMusicAsync(InteractionContext interactionContext)
        {
            if (interactionContext.Member.VoiceState == null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You have to be connected!"));
                return;
            }

            var musicAlreadyPlaying = false;
            foreach (var keyValuePairItem in tokenList.Where(x => x.Key == interactionContext.Guild))
            {
                musicAlreadyPlaying = true;
                break;
            }

            if (!musicAlreadyPlaying)
            {
                var tokenSource = new CancellationTokenSource();
                var cancellationToken = tokenSource.Token;
                var keyPairItem = new KeyValuePair<DiscordGuild, CancellationTokenSource>(interactionContext.Guild, tokenSource);
                tokenList.Add(keyPairItem);

                Task playMusicTask;
                try
                {
                    playMusicTask = Task.Run(() => PlayMusicTask(interactionContext, null, null, null, cancellationToken, false), cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    tokenList.Remove(keyPairItem);
                }
            }
            else
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Music is playing already!"));
        }
        public static async Task NextSongAsync(InteractionContext interactionContext, DiscordClient client, DiscordGuild guild, DiscordMember discordMember)
        {
            if (interactionContext != null)
            {
                if (interactionContext.Member.VoiceState == null)
                {
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You have to be connected!"));
                    return;
                }
            }

            CancellationTokenSource tokenSource = null;
            foreach (var keyValuePairItem in tokenList.Where(x => x.Key == guild))
            {
                tokenSource = keyValuePairItem.Value;
                tokenList.Remove(keyValuePairItem);
                break;
            }


            if (tokenSource != null)
            {
                if (interactionContext != null)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Skiping!"));
                
                tokenSource.Cancel();
                tokenSource.Dispose();
                await Task.Delay(1500);
            }
            else
            {
                if (interactionContext != null)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing to skip!"));
                return;
            }

            tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;
            var keyPairItem = new KeyValuePair<DiscordGuild, CancellationTokenSource>(guild, tokenSource);
            tokenList.Add(keyPairItem);

            Task playMusicTask;

            try
            {
                if (interactionContext != null)
                    playMusicTask = Task.Run(() => PlayMusicTask(interactionContext, null, null, null, cancellationToken, true), cancellationToken);
                else
                    playMusicTask = Task.Run(() => PlayMusicTask(null, client, guild, discordMember, cancellationToken, true), cancellationToken);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                tokenList.Remove(keyPairItem);
            }
        }
        static async Task PlayMusicTask(InteractionContext interactionContext, DiscordClient client, DiscordGuild guild, DiscordMember discordMember, CancellationToken cancellationToken, bool isNextSongRequest)
        {
            try
            {
                if (interactionContext != null)
                {
                    client = interactionContext.Client;
                    guild = interactionContext.Guild;
                    discordMember = interactionContext.Member;
                }

                var voiceNext = client.GetVoiceNext();
                if (voiceNext == null)
                    return;

                var voiceNextConnection = voiceNext.GetConnection(guild);

                var voiceState = discordMember?.VoiceState;
                if (voiceState?.Channel == null)
                    return;

                voiceNextConnection ??= await voiceNext.ConnectAsync(voiceState.Channel);

                if (interactionContext != null)
                {
                    if (isNextSongRequest)
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Skiped song in {voiceNextConnection.TargetChannel.Mention}!"));
                    else
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!"));
                }

                while (!cancellationToken.IsCancellationRequested)
                {
                    var Uri = new Uri(@"M:\");
                    var allFiles = Directory.GetFiles(Uri.AbsolutePath);

                    Random random = new();
                    int randomInt = random.Next(0, (allFiles.Length - 1));
                    var selectedFileToPlay = allFiles[randomInt];

                    //Metadata
                    #region MetaTags
                    var tagLibSelectedFileToplay = TagLib.File.Create(@$"{selectedFileToPlay}");
                    MusicBrainz.Root musicBrainz = null;
                    if (tagLibSelectedFileToplay.Tag.MusicBrainzReleaseId != null)
                    {
                        Uri coverArtUrl = new($"https://coverartarchive.org/release/{tagLibSelectedFileToplay.Tag.MusicBrainzReleaseId}");
                        var httpClient = new HttpClient();
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
                        //DiscordFormat
                        #region discordEmbedBuilder
                        var discordEmbedBuilder = new DiscordEmbedBuilder
                        {
                            Title = tagLibSelectedFileToplay.Tag.Title
                        };
                        discordEmbedBuilder.WithAuthor(tagLibSelectedFileToplay.Tag.JoinedPerformers);
                        if (tagLibSelectedFileToplay.Tag.Album != null)
                            discordEmbedBuilder.AddField(new DiscordEmbedField("Album", tagLibSelectedFileToplay.Tag.Album, true));
                        if (tagLibSelectedFileToplay.Tag.JoinedGenres != null)
                            discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", tagLibSelectedFileToplay.Tag.JoinedGenres, true));

                        HttpClient httpClient = new();
                        Stream streamForBitmap = null;
                        if (musicBrainz != null)
                        {
                            discordEmbedBuilder.WithThumbnail(musicBrainz.images.FirstOrDefault().image);
                            streamForBitmap = await httpClient.GetStreamAsync(musicBrainz.images.FirstOrDefault().image);
                            discordEmbedBuilder.WithUrl(musicBrainz.release);
                        }
                        else if (tagLibSelectedFileToplay.Tag.MusicBrainzReleaseGroupId != null)
                        {
                            discordEmbedBuilder.WithThumbnail($"https://coverartarchive.org/release-group/{tagLibSelectedFileToplay.Tag.MusicBrainzReleaseGroupId}/front");
                            streamForBitmap = await httpClient.GetStreamAsync($"https://coverartarchive.org/release-group/{tagLibSelectedFileToplay.Tag.MusicBrainzReleaseGroupId}/front");
                        }

                        if (streamForBitmap != null)
                        {
                            var bitmapAlbumCover = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Bitmap(streamForBitmap) : null;
                            if (bitmapAlbumCover != null)
                            {
                                Color dominantColor = ColorMath.getDominantColor(bitmapAlbumCover);
                                discordEmbedBuilder.Color = new DiscordColor(dominantColor.R, dominantColor.G, dominantColor.B);
                            }
                        }
                        else
                        {
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzArtistId", tagLibSelectedFileToplay.Tag.MusicBrainzArtistId));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzDiscId", tagLibSelectedFileToplay.Tag.MusicBrainzDiscId));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseArtistId", tagLibSelectedFileToplay.Tag.MusicBrainzReleaseArtistId));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseCountry", tagLibSelectedFileToplay.Tag.MusicBrainzReleaseCountry));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseGroupId", tagLibSelectedFileToplay.Tag.MusicBrainzReleaseGroupId));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseId", tagLibSelectedFileToplay.Tag.MusicBrainzReleaseId));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseStatus", tagLibSelectedFileToplay.Tag.MusicBrainzReleaseStatus));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzReleaseType", tagLibSelectedFileToplay.Tag.MusicBrainzReleaseType));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicBrainzTrackId", tagLibSelectedFileToplay.Tag.MusicBrainzTrackId));
                            discordEmbedBuilder.AddField(new DiscordEmbedField("MusicIpId", tagLibSelectedFileToplay.Tag.MusicIpId));
                        }
                        #endregion
                        DiscordMessage discordMessage = null;
                        if (interactionContext != null)
                        {
                            discordMessage = await interactionContext.Channel.SendMessageAsync(discordEmbedBuilder.Build());
                        }

                        var psi = new ProcessStartInfo
                        {
                            FileName = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/ffmpeg" : $"..\\..\\..\\ffmpeg\\ffmpeg.exe"),
                            Arguments = $@"-i ""{selectedFileToPlay}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        };
                        var ffmpeg = Process.Start(psi);
                        var ffmpegOutput = ffmpeg.StandardOutput.BaseStream;

                        var voiceNextTransmition = voiceNextConnection.GetTransmitSink();
                        voiceNextTransmition.VolumeModifier = 0.2;

                        var ffmpegTask = ffmpegOutput.CopyToAsync(voiceNextTransmition);
                        var lastDiscordChannel = voiceNextConnection.TargetChannel;

                        var counter = 0;
                        while (!ffmpegTask.IsCompleted)
                        {
                            //algorythmus to create the timeline
                            #region TimeLineAlgo
                            if (counter % 10 == 0)
                            {
                                TimeSpan timeSpan = TimeSpan.FromSeconds(counter);

                                string[] strings = new string[15];
                                var thisIsOneHundretPercent = tagLibSelectedFileToplay.Properties.Duration.TotalSeconds;

                                var dotPositionInPercent = 100.0 / thisIsOneHundretPercent * counter;

                                var dotPositionInInt = 15.0 / 100.0 * dotPositionInPercent;

                                for (int i = 0; i < strings.Length; i++)
                                {
                                    if (Convert.ToInt32(dotPositionInInt) == i)
                                        strings[i] = "🔘";
                                    else
                                        strings[i] = "▬";
                                }
                                string finischedString = "";
                                foreach (var item in strings)
                                {
                                    finischedString += item;
                                }

                                discordEmbedBuilder.Description = $"⏹️ {finischedString} [{timeSpan.Hours:#00}:{timeSpan.Minutes:#00}:{timeSpan.Seconds:#00}/{tagLibSelectedFileToplay.Properties.Duration.Hours:#00}:{tagLibSelectedFileToplay.Properties.Duration.Minutes:#00}:{tagLibSelectedFileToplay.Properties.Duration.Seconds:#00}] 🔉";
                                if (discordMessage != null)
                                    await discordMessage.ModifyAsync(x => x.Embed = discordEmbedBuilder.Build());
                            }
                            #endregion

                            if (cancellationToken.IsCancellationRequested)
                            {
                                ffmpegOutput.Close();
                                return;
                            }
                            lastDiscordChannel = voiceNextConnection.TargetChannel;
                            counter++;
                            await Task.Delay(1000);
                        }

                        //algorythmus to create the timeline
                        #region MoteTimeLineAlgo
                        string thingy = $"{tagLibSelectedFileToplay.Properties.Duration.Hours:#00}:{tagLibSelectedFileToplay.Properties.Duration.Minutes:#00}:{tagLibSelectedFileToplay.Properties.Duration.Seconds:#00}";

                        if (!cancellationToken.IsCancellationRequested)
                            if (discordMessage != null)
                                discordEmbedBuilder.Description = $"⏹️ ▬▬▬▬▬▬▬▬▬▬▬▬▬▬🔘 [{thingy}/{thingy}] 🔉";
                        await discordMessage.ModifyAsync(x => x.Embed = discordEmbedBuilder.Build());
                        #endregion

                        await voiceNextTransmition.FlushAsync();
                        await voiceNextConnection.WaitForPlaybackFinishAsync();
                    }
                    catch
                    {

                    }
                }
            }
            catch (Exception exc)
            {
                client.Logger.LogError(exc.Message);
            }
        }
        public static async Task StopMusicAsync(InteractionContext interactionContext)
        {
            CancellationTokenSource tokenSource = null;
            foreach (var keyValuePairItem in tokenList.Where(x => x.Key == interactionContext.Guild))
            {
                tokenSource = keyValuePairItem.Value;
                tokenList.Remove(keyValuePairItem);
                break;
            }

            if (tokenSource != null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Stop the music!"));
                tokenSource.Cancel();
                tokenSource.Dispose();
            }
            else
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing to stop!"));
        }
        internal static async Task ResumePlaying(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
        {
            DiscordMember discordMember = await client.CurrentUser.ConvertToMember(eventArgs.Guild);
            if (eventArgs.User == client.CurrentUser && eventArgs.Before.Channel != eventArgs.After.Channel)
                await NextSongAsync(null, client, eventArgs.Guild, discordMember);
        }

        internal static Task GotKicked(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
        {
            //if get kicked delete KeyPaiValue

            return Task.CompletedTask;
        }
    }
}
