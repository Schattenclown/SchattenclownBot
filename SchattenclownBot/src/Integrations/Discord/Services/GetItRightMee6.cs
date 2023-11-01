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
    internal class GetItRightMee6
    {
        internal static Task ItRight(DiscordClient client, ChannelCreateEventArgs e)
        {
            if (e.Channel.Name.Contains("🥇AFK-Farm#"))
            {
                e.Channel.ModifyAsync(x => x.Bitrate = 256000);
                CustomLogger.ToConsole("BitRate set to 256k on" + e.Channel.Name, ConsoleColor.Green);
            }

            return Task.CompletedTask;
        }

        public static void CheckHighQualityAvailable(int executeSecond)
        {
            CustomLogger.ToConsole("Starting CheckHighQualityAvailable...", ConsoleColor.Green);
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
                            if (discordChannelItem.Id != 982330147141218344)
                            {
                                if (discordChannelItem.Bitrate != 384000)
                                {
                                    await discordChannelItem.ModifyAsync(x => x.Bitrate = 384000);
                                    CustomLogger.ToConsole($"Bit-rate for Channel {discordChannelItem.Name}, {discordChannelItem.Id} set to 384000!", ConsoleColor.Green);
                                }
                            }
                        }
                        catch
                        {
                            bool384KbNotAvailable = true;
                            CustomLogger.ToConsole("Bit-rate 384000 not available for guild", ConsoleColor.Green);
                            break;
                        }
                    }

                    if (bool384KbNotAvailable)
                    {
                        foreach (DiscordChannel discordChannelItem in discordChannels)
                        {
                            try
                            {
                                if (discordChannelItem.Id != 982330147141218344)
                                {
                                    if (discordChannelItem.Bitrate != 256000)
                                    {
                                        await discordChannelItem.ModifyAsync(x => x.Bitrate = 256000);
                                        CustomLogger.ToConsole($"Bit-rate for Channel {discordChannelItem.Name}, {discordChannelItem.Id} set to 256000!", ConsoleColor.Green);
                                    }
                                }
                            }
                            catch
                            {
                                CustomLogger.ToConsole("Bit-rate 256000 not available for guild", ConsoleColor.Green);
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
    }
}