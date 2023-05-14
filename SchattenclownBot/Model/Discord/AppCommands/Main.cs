using System;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Model.Discord.Main;

// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands
{
   /// <summary>
   ///    The AppCommands.
   /// </summary>
   internal class Main : ApplicationCommandsModule
   {
      /// <summary>
      ///    HandlerReader the Avatar and Banner of an User.
      /// </summary>
      /// <param name="contextMenuContext">The contextMenuContext.</param>
      /// <returns></returns>
      [ContextMenu(ApplicationCommandType.User, "Get avatar & banner!")]
      public static async Task GetUserBannerAsync(ContextMenuContext contextMenuContext)
      {
         DiscordUser user = await contextMenuContext.Client.GetUserAsync(contextMenuContext.TargetUser.Id);

         DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder
                  {
                           Title = $"Avatar & Banner of {user.Username}",
                           ImageUrl = user.BannerHash != null ? user.BannerUrl : null
                  }.WithThumbnail(user.AvatarUrl)
                  .WithColor(user.BannerColor ?? DiscordColor.Purple)
                  .WithFooter($"Requested by {contextMenuContext.Member.DisplayName}", contextMenuContext.Member.AvatarUrl)
                  .WithAuthor($"{user.Username}", user.AvatarUrl, user.AvatarUrl);
         await contextMenuContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().AddEmbed(discordEmbedBuilder.Build()));
      }

      /// <summary>
      ///    Creates an Invite link.
      /// </summary>
      /// <param name="interactionContext">The interactionContext.</param>
      /// <returns></returns>
      [SlashCommand(Bot.isDevBot + "Invite", "Invite $chattenclown")]
      public static async Task InviteAsync(InteractionContext interactionContext)
      {
         await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

         Uri botInvite = interactionContext.Client.GetInAppOAuth(Permissions.Administrator);

         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(botInvite.AbsoluteUri));
      }
   }
}