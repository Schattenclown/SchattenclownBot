using System;
using System.Threading.Tasks;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.ApplicationCommands.Standalone
{
    internal class ResetAC : ApplicationCommandsModule
    {
        [SlashCommand("Reset", "Reset the bot!")]
        public async Task ResetCommand(InteractionContext interactionContext)
        {
            if (interactionContext.User.Id != 444152594898878474)
            {
                await interactionContext.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You are not allowed to use this command!").AsEphemeral());
                return;
            }

            await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder().WithColor(DiscordColor.Yellow).WithDescription("SchattenclownBot rebooting!")));
            new CustomLogger().Information($"Reset command called by {interactionContext.User.Username}", ConsoleColor.DarkRed);
            new Reset().RestartProgram();
        }
    }
}
