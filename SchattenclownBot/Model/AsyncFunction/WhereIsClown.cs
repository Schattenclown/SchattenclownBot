using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity.Extensions;
using SchattenclownBot.Model.Discord.Main;

namespace SchattenclownBot.Model.AsyncFunction
{
    internal class WhereIsClown
    {
        public static async Task WhereIsClownRunAsync(int executeSecond)
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    while (DateTime.Now.Second != executeSecond)
                    {
                        await Task.Delay(1000);
                    }

                    var guildList = Bot.Client.Guilds.Values.ToList();

                    var voiceStateAny = false;

                    foreach (var guildItem in guildList)
                    {
                        var discordMemberList = guildItem.Members.Values.ToList();

                        foreach (var discordMemberItem in discordMemberList)
                        {
                            if (discordMemberItem.Id == 444152594898878474 && discordMemberItem.VoiceState != null)
                            {
                                voiceStateAny = true;
                                var mainGuild = Bot.Client.GetGuildAsync(928930967140331590).Result;
                                
                                var desctiprion = "";
                                foreach (var discordMemberInChannelItem in discordMemberItem.VoiceState.Channel.Users.ToList())
                                {
                                    if (discordMemberInChannelItem.VoiceState.IsSelfDeafened)
                                    {
                                        desctiprion += ":mute: " + discordMemberInChannelItem.DisplayName + "\n";
                                    }
                                    else
                                    {
                                        desctiprion += ":speaker: " + discordMemberInChannelItem.DisplayName + "\n";
                                    }
                                }
                                
                                var discordChannelOtherPlaces = mainGuild.GetChannel(981280701066395709);

                                var discordThreads = mainGuild.Threads.Values.ToList();

                                DiscordThreadChannel discordThreadsChannel = null;

                                foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315cl0wn"))
                                {
                                    discordThreadsChannel = discordThreadItem;
                                }

                                if (discordThreadsChannel == null)
                                {
                                    discordThreadsChannel = await discordChannelOtherPlaces.CreateThreadAsync("wh3r315cl0wn", ThreadAutoArchiveDuration.OneDay);
                                }

                                DiscordEmbedBuilder discordEmbedBuilder = new()
                                {
                                    Color = DiscordColor.Purple
                                };
                                discordEmbedBuilder.WithFooter(discordMemberItem.VoiceState.Guild.Name, discordMemberItem.VoiceState.Guild.IconUrl);
                                discordEmbedBuilder.WithDescription(desctiprion);
                                
                                var messages = await discordThreadsChannel.GetMessagesAsync();

                                var inviteLink = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();

                                DiscordMessage discordMessage = null;
                                var content = $"<#{discordMemberItem.VoiceState.Channel.Id}> \n\n {inviteLink} \n\n" + "+3|\\\\/||>";

                                foreach (var messageItem in messages.Where(x => x.Content.Contains("+3|\\\\/||>")))
                                {
                                    discordMessage = messageItem;
                                }

                                if (discordMessage == null)
                                {
                                    await discordThreadsChannel.SendMessageAsync(content, discordEmbedBuilder.Build());
                                }
                                else
                                {
                                    var discordEmbed = discordMessage.Embeds.FirstOrDefault();
                                    
                                    if (discordEmbed.Description != desctiprion || discordMessage.Content != content)
                                    {
                                        await discordMessage.ModifyAsync(content, discordEmbedBuilder.Build());
                                    }
                                }
                            }
                        }
                    }

                    if (!voiceStateAny)
                    {
                        var mainGuild = Bot.Client.GetGuildAsync(928930967140331590).Result;

                        var discordThreads = mainGuild.Threads.Values.ToList();

                        foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315cl0wn"))
                        {
                            await discordThreadItem.DeleteAsync();
                        }

                        var discordChannelOtherPlaces = mainGuild.GetChannel(981280701066395709);

                        var messages = discordChannelOtherPlaces.GetMessagesAsync().Result;

                        foreach (var messageItem in messages.Where(x => x.Content == "wh3r315cl0wn"))
                        {
                            await messageItem.DeleteAsync();
                        }
                    }

                    await Task.Delay(1000 * 1);
                }
            });
        }
    }
}
