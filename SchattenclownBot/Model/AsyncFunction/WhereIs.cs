using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class WhereIs
   {
      public static async Task WhereIsClownRunAsync(int executeSecond)
      {
         await Task.Run(async () =>
         {
            if (Bot.DiscordClient.CurrentUser.Id != 890063457246937129)
               return;

            while (DateTime.Now.Second != executeSecond)
            {
               await Task.Delay(1000);
            }

            List<DiscordGuild> guildList;
            do
            {
               guildList = Bot.DiscordClient.Guilds.Values.ToList();
               await Task.Delay(1000);
            } while (guildList.Count == 0);

            DiscordGuild mainGuild = Bot.DiscordClient.GetGuildAsync(928930967140331590).Result;
            DiscordChannel discordChannelOtherPlaces = mainGuild.GetChannel(987123289619071026);

            while (true)
            {
               try
               {
                  while (DateTime.Now.Second != executeSecond)
                  {
                     await Task.Delay(1000);
                  }

                  bool voiceStateAny = false;
                  List<DiscordThreadChannel> discordThreads;
                  bool getMessagesOncePerGuild = false;
                  List<DiscordMessage> discordMessagesList = new();
                  List<DiscordMember> discordMemberConnectedList = new();
                  DiscordMember lastDiscordMember = default(DiscordMember);
                  guildList.Remove(Bot.DiscordClient.GetGuildAsync(858089281214087179).Result);

                  foreach (DiscordGuild guildItem in guildList)
                  {
                     List<DiscordMember> discordMemberList = guildItem.Members.Values.ToList();

                     discordMemberConnectedList.AddRange(discordMemberList.Where(discordMemberItem => discordMemberItem.VoiceState != null));
                     
                     List<DiscordMember> discordMemberConnectedListSorted = discordMemberConnectedList.OrderBy(discordMemberItem => discordMemberItem.VoiceState.Channel.Id).ToList();

                     foreach (DiscordMember discordMemberItem in discordMemberConnectedListSorted)
                     {
                        try
                        {
                           voiceStateAny = true;
                           lastDiscordMember ??= discordMemberItem;
                           DiscordVoiceState discordVoiceState;
                           if (lastDiscordMember.VoiceState == null || discordMemberItem.VoiceState == null || (lastDiscordMember.VoiceState.Channel.Id == discordMemberItem.VoiceState.Channel.Id && lastDiscordMember != discordMemberItem))
                           {
                              lastDiscordMember = discordMemberItem;
                              continue;
                           }
                           else
                           {
                              discordVoiceState = discordMemberItem.VoiceState;
                           }

                           List<DiscordMember> discordMembersInChannel = discordVoiceState.Channel.Users.ToList();
                           List<DiscordMember> discordMembersInChannelSorted = discordMembersInChannel.OrderBy(x => x.VoiceState.IsSelfStream).ToList();
                           discordMembersInChannelSorted.Reverse();

                           string description = "";
                           foreach (DiscordMember discordMemberInChannelItem in discordMembersInChannelSorted)
                           {
                              string descriptionLineBuilder = "";
                              int counter = 5;
                              string username = SpecialChars.RemoveSpecialCharacters(discordMemberInChannelItem.DisplayName);
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

                              for (int i = 0; i < counter; i++)
                              {
                                 description += "<:xx_empty:989518542456123442>";
                              }

                              description += descriptionLineBuilder + "\n";
                           }

                           discordThreads = mainGuild.Threads.Values.ToList();

                           DiscordThreadChannel discordThreadsChannel = discordThreads.FirstOrDefault(x => x.Name == "wh3r315");
                           discordThreadsChannel ??= await discordChannelOtherPlaces.CreateThreadAsync("wh3r315", ThreadAutoArchiveDuration.OneDay);

                           DiscordEmbedBuilder discordEmbedBuilder = new()
                           {
                              Color = new DiscordColor(17, 17, 17)
                           };
                           discordEmbedBuilder.WithFooter(discordVoiceState.Guild.Name + " | " + discordVoiceState.Channel.Name, discordVoiceState.Guild.IconUrl);
                           discordEmbedBuilder.WithTimestamp(DateTime.Now);

                           if (!getMessagesOncePerGuild)
                           {
                              IReadOnlyList<DiscordMessage> messages = await discordThreadsChannel.GetMessagesAsync();
                              discordMessagesList.AddRange(messages);
                              getMessagesOncePerGuild = true;
                           }

                           DiscordMessage discordMessage = default(DiscordMessage);
                           string content = $"<#{discordVoiceState.Channel.Id}>";
                           if (discordMessagesList != null)
                              foreach (DiscordMessage messageItem in discordMessagesList.Where(x => x.Content.Contains(content)))
                              {
                                 discordMessage = messageItem;
                                 break;
                              }

                           DiscordComponentEmoji discordComponentEmojisJoinChannel = new("📞");
                           DiscordComponentEmoji discordComponentEmojisJoinServer = new("🛡");
                           DiscordComponent[] discordComponents = new DiscordComponent[2];

                           DiscordChannel defaultDiscordChannel = discordVoiceState.Guild.GetDefaultChannel();
                           IReadOnlyList<DiscordInvite> discordServerInvites = await defaultDiscordChannel.GetInvitesAsync();
                           DiscordInvite discordServerInvite = discordServerInvites.FirstOrDefault(x => x.Inviter.Id == Bot.DiscordClient.CurrentUser.Id && x.Channel.Id == defaultDiscordChannel.Id);
                           discordServerInvite ??= await defaultDiscordChannel.CreateInviteAsync();

                           discordComponents[1] = new DiscordLinkButtonComponent(discordServerInvite.Url, "Join server!", false, discordComponentEmojisJoinServer);

                           DiscordInvite discordChannelInvite;
                           if (discordMessage == null)
                           {
                              discordChannelInvite = await discordVoiceState.Channel.CreateInviteAsync();
                              discordEmbedBuilder.WithDescription(description);
                              discordComponents[0] = new DiscordLinkButtonComponent(discordChannelInvite.Url, "Join channel!", false, discordComponentEmojisJoinChannel);

                              discordMessagesList.Add(await discordThreadsChannel.SendMessageAsync(new DiscordMessageBuilder().AddComponents(discordComponents).WithContent(content).AddEmbed(discordEmbedBuilder.Build())));
                           }
                           else
                           {
                              IReadOnlyList<DiscordInvite> discordChannelInvites = await discordVoiceState.Channel.GetInvitesAsync();
                              discordChannelInvite = discordChannelInvites.FirstOrDefault(x => x.Inviter.Id == Bot.DiscordClient.CurrentUser.Id && x.Channel.Id == discordVoiceState.Channel.Id);

                              discordChannelInvite ??= await discordVoiceState.Channel.CreateInviteAsync();

                              discordEmbedBuilder.WithDescription(description);
                              discordComponents[0] = new DiscordLinkButtonComponent(discordChannelInvite.Url, "Join channel!", false, discordComponentEmojisJoinChannel);
                              await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(content).WithEmbed(discordEmbedBuilder.Build()));
                           }

                           lastDiscordMember = discordMemberItem;
                           CWLogger.Write("\n\n" + description, "WhereIs", ConsoleColor.Magenta);
                           await Task.Delay(2000);
                        }
                        catch (Exception ex)
                        {
                           CWLogger.Write(ex.Message, "Exception", ConsoleColor.Red);
                        }
                     }

                     getMessagesOncePerGuild = false;
                     discordMessagesList?.Clear();
                     discordMemberConnectedList.Clear();
                     discordMemberConnectedListSorted.Clear();
                  }

                  discordThreads = mainGuild.Threads.Values.ToList();
                  foreach (DiscordThreadChannel discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                  {
                     IReadOnlyList<DiscordMessage> messages = discordThreadItem.GetMessagesAsync().Result;

                     foreach (DiscordMessage messageItem in messages)
                     {
                        string mentionedChannel = "";
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
                              discordChannel = Bot.DiscordClient.GetChannelAsync(Convert.ToUInt64(mentionedChannel)).Result;
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
                     foreach (DiscordThreadChannel discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
                     {
                        await discordThreadItem.DeleteAsync();
                     }

                     IReadOnlyList<DiscordMessage> messages = discordChannelOtherPlaces.GetMessagesAsync().Result;

                     foreach (DiscordMessage messageItem in messages.Where(x => x.Content == "wh3r315"))
                     {
                        await messageItem.DeleteAsync();
                     }
                  }

                  await Task.Delay(1000);
               }
               catch (Exception ex)
               {
                  CWLogger.Write(ex.Message, "Exception", ConsoleColor.Red);
               }
            }
         });
      }
   }
}
