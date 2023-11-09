using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DisCatSharp.Entities;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.Services
{
    public static class WhereIs
    {
        public enum MessageColor
        {
            DarkGrey,
            Red,
            Green,
            Yellow,
            Blue,
            Magenta,
            Cyan,
            White
        }

        public static void RunAsync(int executeSecond)
        {
            CustomLogger.Information("Starting WhereIsClown...", ConsoleColor.Green);
            Task.Run(async () =>
            {
                if (DiscordBot.DiscordClient.CurrentUser.Id != 890063457246937129)
                {
                    return;
                }

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

                foreach (DiscordGuild guild in guildList)
                {
                    CustomLogger.Information($"Guild: {guild.Id} {guild.Name}", ConsoleColor.Green);
                }

                DiscordGuild mainGuild = DiscordBot.DiscordClient.GetGuildAsync(928930967140331590).Result;
                DiscordChannel discordChannelWhereIs = mainGuild.GetChannel(1088859872029843746);
                List<ulong> ulongBlacklist = new()
                {
                            1088454540329746442,
                            918232272732319744,
                            631177569021984811,
                            180745071656697856
                };

                foreach (ulong discordGuild in ulongBlacklist)
                {
                    try
                    {
                        DiscordGuild guild = await DiscordBot.DiscordClient.GetGuildAsync(discordGuild);
                        guildList.Remove(guild);
                    }
                    catch (Exception exception)
                    {
                        CustomLogger.Information($"Guild: {exception.Message}", ConsoleColor.Red);
                    }
                }

                //guildList.Remove(DiscordBot.DiscordClient.GetGuildAsync(858089281214087179).Result); 
                //1088454540329746442
                //1106648403900891206 ?

                while (true)
                {
                    try
                    {
                        while (DateTime.Now.Second != executeSecond)
                        {
                            await Task.Delay(1000);
                        }

                        bool getMessagesOncePerGuild = false;
                        List<DiscordMessage> discordMessagesList = new();
                        List<DiscordMember> discordMemberConnectedList = new();
                        DiscordMember lastDiscordMember = default;
                        //IReadOnlyList<DiscordMessage> discordChannelWhereIsMessageAny = await discordChannelWhereIs.GetMessagesAsync(1);
                        foreach (List<DiscordMember> discordMemberList in guildList.Select(guildItem => guildItem.Members.Values.ToList()))
                        {
                            discordMemberConnectedList.AddRange(discordMemberList.Where(discordMemberItem => discordMemberItem.VoiceState != null));

                            List<DiscordMember> discordMemberConnectedListSorted = discordMemberConnectedList.OrderBy(discordMemberItem => discordMemberItem.VoiceState.Channel.Id).ToList();

                            foreach (DiscordMember discordMemberItem in discordMemberConnectedListSorted)
                            {
                                try
                                {
                                    lastDiscordMember ??= discordMemberItem;
                                    if (lastDiscordMember.VoiceState == null || discordMemberItem.VoiceState == null || (lastDiscordMember.VoiceState.Channel.Id == discordMemberItem.VoiceState.Channel.Id && lastDiscordMember != discordMemberItem))
                                    {
                                        lastDiscordMember = discordMemberItem;
                                        continue;
                                    }

                                    DiscordVoiceState discordVoiceState = discordMemberItem.VoiceState;

                                    List<DiscordMember> discordMembersInChannel = discordVoiceState.Channel.Users.ToList();
                                    List<DiscordMember> discordMembersInChannelSorted = discordMembersInChannel.OrderBy(x => x.VoiceState.IsSelfStream).ToList();
                                    discordMembersInChannelSorted.Reverse();

                                    //string description = Colored($"\ud83d\udd08 {discordVoiceState.Channel.Name,-69}\n\u001b[2;30min {discordVoiceState.Guild.Name}\u001b[0m", MessageColor.White);
                                    string description = "";
                                    string descriptionForConsole = "";
                                    foreach (DiscordMember discordMemberInChannelItem in discordMembersInChannelSorted)
                                    {
                                        string descriptionLineBuilder = "";
                                        string descriptionLineBuilderForConsole = "";
                                        int counter = 5;

                                        //Prob not needed
                                        StringBuilder stringBuilder = new();
                                        foreach (char c in discordMemberInChannelItem.Username.Where(c => c < 255))
                                        {
                                            stringBuilder.Append(c);
                                        }

                                        string username = stringBuilder.ToString();
                                        /*if (username is "" or " ")
                                        {
                                           username = discordMemberInChannelItem.Discriminator;
                                        }*/

                                        description += "<:xx_talk:989518547803848704>" + "``" + username.PadRight(17).Remove(17) + "``";
                                        descriptionForConsole += "" + username.PadRight(17).Remove(17) + "   |   ";

                                        if (discordMemberInChannelItem.VoiceState is
                                            {
                                                        IsSelfMuted: true
                                            })
                                        {
                                            descriptionLineBuilder += "<:xx_mute:989518546541346856>";
                                            descriptionLineBuilderForConsole += "M";
                                            counter--;
                                        }

                                        if (discordMemberInChannelItem.VoiceState is
                                            {
                                                        IsSelfDeafened: true
                                            })
                                        {
                                            descriptionLineBuilder += "<:xx_deaf:989518540400906270>";
                                            descriptionLineBuilderForConsole += "D";
                                            counter--;
                                        }

                                        if (discordMemberInChannelItem.VoiceState is
                                            {
                                                        IsSelfVideo: true
                                            })
                                        {
                                            descriptionLineBuilder += "<:xx_cam:989518538819645460>";
                                            descriptionLineBuilderForConsole += "C";
                                            counter--;
                                        }

                                        if (discordMemberInChannelItem.VoiceState is
                                            {
                                                        IsSelfStream: true
                                            })
                                        {
                                            descriptionLineBuilder += "<:xx_live_li:989518543886356510><:xx_live_ve:989518545245327449>";
                                            descriptionLineBuilderForConsole += " L";
                                            counter--;
                                            counter--;
                                        }

                                        for (int i = 0; i < counter; i++)
                                        {
                                            description += "<:xx_empty:989518542456123442>";
                                            descriptionForConsole += " ";
                                        }

                                        description += descriptionLineBuilder + "\n";
                                        descriptionForConsole += descriptionLineBuilderForConsole + "\n";
                                    }

                                    //description += "⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀";
                                    DiscordEmbedBuilder discordEmbedBuilder = new()
                                    {
                                                Color = new DiscordColor(43, 45, 49)
                                    };
                                    //discordEmbedBuilder.WithFooter(discordVoiceState.Guild.Name + " | " + discordVoiceState.Channel.Name, discordVoiceState.Guild.IconUrl);
                                    discordEmbedBuilder.WithFooter("\u2800                                                                                                       \u2800");
                                    discordEmbedBuilder.WithTimestamp(DateTime.Now);

                                    if (!getMessagesOncePerGuild)
                                    {
                                        IReadOnlyList<DiscordMessage> messages = await discordChannelWhereIs.GetMessagesAsync();
                                        discordMessagesList.AddRange(messages);
                                        getMessagesOncePerGuild = true;
                                    }

                                    DiscordMessage discordMessage = default;

                                    //string append = "";
                                    //if(discordChannelWhereIsMessageAny.Any())

                                    //string content = $"``                                                         ``\n|| https://discord.com/channels/{discordVoiceState.Guild.Id}/{discordVoiceState.Channel.Id} ||";
                                    string content = $"\u2800\n|| https://discord.com/channels/{discordVoiceState.Guild.Id}/{discordVoiceState.Channel.Id} ||";
                                    if (discordMessagesList != null)
                                    {
                                        foreach (DiscordMessage messageItem in discordMessagesList.Where(x => x.Content.Contains(content)))
                                        {
                                            discordMessage = messageItem;
                                            break;
                                        }
                                    }

                                    DiscordComponentEmoji discordComponentEmojisJoinChannel = new("📞");
                                    DiscordComponentEmoji discordComponentEmojisJoinServer = new("🛡");
                                    DiscordComponent[] discordComponents = new DiscordComponent[2];

                                    DiscordChannel defaultDiscordChannel = discordVoiceState.Guild.GetDefaultChannel();
                                    IReadOnlyList<DiscordInvite> discordServerInvites = await defaultDiscordChannel.GetInvitesAsync();
                                    DiscordInvite discordServerInvite = discordServerInvites.FirstOrDefault(x => x.Inviter.Id == DiscordBot.DiscordClient.CurrentUser.Id && x.Channel.Id == defaultDiscordChannel.Id);
                                    discordServerInvite ??= await defaultDiscordChannel.CreateInviteAsync();

                                    discordComponents[1] = new DiscordLinkButtonComponent(discordServerInvite.Url, "Join server!", false, discordComponentEmojisJoinServer);

                                    DiscordInvite discordChannelInvite;
                                    if (discordMessage == null)
                                    {
                                        discordChannelInvite = await discordVoiceState.Channel.CreateInviteAsync();
                                        discordEmbedBuilder.WithDescription(description);
                                        discordComponents[0] = new DiscordLinkButtonComponent(discordChannelInvite.Url, "Join channel!", false, discordComponentEmojisJoinChannel);

                                        discordMessagesList.Add(await discordChannelWhereIs.SendMessageAsync(new DiscordMessageBuilder().WithContent(content).AddEmbed(discordEmbedBuilder.Build())));
                                        //discordMessagesList.Add(await discordChannelWhereIs.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).WithContent(content).AddEmbed(discordEmbedBuilder.Build())));
                                    }
                                    else
                                    {
                                        IReadOnlyList<DiscordInvite> discordChannelInvites = await discordVoiceState.Channel.GetInvitesAsync();
                                        discordChannelInvite = discordChannelInvites.FirstOrDefault(x => x.Inviter.Id == DiscordBot.DiscordClient.CurrentUser.Id && x.Channel.Id == discordVoiceState.Channel.Id);

                                        discordChannelInvite ??= await discordVoiceState.Channel.CreateInviteAsync();

                                        discordEmbedBuilder.WithDescription(description);
                                        discordComponents[0] = new DiscordLinkButtonComponent(discordChannelInvite.Url, "Join channel!", false, discordComponentEmojisJoinChannel);
                                        await discordMessage.ModifyAsync(x => x.WithContent(content).WithEmbed(discordEmbedBuilder.Build()));
                                        await Task.Delay(5000);
                                        //await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(content).WithEmbed(discordEmbedBuilder.Build()));
                                    }

                                    lastDiscordMember = discordMemberItem;
                                    //CustomLogger.Green(discordMemberItem.Guild.Id + "   " + discordMemberItem.Guild.Name + "\n\n" + descriptionForConsole);
                                    await Task.Delay(2000);
                                }
                                catch (Exception ex)
                                {
                                    CustomLogger.Error(ex);
                                }
                            }

                            getMessagesOncePerGuild = false;
                            discordMessagesList?.Clear();
                            discordMemberConnectedList.Clear();
                            discordMemberConnectedListSorted.Clear();
                        }

                        IReadOnlyList<DiscordMessage> discordMessages = await discordChannelWhereIs.GetMessagesAsync();

                        foreach (DiscordMessage discordMessage in discordMessages)
                        {
                            string mentionedChannel = "";
                            try
                            {
                                mentionedChannel = StringCutter.RemoveUntil(discordMessage.Content, "https://discord.com/channels/", "https://discord.com/channels/".Length);
                                mentionedChannel = StringCutter.RemoveUntil(mentionedChannel, "/", 1);
                                mentionedChannel = StringCutter.RemoveAfter(mentionedChannel, " ||", 0);
                            }
                            catch
                            {
                                // ignored
                            }

                            DiscordChannel discordChannel = null;
                            try
                            {
                                if (mentionedChannel != null)
                                {
                                    discordChannel = DiscordBot.DiscordClient.GetChannelAsync(Convert.ToUInt64(mentionedChannel)).Result;
                                }
                            }
                            catch
                            {
                                // ignored
                            }

                            if (discordChannel == null || !discordChannel.Users.Any())
                            {
                                await discordMessage.DeleteAsync();
                            }
                        }

                        await Task.Delay(1000);

                        CustomLogger.Information("Finished", ConsoleColor.Green);

                        if (!LastMinuteCheck.WhereIsClownRunAsync)
                        {
                            LastMinuteCheck.WhereIsClownRunAsync = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Error(ex);
                    }
                }
            });
        }

        public static string Colored(string message, MessageColor messageColor)
        {
            int padding = 0; //75 - message.Length;
            switch (messageColor)
            {
                case MessageColor.DarkGrey:
                    return $"```ansi\n\u001b[2;30m{message}\u001b[0m{"".PadLeft(padding)}\n```";
                case MessageColor.Red:
                    return $"```ansi\n\u001b[2;31m{message}\u001b[0m{"".PadLeft(padding)}\n```";
                case MessageColor.Green:
                    return $"```ansi\n\u001b[2;32m{message}\u001b[0m{"".PadLeft(padding)}\n```";
                case MessageColor.Yellow:
                    return $"```ansi\n\u001b[2;33m{message}\u001b[0m{"".PadLeft(padding)}\n```";
                case MessageColor.Blue:
                    return $"```ansi\n\u001b[2;34m{message}\u001b[0m{"".PadLeft(padding)}\n```";
                case MessageColor.Magenta:
                    return $"```ansi\n\u001b[2;35m{message}\u001b[0m{"".PadLeft(padding)}\n```";
                case MessageColor.Cyan:
                    return $"```ansi\n\u001b[2;36m{message}\u001b[0m{"".PadLeft(padding)}\n```";
                case MessageColor.White:
                    return $"```ansi\n\u001b[2;37m{message}\u001b[0m{"".PadLeft(padding)}\n```";
            }

            return message;
        }
    }
}