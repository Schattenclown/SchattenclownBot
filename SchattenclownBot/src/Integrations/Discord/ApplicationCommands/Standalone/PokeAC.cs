﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Extensions;
using SchattenclownBot.Integrations.Discord.Main;

// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Integrations.Discord.ApplicationCommands.Standalone
{
    public class PokeAC : ApplicationCommandsModule
    {
        [SlashCommand("DaddysPoke", "Harder daddy!")]
        public async Task DaddysPokeAsync(InteractionContext interactionContext, [Option("discordUser", "@...")] DiscordUser discordUser)
        {
            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            DiscordMember discordMember = await interactionContext.Guild.GetMemberAsync(discordUser.Id);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode
            if (discordMember.VoiceState == null)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error: Not connected"));
                return;
            }
            // ReSharper restore HeuristicUnreachableCode

            try
            {
                DiscordChannel currentDiscordChannel = discordMember.VoiceState.Channel;
                IReadOnlyList<DiscordChannel> discordChannels = await interactionContext.Guild.GetChannelsAsync();
                IEnumerable<DiscordChannel> discordVoiceChannels = discordChannels.Where(x => x.Type == ChannelType.Voice).Where(x => x.Id != currentDiscordChannel.Id && !x.Users.Any());
                foreach (DiscordChannel discordChannel in discordVoiceChannels)
                {
                    try
                    {
                        await discordMember.PlaceInAsync(discordChannel);
                        await Task.Delay(1000);
                    }
                    catch (Exception)
                    {
                        await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Something went wrong!"));
                    }
                }

                await discordMember.PlaceInAsync(currentDiscordChannel);
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done!"));
            }
            catch (Exception)
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Something went wrong!"));
            }
        }

        /// <summary>
        ///     PokeAC an User per command.
        /// </summary>
        /// <param name="interactionContext">The interactionContext</param>
        /// <param name="discordUser">the discordUser</param>
        /// <returns></returns>
        [SlashCommand("Poke", "Poke user!")]
        public async Task Ww(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
        {
            InteractivityExtension interactivityExtension = interactionContext.Client.GetInteractivity();
            DiscordStringSelectComponentOption[] discordSelectComponentOptionList = new DiscordStringSelectComponentOption[2];
            discordSelectComponentOptionList[0] = new DiscordStringSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
            discordSelectComponentOptionList[1] = new DiscordStringSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

            DiscordStringSelectComponent discordSelectComponent = new("Select a method!", discordSelectComponentOptionList, "force");

            await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"PokeAC discordUser <@{discordUser.Id}>!"));
            DiscordMessage discordMessage = await interactionContext.GetOriginalResponseAsync();
            InteractivityResult<ComponentInteractionCreateEventArgs> interactivityResult = await interactivityExtension.WaitForSelectAsync(discordMessage, "force", ComponentType.StringSelect, TimeSpan.FromMinutes(1));
            if (!interactivityResult.TimedOut)
            {
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
                string firstResult = interactivityResult.Result.Values.First();
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(discordSelectComponent.Disable()));
                DiscordMember discordTargetMember = await interactionContext.Guild.GetMemberAsync(discordUser.Id);
                await PokeTask(interactivityResult.Result.Interaction, interactionContext.Member, discordTargetMember, false, firstResult == "light" ? 2 : 4, firstResult == "hard");
            }
            else
            {
                await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out!"));
            }
        }

        /// <summary>
        ///     PokeAC an User per contextmenu.
        /// </summary>
        /// <param name="contextMenuContext">The contextMenuContext</param>
        /// <returns></returns>
        [ContextMenu(ApplicationCommandType.User, "Poke user!")]
        public async Task PokeAsync(ContextMenuContext contextMenuContext)
        {
            InteractivityExtension interactivityExtension = contextMenuContext.Client.GetInteractivity();
            DiscordStringSelectComponentOption[] discordSelectComponentOptionList = new DiscordStringSelectComponentOption[2];
            discordSelectComponentOptionList[0] = new DiscordStringSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
            discordSelectComponentOptionList[1] = new DiscordStringSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

            DiscordStringSelectComponent discordStringSelectComponent = new("Select a method!", discordSelectComponentOptionList, "force");

            await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordStringSelectComponent).WithContent($"PokeAC discordUser <@{contextMenuContext.TargetMember.Id}>!"));
            DiscordMessage discordMessage = await contextMenuContext.GetOriginalResponseAsync();
            InteractivityResult<ComponentInteractionCreateEventArgs> interactivityResult = await interactivityExtension.WaitForSelectAsync(discordMessage, "force", ComponentType.StringSelect, TimeSpan.FromMinutes(1));
            if (!interactivityResult.TimedOut)
            {
                await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
                string firstResult = interactivityResult.Result.Values.First();
                await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(discordStringSelectComponent.Disable()));
                await PokeTask(interactivityResult.Result.Interaction, contextMenuContext.Member, contextMenuContext.TargetMember, false, firstResult == "light" ? 2 : 4, firstResult == "hard");
            }
            else
            {
                await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out!"));
            }
        }

        /// <summary>
        ///     PokeAC an User per contextmenu.
        /// </summary>
        /// <param name="contextMenuContext">The contextMenuContext</param>
        /// <returns></returns>
        [ContextMenu(ApplicationCommandType.User, "Poke user Instant!")]
        public async Task InstantPokeAsync(ContextMenuContext contextMenuContext)
        {
            await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent($"PokeAC discordUser <@{contextMenuContext.TargetMember.Id}>!"));

            await PokeTask(contextMenuContext.Interaction, contextMenuContext.Member, contextMenuContext.TargetMember, false, 4, true);
        }

        /// <summary>
        ///     The PokeAC function.
        /// </summary>
        /// <param name="discordTargetMember"></param>
        /// <param name="deleteResponseAsync">If the response should be Deleted after the poke action.</param>
        /// <param name="pokeAmount">The amount the discordUser gets poked.</param>
        /// <param name="force">Light or hard slap.</param>
        /// <param name="discordInteraction"></param>
        /// <param name="discordMember"></param>
        /// <returns></returns>
        public async Task PokeTask(DiscordInteraction discordInteraction, DiscordMember discordMember, DiscordMember discordTargetMember, bool deleteResponseAsync, int pokeAmount, bool force)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            // ReSharper disable HeuristicUnreachableCode
            DiscordEmbedBuilder discordEmbedBuilder = new()
            {
                        Title = $"Poke {discordTargetMember.DisplayName}"
            };

            discordEmbedBuilder.WithFooter($"Requested by {discordMember.DisplayName}", discordMember.AvatarUrl);

            await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));

            bool rightToMove = false;

            List<DiscordRole> discordRoleList = discordMember.Roles.ToList();

            foreach (DiscordRole dummy in discordRoleList.Where(discordRoleItem => discordRoleItem.Permissions.HasPermission(Permissions.MoveMembers)))
            {
                rightToMove = true;
            }

            bool desktopHasValue = false;
            bool webHasValue = false;
            bool mobileHasValue = false;
            bool presenceWasNull = false;

            if (discordTargetMember.Presence != null)
            {
                desktopHasValue = discordTargetMember.Presence.ClientStatus.Desktop.HasValue;
                webHasValue = discordTargetMember.Presence.ClientStatus.Web.HasValue;
                mobileHasValue = discordTargetMember.Presence.ClientStatus.Mobile.HasValue;
            }
            else
            {
                presenceWasNull = true;
            }


            if (discordTargetMember.VoiceState != null && rightToMove && (force || presenceWasNull || ((desktopHasValue || webHasValue) && !mobileHasValue)))
            {
                DiscordChannel currentChannel = default;
                DiscordChannel tempCategory = default;
                DiscordChannel tempChannel2 = default;
                DiscordChannel tempChannel1 = default;

                try
                {
                    DiscordEmoji discordEmojis = DiscordEmoji.FromName(DiscordBot.DiscordClient, ":no_entry_sign:");

                    tempCategory = discordInteraction.Guild.CreateChannelCategoryAsync("%Temp%").Result;
                    tempChannel1 = discordInteraction.Guild.CreateVoiceChannelAsync(discordEmojis, tempCategory).Result;
                    tempChannel2 = discordInteraction.Guild.CreateVoiceChannelAsync(discordEmojis, tempCategory).Result;
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while creating the channels!";
                    await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    currentChannel = discordTargetMember.VoiceState.Channel;

                    for (int i = 0; i < pokeAmount; i++)
                    {
                        if (tempChannel1 != null)
                        {
                            await discordTargetMember.PlaceInAsync(tempChannel1);
                        }

                        await Task.Delay(250);
                        if (tempChannel2 != null)
                        {
                            await discordTargetMember.PlaceInAsync(tempChannel2);
                        }

                        await Task.Delay(250);
                    }

                    if (tempChannel1 != null)
                    {
                        await discordTargetMember.PlaceInAsync(tempChannel1);
                    }

                    await Task.Delay(250);
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error! User left?";
                    await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    if (currentChannel != null)
                    {
                        await discordTargetMember.PlaceInAsync(currentChannel);
                    }
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error! User left?";
                    await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    if (tempChannel2 != null)
                    {
                        await tempChannel2.DeleteAsync();
                    }
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    if (tempChannel1 != null)
                    {
                        await tempChannel1.DeleteAsync();
                    }
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }

                try
                {
                    if (tempCategory != null)
                    {
                        await tempCategory.DeleteAsync();
                    }
                }
                catch
                {
                    discordEmbedBuilder.Description = "Error while deleting the channels!";
                    await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                }
            }
            else if (discordTargetMember.VoiceState == null)
            {
                discordEmbedBuilder.Description = "User is not connected!";
                await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
            else if (!rightToMove)
            {
                discordEmbedBuilder.Description = "Your not allowed to use that!";
                await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
            else if (mobileHasValue)
            {
                string description = "Their phone will explode STOP!\n";

                DiscordEmoji discordEmojisWhiteCheckMark = DiscordEmoji.FromName(DiscordBot.DiscordClient, ":white_check_mark:");
                DiscordEmoji discordEmojisCheckX = DiscordEmoji.FromName(DiscordBot.DiscordClient, ":x:");

                if (discordTargetMember.Presence.ClientStatus.Desktop.HasValue)
                {
                    description += discordEmojisWhiteCheckMark + " Desktop" + "\n";
                }
                else
                {
                    description += discordEmojisCheckX + " Desktop" + "\n";
                }

                if (discordTargetMember.Presence.ClientStatus.Web.HasValue)
                {
                    description += discordEmojisWhiteCheckMark + " Web" + "\n";
                }
                else
                {
                    description += discordEmojisCheckX + " Web" + "\n";
                }

                if (discordTargetMember.Presence.ClientStatus.Mobile.HasValue)
                {
                    description += discordEmojisWhiteCheckMark + " Mobile";
                }
                else
                {
                    description += discordEmojisCheckX + " Mobile";
                }

                discordEmbedBuilder.Description = description;

                await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            // ReSharper restore HeuristicUnreachableCode

            if (deleteResponseAsync)
            {
                for (int i = 3; i > 0; i--)
                {
                    discordEmbedBuilder.AddField(new DiscordEmbedField("This message will be deleted in", $"{i} Seconds"));
                    await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
                    await Task.Delay(1000);
                    discordEmbedBuilder.RemoveFieldAt(0);
                }

                await discordInteraction.DeleteOriginalResponseAsync();
            }
        }
    }
}