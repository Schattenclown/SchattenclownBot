using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.VoiceNext;
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
// ReSharper disable MethodSupportsCancellation
// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands
{
    internal class PlayMusic : ApplicationCommandsModule
    {
        private static readonly List<KeyValuePair<DiscordGuild, CancellationTokenSource>> TokenList = new();
        [SlashCommand("Play", "Just plays some music!")]
        public async Task PlayAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (interactionContext.Member.VoiceState == null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
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
                KeyValuePair<DiscordGuild, CancellationTokenSource> keyPairItem = new(interactionContext.Guild, tokenSource);
                TokenList.Add(keyPairItem);

                try
                {
#pragma warning disable CS4014
                    Task.Run(() => PlayMusicTask(interactionContext, null, null, null, null, cancellationToken, false), cancellationToken);
#pragma warning restore CS4014
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    TokenList.Remove(keyPairItem);
                }
            }
            else
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Music is playing already!"));
        }
        [SlashCommand("Stop", "Stop the music!")]
        public async Task StopAsync(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            CancellationTokenSource tokenSource = null;
            foreach (KeyValuePair<DiscordGuild, CancellationTokenSource> keyValuePairItem in TokenList.Where(x => x.Key == interactionContext.Guild))
            {
                tokenSource = keyValuePairItem.Value;
                TokenList.Remove(keyValuePairItem);
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

        [SlashCommand("Skip", "Skip this song!")]
        public async Task Skip(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await PlayMusic.NextSongAsync(interactionContext);
        }

        [SlashCommand("Next", "Skip this song!")]
        public async Task Next(InteractionContext interactionContext)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await PlayMusic.NextSongAsync(interactionContext);
        }

        public static async Task NextSongAsync(InteractionContext interactionContext)
        {
            if (interactionContext.Member.VoiceState == null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You have to be connected!"));
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
            KeyValuePair<DiscordGuild, CancellationTokenSource> keyPairItem = new(interactionContext.Guild, tokenSource);
            TokenList.Add(keyPairItem);

            try
            {
#pragma warning disable CS4014
                Task.Run(() => PlayMusicTask(interactionContext, null, null, null, null, cancellationToken, true), cancellationToken);
#pragma warning restore CS4014
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                TokenList.Remove(keyPairItem);
            }
        }

        static async Task PlayMusicTask(InteractionContext interactionContext, DiscordClient client, DiscordGuild discordGuild, DiscordMember discordMember, DiscordChannel interactionChannel, CancellationToken cancellationToken, bool isNextSongRequest)
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
                else
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

                    //Metadata
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
                        //DiscordFormat
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

                        DiscordMessage discordMessage;

                        if (interactionContext != null)
                            discordMessage = await interactionContext.Channel.SendMessageAsync(discordEmbedBuilder.Build());
                        else
                            discordMessage = await interactionChannel.SendMessageAsync(discordEmbedBuilder.Build());

                        DiscordComponentEmoji discordComponentEmojisNext = new("⏭️");
                        DiscordComponentEmoji discordComponentEmojisStop = new("⏹️");
                        DiscordComponent[] discordComponents = new DiscordComponent[2];
                        discordComponents[0] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Primary, "next_song", "Next!", false, discordComponentEmojisNext);
                        discordComponents[1] = new DiscordButtonComponent(DisCatSharp.Enums.ButtonStyle.Danger, "stop_song", "Stop!", false, discordComponentEmojisStop);


                        await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents));


                        ProcessStartInfo psi = new()
                        {
                            FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/ffmpeg" : "..\\..\\..\\ffmpeg\\ffmpeg.exe",
                            Arguments = $@"-i ""{selectedFileToPlay}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        };
                        Process ffmpeg = Process.Start(psi);
                        Stream ffmpegOutput = ffmpeg.StandardOutput.BaseStream;

                        VoiceTransmitSink voiceTransmitSink = voiceNextConnection.GetTransmitSink();
                        voiceTransmitSink.VolumeModifier = 0.2;

                        Task ffmpegTask = ffmpegOutput.CopyToAsync(voiceTransmitSink);

                        int counter = 0;
                        TimeSpan timeSpan = new(0, 0, 0, 0);
                        string playerAdvance = "";
                        while (!ffmpegTask.IsCompleted)
                        {
                            //algorithms to create the timeline
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
                                ffmpegOutput.Close();
                                break;
                            }

                            counter++;
                            await Task.Delay(1000);
                        }

                        //algorithms to create the timeline
                        #region MoteTimeLineAlgo
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

        internal static Task NextSongPerButton(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {

            if (eventArgs.Id == "next_song")
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
                KeyValuePair<DiscordGuild, CancellationTokenSource> keyPairItem = new(eventArgs.Guild, tokenSource);
                TokenList.Add(keyPairItem);

                try
                {
                    Task.Run(() => PlayMusicTask(null, client, eventArgs.Guild, discordMember, eventArgs.Channel, cancellationToken, true), cancellationToken);

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    TokenList.Remove(keyPairItem);
                }
            }
            else if (eventArgs.Id == "stop_song")
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
                    eventArgs.Channel.SendMessageAsync("Stop the music!");
                    tokenSource.Cancel();
                    tokenSource.Dispose();
                }
                else
                    eventArgs.Channel.SendMessageAsync("Nothing to stop!");


            }
            return Task.CompletedTask;
        }

        internal static async Task ResumePlaying(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
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

        internal static async Task GotKicked(DiscordClient client, VoiceStateUpdateEventArgs eventArgs)
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
