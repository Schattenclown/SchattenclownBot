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
                await Task.Delay(4000);
                var guildList = Bot.Client.Guilds.Values.ToList();
                do
                {
                    guildList = Bot.Client.Guilds.Values.ToList();
                    await Task.Delay(1000);
                } while (guildList.Count == 0);

                var mainGuild = Bot.Client.GetGuildAsync(928930967140331590).Result;
                var discordChannelOtherPlaces = mainGuild.GetChannel(987123289619071026);

                while (true)
                {
                    while (DateTime.Now.Second != executeSecond)
                    {
                        await Task.Delay(1000);
                    }

                    var voiceStateAny = false;
                    var discordThreads = mainGuild.Threads.Values.ToList();
                    var discordInvite = default(DiscordInvite);
                    var getMessagesOncePerGuilde = false;
                    var discordMessagesList = new List<DiscordMessage>();
                    var discordMemberConnectedList = new List<DiscordMember>();
                    var discordMemberConnectedListSorted = new List<DiscordMember>();
                    var lastDiscordMember = default(DiscordMember);

                    foreach (var guildItem in guildList)
                    {
                        var discordMemberList = guildItem.Members.Values.ToList();

                        foreach (var discordMemberItem in discordMemberList)
                        {
                            if (discordMemberItem.VoiceState != null && discordMemberItem.Guild.Id != 975889218968629298)
                                discordMemberConnectedList.Add(discordMemberItem);
                        }

                        discordMemberConnectedListSorted = discordMemberConnectedList.OrderBy(x => x.VoiceState.Channel.Id).ToList();
                        
                        foreach (var discordMemberItem in discordMemberConnectedListSorted)
                        {
                            if(lastDiscordMember == null)
                                lastDiscordMember = discordMemberItem;

                            if ((discordMemberItem.VoiceState != null && discordMemberItem.Guild.Id != 975889218968629298) && (lastDiscordMember.VoiceState.Channel.Id != discordMemberItem.VoiceState.Channel.Id || lastDiscordMember == discordMemberItem))
                            {
                                voiceStateAny = true;

                                List<DiscordMember> discordMembersInChannel = discordMemberItem.VoiceState.Channel.Users.ToList();
                                List<DiscordMember> discordMembersInChannelSotrted = discordMembersInChannel.OrderBy(x => x.VoiceState.IsSelfStream).ToList();
                                discordMembersInChannelSotrted.Reverse();

                                var desctiprion = "";
                                foreach (var discordMemberInChannelItem in discordMembersInChannelSotrted)
                                {
                                    string desctiprionLineBuilder = "";
                                    int counter = 5;
                                    string username = RemoveSpecialCharacters(discordMemberInChannelItem.DisplayName).PadRight(16).Remove(16);
                                    if (username == "")
                                        username = discordMemberInChannelItem.Discriminator;
                                    desctiprion += "<:x_talk:988942227004858438> " + "``" + username + "`` ";

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
                                discordThreads = mainGuild.Threads.Values.ToList();

                                foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                                {
                                    discordThreadsChannel = discordThreadItem;
                                    break;
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

                                if(!getMessagesOncePerGuilde)
                                {
                                    var messages = await discordThreadsChannel.GetMessagesAsync();
                                    foreach (var message in messages)
                                    {
                                        discordMessagesList.Add(message);
                                    }
                                    getMessagesOncePerGuilde = true;
                                }

                                var discordMessage = default(DiscordMessage);
                                var content = $"<#{discordMemberItem.VoiceState.Channel.Id}>" + "\n+3|\\\\/||>";
                                if (discordMessagesList != null)
                                    foreach (var messageItem in discordMessagesList.Where(x => x.Content.Contains(content)))
                                    {
                                        discordMessage = messageItem;
                                        break;
                                    }

                                if (discordMessage == null)
                                {
                                    discordInvite = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                    discordEmbedBuilder.WithDescription(desctiprion + $"\n[Join Server and Channel]({discordInvite})");
                                    discordMessagesList.Add(await discordThreadsChannel.SendMessageAsync(content, discordEmbedBuilder.Build()));
                                    
                                }
                                else
                                {
                                    if (discordInvite == null)
                                    {
                                        var discordInvites = await discordMemberItem.VoiceState.Channel.GetInvitesAsync();

                                        foreach (var invite in discordInvites.Where(x => x.Inviter.Id == Bot.Client.CurrentUser.Id))
                                        {
                                            discordInvite = invite;
                                            break;
                                        }

                                        if (discordInvite == null)
                                        {
                                            discordInvite = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                        }
                                    }

                                    discordEmbedBuilder.WithDescription(desctiprion + $"\n[Join Server and Channel]({discordInvite})");
                                    var discordEmbed = discordMessage.Embeds.FirstOrDefault();

                                    if (discordEmbed.Description != desctiprion || discordMessage.Content != content)
                                    {
                                        await discordMessage.ModifyAsync(content, discordEmbedBuilder.Build());
                                    }
                                }
                                lastDiscordMember = discordMemberItem;
                                await Task.Delay(2000);
                            }
                        }

                        discordInvite = null;
                        getMessagesOncePerGuilde = false;
                        if (discordMessagesList != null)
                            discordMessagesList.Clear();
                        discordMemberConnectedList.Clear();
                        discordMemberConnectedListSorted.Clear();
                    }

                    discordThreads = mainGuild.Threads.Values.ToList();
                    foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                    {
                        var messages = discordThreadItem.GetMessagesAsync().Result;

                        foreach (var messageItem in messages)
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
                            await Task.Delay(1000);
                        }
                    }

                    if (!voiceStateAny)
                    {
                        foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                        {
                            await discordThreadItem.DeleteAsync();
                        }

                        var messages = discordChannelOtherPlaces.GetMessagesAsync().Result;

                        foreach (var messageItem in messages.Where(x => x.Content == "wh3r315"))
                        {
                            await messageItem.DeleteAsync();
                        }
                    }

                    await Task.Delay(1000);
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
