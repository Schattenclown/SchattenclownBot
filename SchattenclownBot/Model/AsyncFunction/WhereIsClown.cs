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
                    /*while (DateTime.Now.Second != executeSecond)
                    {
                        await Task.Delay(1000);
                    }*/

                    var guildList = Bot.Client.Guilds.Values.ToList();

                    var voiceStateAny = false;

                    foreach (var guildItem in guildList)
                    {
                        var discordMemberList = guildItem.Members.Values.ToList();

                        foreach (var discordMemberItem in discordMemberList)
                        {
                            if (discordMemberItem.Id == 444152594898878474 && discordMemberItem.VoiceState != null && discordMemberItem.Guild.Id != 928930967140331590)
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

                                var discordChannelOtherPlaces = mainGuild.GetChannel(987123289619071026);

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
                                
                                var messages = await discordThreadsChannel.GetMessagesAsync();

                                DiscordMessage discordMessage = null;
                                var content = $"<#{discordMemberItem.VoiceState.Channel.Id}> \n\n" + "+3|\\\\/||>";

                                foreach (var messageItem in messages.Where(x => x.Content.Contains("+3|\\\\/||>")))
                                {
                                    discordMessage = messageItem;
                                }

                                if (discordMessage == null)
                                {
                                    var discordInvite = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                    desctiprion += $"\n\n[Join Server {discordInvite.Channel.Name}]({discordInvite})";
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

                                    if(discordInvite == null)
                                    {
                                        discordInvite = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                    }

                                    desctiprion += $"\n\n[Join Server {discordInvite.Channel.Name}]({discordInvite})";
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

                    if (!voiceStateAny)
                    {
                        var mainGuild = Bot.Client.GetGuildAsync(928930967140331590).Result;

                        var discordThreads = mainGuild.Threads.Values.ToList();

                        foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315cl0wn"))
                        {
                            await discordThreadItem.DeleteAsync();
                        }

                        var discordChannelOtherPlaces = mainGuild.GetChannel(987123289619071026);

                        var messages = discordChannelOtherPlaces.GetMessagesAsync().Result;

                        foreach (var messageItem in messages.Where(x => x.Content == "wh3r315cl0wn"))
                        {
                            await messageItem.DeleteAsync();
                        }
                    }

                    await Task.Delay(1000 * 20);
                }
            });
        }
    }
}
