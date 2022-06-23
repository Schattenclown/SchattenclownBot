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
                List<DiscordGuild> guildList;
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
                    List<DiscordThreadChannel> discordThreads;
                    var discordInvite = default(DiscordInvite);
                    var getMessagesOncePerGuild = false;
                    var discordMessagesList = new List<DiscordMessage>();
                    var discordemojis = mainGuild.GetEmojisAsync().Result;
                    var discordMemberConnectedList = new List<DiscordMember>();
                    var lastDiscordMember = default(DiscordMember);

                    foreach (var guildItem in guildList)
                    {
                        var discordMemberList = guildItem.Members.Values.ToList();

                        discordMemberConnectedList.AddRange(discordMemberList.Where(discordMemberItem => discordMemberItem.VoiceState != null && discordMemberItem.Guild.Id != 975889218968629298));

                        var discordMemberConnectedListSorted = discordMemberConnectedList.OrderBy(x => x.VoiceState.Channel.Id).ToList();

                        foreach (var discordMemberItem in discordMemberConnectedListSorted)
                        {
                            if (lastDiscordMember == null)
                                lastDiscordMember = discordMemberItem;

                            if ((discordMemberItem.VoiceState == null || discordMemberItem.Guild.Id == 975889218968629298) || (lastDiscordMember.VoiceState.Channel.Id == discordMemberItem.VoiceState.Channel.Id && lastDiscordMember != discordMemberItem))
                                continue;
                            voiceStateAny = true;

                            var discordMembersInChannel = discordMemberItem.VoiceState.Channel.Users.ToList();
                            var discordMembersInChannelSorted = discordMembersInChannel.OrderBy(x => x.VoiceState.IsSelfStream).ToList();
                            discordMembersInChannelSorted.Reverse();

                            var description = "";
                            foreach (var discordMemberInChannelItem in discordMembersInChannelSorted)
                            {
                                var descriptionLineBuilder = "";
                                var counter = 5;
                                var username = RemoveSpecialCharacters(discordMemberInChannelItem.DisplayName);
                                if (username is "" or " ")
                                    username = discordMemberInChannelItem.Discriminator;
                                description += "<:xx_talk:989518547803848704>" + "``" + username.PadRight(16).Remove(16) + "``";

                                if (discordMemberInChannelItem.VoiceState.IsSelfMuted)
                                {
                                    descriptionLineBuilder += "<:xx_mute:989518546541346856>";
                                    counter--;
                                }
                                if (discordMemberInChannelItem.VoiceState.IsSelfDeafened)
                                {
                                    descriptionLineBuilder += "<:xx_deaf:989518540400906270>";
                                    counter--;
                                }
                                if (discordMemberInChannelItem.VoiceState.IsSelfVideo)
                                {
                                    descriptionLineBuilder += "<:xx_cam:989518538819645460>";
                                    counter--;
                                }
                                if (discordMemberInChannelItem.VoiceState.IsSelfStream)
                                {
                                    descriptionLineBuilder += "<:xx_live_li:989518543886356510><:xx_live_ve:989518545245327449>";
                                    counter--; counter--;
                                }

                                for (var i = 0; i < counter; i++)
                                {
                                    description += "<:xx_empty:989518542456123442>";
                                }

                                description += descriptionLineBuilder + "\n";

                            }

                            discordThreads = mainGuild.Threads.Values.ToList();

                            var discordThreadsChannel = discordThreads.FirstOrDefault(x => x.Name == "wh3r315");

                            if (discordThreadsChannel == null)
                            {
                                discordThreadsChannel = await discordChannelOtherPlaces.CreateThreadAsync("wh3r315", ThreadAutoArchiveDuration.OneDay);
                            }

                            DiscordEmbedBuilder discordEmbedBuilder = new()
                            {
                                Color = new DiscordColor(17, 17, 17)
                            };
                            discordEmbedBuilder.WithFooter(discordMemberItem.VoiceState.Guild.Name + " | " + discordMemberItem.VoiceState.Channel.Name, discordMemberItem.VoiceState.Guild.IconUrl);

                            if (!getMessagesOncePerGuild)
                            {
                                var messages = await discordThreadsChannel.GetMessagesAsync();
                                discordMessagesList.AddRange(messages);
                                getMessagesOncePerGuild = true;
                            }

                            var discordMessage = default(DiscordMessage);
                            var content = $"<#{discordMemberItem.VoiceState.Channel.Id}>";
                            if (discordMessagesList != null)
                                foreach (var messageItem in discordMessagesList.Where(x => x.Content.Contains(content)))
                                {
                                    discordMessage = messageItem;
                                    break;
                                }

                            if (discordMessage == null)
                            {
                                discordInvite = await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                discordEmbedBuilder.WithDescription(description + $"\n[Join Server and Channel]({discordInvite})");
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

                                    discordInvite ??= await discordMemberItem.VoiceState.Channel.CreateInviteAsync();
                                }

                                discordEmbedBuilder.WithDescription(description + $"\n[Join Server and Channel]({discordInvite})");
                                var discordEmbed = discordMessage.Embeds.FirstOrDefault();

                                if (discordEmbed.Description != description || discordMessage.Content != content)
                                {
                                    await discordMessage.ModifyAsync(content, discordEmbedBuilder.Build());
                                }
                            }
                            lastDiscordMember = discordMemberItem;
                            await Task.Delay(2000);
                        }

                        discordInvite = null;
                        getMessagesOncePerGuild = false;
                        discordMessagesList?.Clear();
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
                                mentionedChannel = StringCutter.RemoveAfterWord(mentionedChannel, ">", 0);
                            }
                            catch
                            {
                                // ignored
                            }

                            DiscordChannel discordChannel = null;
                            try
                            {
                                if (mentionedChannel != null)
                                    discordChannel = Bot.Client.GetChannelAsync(Convert.ToUInt64(mentionedChannel)).Result;
                            }
                            catch
                            {
                                // ignored
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

                    await Task.Delay(5000);
                }
            });
        }
        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new();
            foreach (var c in str.Where(c => c < 255))
            {
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}
