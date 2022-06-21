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
using SchattenclownBot.Model.HelpClasses;

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
                    var mainGuild = Bot.Client.GetGuildAsync(928930967140331590).Result;
                    var discordThreads = mainGuild.Threads.Values.ToList();

                    foreach (var guildItem in guildList)
                    {
                        var discordMemberList = guildItem.Members.Values.ToList();

                        foreach (var discordMemberItem in discordMemberList)
                        {
                            //if (discordMemberItem.Id == 444152594898878474 && discordMemberItem.VoiceState != null && discordMemberItem.Guild.Id != 928930967140331590)
                            if (discordMemberItem.VoiceState != null && discordMemberItem.Guild.Id != 975889218968629298)
                            {
                                voiceStateAny = true;

                                var desctiprion = "";
                                foreach (var discordMemberInChannelItem in discordMemberItem.VoiceState.Channel.Users.ToList())
                                {
                                    if (discordMemberInChannelItem.VoiceState.IsSelfDeafened)
                                        desctiprion += ":mute: ";
                                    else
                                        desctiprion += ":green_circle: ";

                                    if (discordMemberInChannelItem.VoiceState.IsSelfVideo)
                                        desctiprion += ":video_camera: ";
                                    else
                                        desctiprion += ":black_medium_square: ";

                                    if (discordMemberInChannelItem.VoiceState.IsSelfStream)
                                        desctiprion += ":tv: ";
                                    else
                                        desctiprion += ":black_medium_square: ";

                                    desctiprion += discordMemberInChannelItem.DisplayName + "\n";
                                }

                                var discordChannelOtherPlaces = mainGuild.GetChannel(987123289619071026);

                                DiscordThreadChannel discordThreadsChannel = null;

                                foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                                {
                                    discordThreadsChannel = discordThreadItem;
                                }

                                if (discordThreadsChannel == null)
                                {
                                    discordThreadsChannel = await discordChannelOtherPlaces.CreateThreadAsync("wh3r315", ThreadAutoArchiveDuration.OneDay);
                                }

                                DiscordEmbedBuilder discordEmbedBuilder = new()
                                {
                                    Color = DiscordColor.None
                                };
                                discordEmbedBuilder.WithFooter(discordMemberItem.VoiceState.Guild.Name, discordMemberItem.VoiceState.Guild.IconUrl);

                                var messages = await discordThreadsChannel.GetMessagesAsync();

                                DiscordMessage discordMessage = null;
                                var content = discordMemberItem.VoiceState.Channel.Id + " +3|\\\\/||>" + $"\n\n<#{discordMemberItem.VoiceState.Channel.Id}>";

                                foreach (var messageItem in messages.Where(x => x.Content.Contains(discordMemberItem.VoiceState.Channel.Id + " " + "+3|\\\\/||>")))
                                {
                                    discordMessage = messageItem;
                                }

                                if (discordMessage == null)
                                {
                                    var discordInvite = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                    desctiprion += $"\n[Join Server]({discordInvite})";
                                    discordEmbedBuilder.WithDescription(desctiprion);
                                    await discordThreadsChannel.SendMessageAsync(content, discordEmbedBuilder.Build());
                                }
                                else
                                {
                                    var discordInvites = discordMemberItem.VoiceState.Channel.GetInvitesAsync().Result;
                                    DiscordInvite discordInvite = null;
                                    foreach (var invite in discordInvites.Where(x => x.Inviter.Id == 890063457246937129))
                                    {
                                        discordInvite = invite;
                                    }

                                    if (discordInvite == null)
                                    {
                                        discordInvite = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                    }

                                    desctiprion += $"\n[Join Server]({discordInvite})";
                                    discordEmbedBuilder.WithDescription(desctiprion);
                                    var discordEmbed = discordMessage.Embeds.FirstOrDefault();

                                    if (discordEmbed.Description != desctiprion || discordMessage.Content != content)
                                    {
                                        await discordMessage.ModifyAsync(content, discordEmbedBuilder.Build());
                                    }
                                }
                            }
                        }
                    }

                    foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                    {
                        var messages = discordThreadItem.GetMessagesAsync().Result;

                        foreach (var messageItem in messages)
                        {
                            var mentionedChannels = "";
                            mentionedChannels = StringCutter.RemoveAfterWord(messageItem.Content, "+3|\\\\/||>", 0);
                            var discordChannel = Bot.Client.GetChannelAsync(Convert.ToUInt64(mentionedChannels)).Result;
                            if (!discordChannel.Users.Any())
                            {
                                await messageItem.DeleteAsync();
                            }
                        }
                    }

                    if (!voiceStateAny)
                    {
                        foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                        {
                            await discordThreadItem.DeleteAsync();
                        }

                        var discordChannelOtherPlaces = mainGuild.GetChannel(987123289619071026);

                        var messages = discordChannelOtherPlaces.GetMessagesAsync().Result;

                        foreach (var messageItem in messages.Where(x => x.Content == "wh3r315"))
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
