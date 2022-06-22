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
                var guildList = Bot.Client.Guilds.Values.ToList();
                do
                {
                    guildList = Bot.Client.Guilds.Values.ToList();
                    await Task.Delay(1000);

                } while (guildList.Count == 0);

                while (true)
                {
                    /*while (DateTime.Now.Second != executeSecond)
                    {
                        await Task.Delay(1000);
                    }*/

                    var voiceStateAny = false;
                    var mainGuild = Bot.Client.GetGuildAsync(928930967140331590).Result;
                    var discordThreads = mainGuild.Threads.Values.ToList();
                    var discordEmojiLive = mainGuild.GetEmojisAsync().Result;
                    var discordChannelOtherPlaces = mainGuild.GetChannel(987123289619071026);
                    var discordMessagesReadOnce = false;
                    var discordThreadChannelReadOnce = false;
                    List<DiscordMessage> discordMessagesList = new();
                    List<DiscordThreadChannel> DiscordThreadChannelList = new();

                    foreach (var guildItem in guildList)
                    {
                        var discordMemberList = guildItem.Members.Values.ToList();

                        foreach (var discordMemberItem in discordMemberList)
                        {
                            if (discordMemberItem.VoiceState != null && discordMemberItem.Guild.Id != 975889218968629298)
                            {
                                voiceStateAny = true;

                                var desctiprion = "";
                                foreach (var discordMemberInChannelItem in discordMemberItem.VoiceState.Channel.Users.ToList())
                                {
                                    string desctiprionLineBuilder = "";
                                    int counter = 5;
                                    
                                    desctiprion += "<:x_talk:988942227004858438> " + "``" + RemoveSpecialCharacters(discordMemberInChannelItem.DisplayName).PadRight(16).Remove(16) + "`` ";

                                    if (discordMemberInChannelItem.VoiceState.IsSelfMuted)
                                    {
                                        desctiprionLineBuilder += "<:x_mute:989191045994668072> ";
                                        counter--;
                                    }
                                    if (discordMemberInChannelItem.VoiceState.IsSelfDeafened)
                                    {
                                        desctiprionLineBuilder += "<:x_deaf:989191057919066203> ";
                                        counter--;
                                    }
                                    if (discordMemberInChannelItem.VoiceState.IsSelfVideo)
                                    {
                                        desctiprionLineBuilder += "<:x_cam:989194342222663680> ";
                                        counter--;
                                    }
                                    if (discordMemberInChannelItem.VoiceState.IsSelfStream)
                                    {
                                        desctiprionLineBuilder += "<:x_li:989178879782572062><:x_ve:989178889861472368>";
                                        counter--; counter--;
                                    }

                                    for (int i = 0; i < counter; i++)
                                    {
                                        desctiprion += "<:x_emty:988948135600599080> ";
                                    }

                                    desctiprion += desctiprionLineBuilder + "\n";

                                }

                                DiscordThreadChannel discordThreadsChannel = null;
                                if (!discordThreadChannelReadOnce)
                                {
                                    var discordThreadsListVar = mainGuild.Threads.Values.ToList();
                                    foreach (var discordThreadItem in discordThreadsListVar)
                                    {
                                        DiscordThreadChannelList.Add(discordThreadItem);
                                    }
                                    discordThreadChannelReadOnce = true;
                                }

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
                                    Color = new DiscordColor(17, 17, 17)
                                };
                                discordEmbedBuilder.WithFooter(discordMemberItem.VoiceState.Guild.Name + " | " + discordMemberItem.VoiceState.Channel.Name, discordMemberItem.VoiceState.Guild.IconUrl);

                                
                                if (!discordMessagesReadOnce)
                                {
                                    var discordMessagesListVar = await discordThreadsChannel.GetMessagesAsync();
                                    foreach (var discordMessageItem in discordMessagesListVar)
                                    {
                                        discordMessagesList.Add(discordMessageItem);
                                    }
                                    discordMessagesReadOnce = true;
                                }

                                DiscordMessage discordMessage = null;
                                var content = $"<#{discordMemberItem.VoiceState.Channel.Id}>" + "\n+3|\\\\/||>";

                                foreach (var messageItem in discordMessagesList.Where(x => x.Content.Contains($"<#{discordMemberItem.VoiceState.Channel.Id}>" + "\n+3|\\\\/||>")))
                                {
                                    discordMessage = messageItem;
                                }

                                if (discordMessage == null)
                                {
                                    var discordInvite = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                    discordEmbedBuilder.WithDescription(desctiprion + $"\n[Join Server and Channel]({discordInvite})");
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

                                    discordEmbedBuilder.WithDescription(desctiprion + $"\n[Join Server and Channel]({discordInvite})");
                                    var discordEmbed = discordMessage.Embeds.FirstOrDefault();

                                    if (discordEmbed.Description != desctiprion || discordMessage.Content != content)
                                    {
                                        await discordMessage.ModifyAsync(content, discordEmbedBuilder.Build());
                                    }
                                }
                            }
                        }
                    }

                    discordThreads = mainGuild.Threads.Values.ToList();
                    foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                    {
                        var messagess = discordThreadItem.GetMessagesAsync().Result;

                        foreach (var messageItem in messagess)
                        {
                            var mentionedChannel = "";
                            try
                            {
                                mentionedChannel = StringCutter.RemoveUntilWord(messageItem.Content, "<#", 2);
                                mentionedChannel = StringCutter.RemoveAfterWord(mentionedChannel, "\n+3|\\\\/||>", -1);
                            }
                            catch
                            {

                            }

                            DiscordChannel discordChannel = null;
                            try
                            {
                                if (mentionedChannel != null)
                                    discordChannel = Bot.Client.GetChannelAsync(Convert.ToUInt64(mentionedChannel)).Result;
                            }
                            catch
                            {

                            }

                            if (discordChannel == null || !discordChannel.Users.Any())
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

                        var messagess = discordChannelOtherPlaces.GetMessagesAsync().Result;

                        foreach (var messageItem in messagess.Where(x => x.Content == "wh3r315"))
                        {
                            await messageItem.DeleteAsync();
                        }
                    }
                    DiscordThreadChannelList.Clear();
                    discordThreadChannelReadOnce = false;

                    discordMessagesList.Clear();
                    discordMessagesReadOnce = false;
                    await Task.Delay(1000 * 60);
                }
            });
        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new();
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c < 255)
                {
                    sb.Append(str[i]);
                }
            }

            return sb.ToString();
        }
    }
}
