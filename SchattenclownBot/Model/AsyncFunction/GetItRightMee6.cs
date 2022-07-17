using System;
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

namespace SchattenclownBot.Model.AsyncFunction
{
    internal class GetItRightMee6
    {
        internal static Task ItRight(DiscordClient client, ChannelCreateEventArgs e)
        {
            if (e.Channel.Name.Contains("🥇AFK-Farm#"))
            {
                e.Channel.ModifyAsync(x => x.Bitrate = 256000);
            }

            return Task.CompletedTask;
        }

        public static async Task CheckHighQualityAvailable(int executeSecond)
        {
            await Task.Run(async () =>
            {
                while (DateTime.Now.Second != executeSecond)
                {
                    await Task.Delay(1000);
                }

                List<DiscordGuild> guildList;
                do
                {
                    guildList = Bot.Client.Guilds.Values.ToList();
                    await Task.Delay(1000);
                } while (guildList.Count == 0);

                while (true)
                {
                    var bool384kBnotAvaliable = false;
                    DiscordGuild mainGuild = null;

                    foreach (var guildItem in guildList.Where(x => x.Id == 928930967140331590))
                    {
                        mainGuild = guildItem;
                    }

                    var discordChannels = mainGuild.Channels.Values.Where(x => x.Type == ChannelType.Voice);

                    foreach (var discordChannelItem in discordChannels)
                    {
                        try
                        {
                            if (discordChannelItem.Id != 982330147141218344)
                                if (discordChannelItem.Bitrate != 384000)
                                {
                                    await discordChannelItem.ModifyAsync(x => x.Bitrate = 384000);
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Bitrate for Channel {discordChannelItem.Name}, {discordChannelItem.Id} set to 384000!");
                                    Console.ForegroundColor = ConsoleColor.Gray;
                                }
                        }
                        catch
                        {
                            bool384kBnotAvaliable = true;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Bitrate 384000 not avaliable for guild");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                        }
                    }

                    if (bool384kBnotAvaliable == true)
                    {
                        foreach (var discordChannelItem in discordChannels)
                        {
                            try
                            {
                                if (discordChannelItem.Id != 982330147141218344)
                                    if (discordChannelItem.Bitrate != 256000)
                                    {
                                        await discordChannelItem.ModifyAsync(x => x.Bitrate = 256000);
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine($"Bitrate for Channel {discordChannelItem.Name}, {discordChannelItem.Id} set to 256000!");
                                        Console.ForegroundColor = ConsoleColor.Gray;
                                    }
                            }
                            catch
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("Bitrate 256000 not avaliable for guild");
                                Console.ForegroundColor = ConsoleColor.Gray;
                                break;
                            }
                        }
                    }

                    await Task.Delay(1000);
                }
            });
        }
    }
}
