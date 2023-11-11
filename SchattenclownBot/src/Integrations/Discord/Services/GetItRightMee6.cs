using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.Services
{
    public class GetItRightMee6
    {
        public void RunAsync(int executeSecond)
        {
            new CustomLogger().Information("Starting RunAsync...", ConsoleColor.Green);
            Task.Run(async () =>
            {
                while (DateTime.Now.Second != executeSecond)
                {
                    await Task.Delay(1000);
                }

                List<DiscordGuild> guildList;
                do
                {
                    guildList = DiscordBot.DiscordClient.Guilds.Values.ToList();
                    await Task.Delay(1000);
                } while (guildList.Count == 0);

                while (true)
                {
                    bool bool384KbNotAvailable = false;
                    DiscordGuild mainGuild = null;

                    foreach (DiscordGuild guildItem in guildList.Where(x => x.Id == 928930967140331590))
                    {
                        mainGuild = guildItem;
                    }

                    if (mainGuild == null)
                    {
                        return;
                    }

                    IEnumerable<DiscordChannel> discordChannels = mainGuild.Channels.Values.Where(x => x.Type == ChannelType.Voice).ToList();

                    foreach (DiscordChannel discordChannelItem in discordChannels)
                    {
                        try
                        {
                            if (discordChannelItem.Id != 982330147141218344 && discordChannelItem.Bitrate != 384000)
                            {
                                await discordChannelItem.ModifyAsync(x => x.Bitrate = 384000);
                                new CustomLogger().Information($"Bit-rate for Channel {discordChannelItem.Name}, {discordChannelItem.Id} set to 384000!", ConsoleColor.Green);
                                
                            }
                        }
                        catch
                        {
                            bool384KbNotAvailable = true;
                            new CustomLogger().Information("Bit-rate 384000 not available for guild", ConsoleColor.Green);
                            break;
                        }
                    }

                    if (bool384KbNotAvailable)
                    {
                        foreach (DiscordChannel discordChannelItem in discordChannels)
                        {
                            try
                            {
                                if (discordChannelItem.Id != 982330147141218344 && discordChannelItem.Bitrate != 256000)
                                {
                                    await discordChannelItem.ModifyAsync(x => x.Bitrate = 256000);
                                    new CustomLogger().Information($"Bit-rate for Channel {discordChannelItem.Name}, {discordChannelItem.Id} set to 256000!", ConsoleColor.Green);
                                }
                            }
                            catch
                            {
                                new CustomLogger().Information("Bit-rate 256000 not available for guild", ConsoleColor.Green);
                                break;
                            }
                        }
                    }

                    await Task.Delay(1000);
                    if (!LastMinuteCheck.CheckHighQualityAvailable)
                    {
                        LastMinuteCheck.CheckHighQualityAvailable = true;
                    }
                }
            });
        }

        public Task OnChannelCreated(DiscordClient discordClient, ChannelCreateEventArgs channelCreateEventArgs)
        {
            if (!channelCreateEventArgs.Channel.Name.Contains("🥇AFK-Farm#"))
            {
                return Task.CompletedTask;
            }

            channelCreateEventArgs.Channel.ModifyAsync(x => x.Bitrate = 256000);
            new CustomLogger().Information("BitRate set to 256k on" + channelCreateEventArgs.Channel.Name, ConsoleColor.Green);

            return Task.CompletedTask;
        }
    }
}