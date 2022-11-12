// Copyright (c) Schattenclown

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DisCatSharp.Entities;
using DisCatSharp.Enums;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot.Model.AsyncFunction;

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

			var mainGuild = Bot.DiscordClient.GetGuildAsync(928930967140331590).Result;
			var discordChannelOtherPlaces = mainGuild.GetChannel(987123289619071026);
			guildList.Remove(Bot.DiscordClient.GetGuildAsync(858089281214087179).Result);

			while (true)
			{
				try
				{
					while (DateTime.Now.Second != executeSecond)
					{
						await Task.Delay(1000);
					}

					var voiceStateAny = false;
					List<DiscordThreadChannel> discordThreads;
					var getMessagesOncePerGuild = false;
					List<DiscordMessage> discordMessagesList = new();
					List<DiscordMember> discordMemberConnectedList = new();
					DiscordMember lastDiscordMember = default;

					foreach (var guildItem in guildList)
					{
						var discordMemberList = guildItem.Members.Values.ToList();

						discordMemberConnectedList.AddRange(discordMemberList.Where(discordMemberItem => discordMemberItem.VoiceState != null));

						var discordMemberConnectedListSorted = discordMemberConnectedList.OrderBy(discordMemberItem => discordMemberItem.VoiceState.Channel.Id).ToList();

						foreach (var discordMemberItem in discordMemberConnectedListSorted)
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

								var discordMembersInChannel = discordVoiceState.Channel.Users.ToList();
								var discordMembersInChannelSorted = discordMembersInChannel.OrderBy(x => x.VoiceState.IsSelfStream).ToList();
								discordMembersInChannelSorted.Reverse();

								var description = "";
								foreach (var discordMemberInChannelItem in discordMembersInChannelSorted)
								{
									var descriptionLineBuilder = "";
									var counter = 5;
									var username = SpecialChars.RemoveSpecialCharacters(discordMemberInChannelItem.DisplayName);
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
								discordThreadsChannel ??= await discordChannelOtherPlaces.CreateThreadAsync("wh3r315", ThreadAutoArchiveDuration.OneDay);

								DiscordEmbedBuilder discordEmbedBuilder = new()
								{
									Color = new DiscordColor(17, 17, 17)
								};
								discordEmbedBuilder.WithFooter(discordVoiceState.Guild.Name + " | " + discordVoiceState.Channel.Name, discordVoiceState.Guild.IconUrl);
								discordEmbedBuilder.WithTimestamp(DateTime.Now);

								if (!getMessagesOncePerGuild)
								{
									var messages = await discordThreadsChannel.GetMessagesAsync();
									discordMessagesList.AddRange(messages);
									getMessagesOncePerGuild = true;
								}

								DiscordMessage discordMessage = default;
								var content = $"<#{discordVoiceState.Channel.Id}>";
								if (discordMessagesList != null)
									foreach (var messageItem in discordMessagesList.Where(x => x.Content.Contains(content)))
									{
										discordMessage = messageItem;
										break;
									}

								DiscordComponentEmoji discordComponentEmojisJoinChannel = new("📞");
								DiscordComponentEmoji discordComponentEmojisJoinServer = new("🛡");
								var discordComponents = new DiscordComponent[2];

								var defaultDiscordChannel = discordVoiceState.Guild.GetDefaultChannel();
								var discordServerInvites = await defaultDiscordChannel.GetInvitesAsync();
								var discordServerInvite = discordServerInvites.FirstOrDefault(x => x.Inviter.Id == Bot.DiscordClient.CurrentUser.Id && x.Channel.Id == defaultDiscordChannel.Id);
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
									var discordChannelInvites = await discordVoiceState.Channel.GetInvitesAsync();
									discordChannelInvite = discordChannelInvites.FirstOrDefault(x => x.Inviter.Id == Bot.DiscordClient.CurrentUser.Id && x.Channel.Id == discordVoiceState.Channel.Id);

									discordChannelInvite ??= await discordVoiceState.Channel.CreateInviteAsync();

									discordEmbedBuilder.WithDescription(description);
									discordComponents[0] = new DiscordLinkButtonComponent(discordChannelInvite.Url, "Join channel!", false, discordComponentEmojisJoinChannel);
									await discordMessage.ModifyAsync(x => x.AddComponents(discordComponents).WithContent(content).WithEmbed(discordEmbedBuilder.Build()));
								}

								lastDiscordMember = discordMemberItem;
								CWLogger.Write("\n\n" + description, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
								await Task.Delay(2000);
							}
							catch (Exception ex)
							{
								CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
							}
						}

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
						foreach (var discordThreadItem in discordThreads.Where(x => x.Name == "wh3r315"))
						{
							await discordThreadItem.DeleteAsync();
						}

						var messages = await discordChannelOtherPlaces.GetMessagesAsync();

						foreach (var messageItem in messages.Where(x => x.Content == "wh3r315"))
						{
							await messageItem.DeleteAsync();
						}
					}


					await Task.Delay(1000);

					CWLogger.Write("Finished", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);

					if (!LastMinuteCheck.WhereIsClownRunAsync)
						LastMinuteCheck.WhereIsClownRunAsync = true;
				}
				catch (Exception ex)
				{
					CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
				}
			}
		});
	}
}
