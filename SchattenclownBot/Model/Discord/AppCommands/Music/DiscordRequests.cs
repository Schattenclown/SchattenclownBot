using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Model.Discord.AppCommands.Music.Objects;
using SchattenclownBot.Model.Discord.Main;

namespace SchattenclownBot.Model.Discord.AppCommands.Music;

internal class DiscordRequests : ApplicationCommandsModule
{
   [SlashCommand("Play" + Bot.isDevBot, "Play Spotify or YouTube links!")]
   internal async Task PlayCommand(InteractionContext interactionContext, [Option("Link", "Link!")] string webLink)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
      await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Working on it.")));

      await Main.AddTracksToQueueAsyncTask(interactionContext.Member.Id, webLink, false);
   }

   [SlashCommand("Stop" + Bot.isDevBot, "Stop the music!")]
   internal async Task StopCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

      if (interactionContext.Member.VoiceState == null)
      {
         await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithDescription("You must be connected.")));
         return;
      }

      await Main.StopMusicTask(new GMC(interactionContext.Guild, interactionContext.Member, interactionContext.Channel), true);
   }

   [SlashCommand("Shuffle" + Bot.isDevBot, "Randomize the queue!")]
   internal async Task ShuffleCommand(InteractionContext interactionContext)
   {
      await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
      await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("Shuffle requested.")));

      await Main.ShuffleQueueTracksAsyncTask(new GMC(interactionContext.Guild, interactionContext.Member, interactionContext.Channel));
   }
}