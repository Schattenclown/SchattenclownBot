using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using DisCatSharp.Net.Models;
using DisCatSharp.Enums;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.VoiceNext;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using TagLib;
using SchattenclownBot.Model.Objects;
using System.Net.Http;

namespace SchattenclownBot.Model.AsyncFunction
{
    internal class PlayMusic
    {
        private static List<KeyValuePair<DiscordGuild, CancellationTokenSource>> tokenList = new List<KeyValuePair<DiscordGuild, CancellationTokenSource>>();
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
                tokenList.Add(new KeyValuePair<DiscordGuild, CancellationTokenSource>(interactionContext.Guild, tokenSource));

                Task playMusicTask;

                playMusicTask = Task.Run(() => PlayMusicTask(interactionContext, cancellationToken, false), cancellationToken);
            }
            else
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Music is playing already!"));
        }
        public static async Task NextSongAsync(InteractionContext interactionContext)
        {
            if (interactionContext.Member.VoiceState == null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"You have to be connected!"));
                return;
            }

            CancellationTokenSource tokenSource = null;
            foreach (var keyValuePairItem in tokenList.Where(x => x.Key == interactionContext.Guild))
            {
                tokenSource = keyValuePairItem.Value;
                tokenList.Remove(keyValuePairItem);
                break;
            }

            if (tokenSource != null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Skiping!"));
                tokenSource.Cancel();
                tokenSource.Dispose();
                await Task.Delay(500);
            }
            else
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Nothing to skip!"));
                return;
            }

            tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;
            tokenList.Add(new KeyValuePair<DiscordGuild, CancellationTokenSource>(interactionContext.Guild, tokenSource));

            Task playMusicTask;

            playMusicTask = Task.Run(() => PlayMusicTask(interactionContext, cancellationToken, true), cancellationToken);
        }
        static async Task PlayMusicTask(InteractionContext interactionContext, CancellationToken cancellationToken, bool isNextSongRequest)
        {
            try
            {
                DiscordChannel discordchannel = null;

                var voiceNext = interactionContext.Client.GetVoiceNext();
                if (voiceNext == null)
                    return;

                var voiceNextConnection = voiceNext.GetConnection(interactionContext.Guild);

                if (voiceNextConnection != null)
                    await voiceNextConnection.SendSpeakingAsync(false);

                var voiceState = interactionContext.Member?.VoiceState;
                if (voiceState?.Channel == null && discordchannel == null)
                    return;

                discordchannel ??= voiceState.Channel;

                voiceNextConnection ??= await voiceNext.ConnectAsync(discordchannel);

                if (isNextSongRequest)
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Skiped song in {voiceNextConnection.TargetChannel.Mention}!"));
                else
                    await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"I start playing music in {voiceNextConnection.TargetChannel.Mention}!"));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var Uri = new Uri(@"M:\");
                    var files = Directory.GetFiles(Uri.AbsolutePath);

                    Random random = new Random();
                    int randomInt = random.Next(0, (files.Length - 1));
                    var selectedFile = files[randomInt];

                    var file = TagLib.File.Create(@$"{selectedFile}");
                    MusicBrainz.Root musicBrainz = null;
                    if (file.Tag.MusicBrainzReleaseId != null)
                    {
                        Uri uri = new Uri($"https://coverartarchive.org/release/{file.Tag.MusicBrainzReleaseId}");
                        var client = new HttpClient();
                        client.DefaultRequestHeaders.Add("User-Agent", "C# console program");
                        string content = await client.GetStringAsync(uri);
                        musicBrainz = MusicBrainz.CreateObj(content);
                    }

                    string arg = $@"-i ""{selectedFile}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet";

                    try
                    {
                        await voiceNextConnection.SendSpeakingAsync(true);

                        var discordEmbedBuilder = new DiscordEmbedBuilder
                        {
                            Title = file.Tag.Title
                        };
                        discordEmbedBuilder.WithAuthor(file.Tag.JoinedPerformers);
                        discordEmbedBuilder.AddField(new DiscordEmbedField("Album", file.Tag.Album));
                        discordEmbedBuilder.AddField(new DiscordEmbedField("Genre", file.Tag.JoinedGenres));

                        if (musicBrainz != null)
                        {
                            discordEmbedBuilder.WithThumbnail(musicBrainz.images.FirstOrDefault().image);
                            discordEmbedBuilder.WithUrl(musicBrainz.release);
                        }

                        await interactionContext.Channel.SendMessageAsync(discordEmbedBuilder.Build());

                        var psi = new ProcessStartInfo
                        {
                            FileName = (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "/usr/bin/ffmpeg" : $"..\\..\\..\\ffmpeg\\ffmpeg.exe"),
                            Arguments = arg,
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        };
                        var ffmpeg = Process.Start(psi);
                        var ffmpegOutput = ffmpeg.StandardOutput.BaseStream;

                        var voiceNextTransmition = voiceNextConnection.GetTransmitSink();
                        voiceNextTransmition.VolumeModifier = 0.2;

                        var ffmpegTask = ffmpegOutput.CopyToAsync(voiceNextTransmition);

                        while (!ffmpegTask.IsCompleted)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                ffmpegOutput.Close();
                                break;
                            }
                            await Task.Delay(500);
                        }
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
                interactionContext.Client.Logger.LogError(exc.Message);
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
        /*internal static Task ChangeStatus(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            if (e.User == Bot.Client.CurrentUser)
            {
                if (e.Channel == null)
                {
                    var activity = new DiscordActivity()
                    {
                        Name = $"/help",
                        ActivityType = ActivityType.Competing
                    };
                    Bot.Client.UpdateStatusAsync(activity: activity, userStatus: UserStatus.Online, idleSince: null);
                }
            }

            return Task.CompletedTask;
        }*/
    }
}
