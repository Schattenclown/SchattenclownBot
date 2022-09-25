using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Extensions;
using SchattenclownBot.Model.Discord.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands
{
   internal class Poke : ApplicationCommandsModule
   {
      [SlashCommand("DaddysPoke" + Bot.isDevBot, "Harder daddy!")]
      public static async Task DaddysPokeAsync(InteractionContext interactionContext, [Option("discordUser", "@...")] DiscordUser discordUser)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
         DiscordMember discordMember = await interactionContext.Guild.GetMemberAsync(discordUser.Id);
         if (discordMember.VoiceState == null)
         {
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Error: Not connected"));
            return;
         }
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
      ///     Poke an User per command.
      /// </summary>
      /// <param name="interactionContext">The interactionContext</param>
      /// <param name="discordUser">the discordUser</param>
      /// <returns></returns>
      [SlashCommand("Poke" + Bot.isDevBot, "Poke discordUser!")]
      public static async Task PokeAsync(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
      {
         InteractivityExtension interactivityExtension = interactionContext.Client.GetInteractivity();
         DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[2];
         discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
         discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

         DiscordSelectComponent discordSelectComponent = new(placeholder: "Select a method!", discordSelectComponentOptionList, "force");

         await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Poke discordUser <@{discordUser.Id}>!"));
         DiscordMessage discordMessage = await interactionContext.GetOriginalResponseAsync();
         InteractivityResult<ComponentInteractionCreateEventArgs> interactivityResult = await interactivityExtension.WaitForSelectAsync(discordMessage, "force", TimeSpan.FromMinutes(1));
         if (!interactivityResult.TimedOut)
         {
            await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            string firstResult = interactivityResult.Result.Values.First();
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(discordSelectComponent.Disable()));
            DiscordMember discordTargetMember = await interactionContext.Guild.GetMemberAsync(discordUser.Id);
            await PokeTask(interactivityResult.Result.Interaction, interactionContext.Member, discordTargetMember, false, firstResult == "light" ? 2 : 4, firstResult == "hard");
         }
         else
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out!"));
      }

      /// <summary>
      ///     Poke an User per contextmenu.
      /// </summary>
      /// <param name="contextMenuContext">The contextMenuContext</param>
      /// <returns></returns>
      [ContextMenu(ApplicationCommandType.User, "Poke discordUser!")]
      public static async Task PokeAsync(ContextMenuContext contextMenuContext)
      {
         InteractivityExtension interactivityExtension = contextMenuContext.Client.GetInteractivity();
         DiscordSelectComponentOption[] discordSelectComponentOptionList = new DiscordSelectComponentOption[2];
         discordSelectComponentOptionList[0] = new DiscordSelectComponentOption("Light", "light", emoji: new DiscordComponentEmoji("👉"));
         discordSelectComponentOptionList[1] = new DiscordSelectComponentOption("Hard", "hard", emoji: new DiscordComponentEmoji("🤜"));

         DiscordSelectComponent discordSelectComponent = new(placeholder: "Select a method!", discordSelectComponentOptionList, "force");

         await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddComponents(discordSelectComponent).WithContent($"Poke discordUser <@{contextMenuContext.TargetMember.Id}>!"));
         DiscordMessage discordMessage = await contextMenuContext.GetOriginalResponseAsync();
         InteractivityResult<ComponentInteractionCreateEventArgs> interactivityResult = await interactivityExtension.WaitForSelectAsync(discordMessage, "force", TimeSpan.FromMinutes(1));
         if (!interactivityResult.TimedOut)
         {
            await interactivityResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
            string firstResult = interactivityResult.Result.Values.First();
            await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(discordSelectComponent.Disable()));
            await PokeTask(interactivityResult.Result.Interaction, contextMenuContext.Member, contextMenuContext.TargetMember, false, firstResult == "light" ? 2 : 4, firstResult == "hard");
         }
         else
            await contextMenuContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timed out!"));
      }

      /// <summary>
      ///     The Poke function.
      /// </summary>
      /// <param name="discordTargetMember"></param>
      /// <param name="deleteResponseAsync">If the response should be Deleted after the poke action.</param>
      /// <param name="pokeAmount">The amount the discordUser gets poked.</param>
      /// <param name="force">Light or hard slap.</param>
      /// <param name="discordInteraction"></param>
      /// <param name="discordMember"></param>
      /// <returns></returns>
      public static async Task PokeTask(DiscordInteraction discordInteraction, DiscordMember discordMember, DiscordMember discordTargetMember, bool deleteResponseAsync, int pokeAmount, bool force)
      {
         DiscordEmbedBuilder discordEmbedBuilder = new()
         {
            Title = $"Poke {discordTargetMember.DisplayName}"
         };

         discordEmbedBuilder.WithFooter($"Requested by {discordMember.DisplayName}", discordMember.AvatarUrl);

         await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));

         bool rightToMove = false;

         List<DiscordRole> discordRoleList = discordMember.Roles.ToList();

         foreach (DiscordRole dummy in discordRoleList.Where(discordRoleItem => discordRoleItem.Permissions.HasPermission(Permissions.MoveMembers)))
            rightToMove = true;

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
               DiscordEmoji discordEmojis = DiscordEmoji.FromName(Bot.DiscordClient, ":no_entry_sign:");

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
                  await discordTargetMember.PlaceInAsync(tempChannel1);
                  await Task.Delay(250);
                  await discordTargetMember.PlaceInAsync(tempChannel2);
                  await Task.Delay(250);
               }

               await discordTargetMember.PlaceInAsync(tempChannel1);
               await Task.Delay(250);
            }
            catch
            {
               discordEmbedBuilder.Description = "Error! User left?";
               await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
               await discordTargetMember.PlaceInAsync(currentChannel);
            }
            catch
            {
               discordEmbedBuilder.Description = "Error! User left?";
               await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
               await tempChannel2.DeleteAsync();
            }
            catch
            {
               discordEmbedBuilder.Description = "Error while deleting the channels!";
               await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
               await tempChannel1.DeleteAsync();
            }
            catch
            {
               discordEmbedBuilder.Description = "Error while deleting the channels!";
               await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
            }

            try
            {
               await tempCategory.DeleteAsync();
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

            DiscordEmoji discordEmojisWhiteCheckMark = DiscordEmoji.FromName(Bot.DiscordClient, ":white_check_mark:");
            DiscordEmoji discordEmojisCheckX = DiscordEmoji.FromName(Bot.DiscordClient, ":x:");

            if (discordTargetMember.Presence.ClientStatus.Desktop.HasValue)
               description += discordEmojisWhiteCheckMark + " Desktop" + "\n";
            else
               description += discordEmojisCheckX + " Desktop" + "\n";

            if (discordTargetMember.Presence.ClientStatus.Web.HasValue)
               description += discordEmojisWhiteCheckMark + " Web" + "\n";
            else
               description += discordEmojisCheckX + " Web" + "\n";

            if (discordTargetMember.Presence.ClientStatus.Mobile.HasValue)
               description += discordEmojisWhiteCheckMark + " Mobile";
            else
               description += discordEmojisCheckX + " Mobile";

            discordEmbedBuilder.Description = description;

            await discordInteraction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
         }

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
