using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord.Events
{
    /// <summary>
    /// The slash events.
    /// </summary>
    internal class ApplicationCommandsEvents
    {
        public static async Task Client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            await Task.FromResult(true);
        }

        public static Task Discord_ApplicationCommandUpdated(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command updated: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }

        public static Task Discord_ApplicationCommandDeleted(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command deleted: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }

        public static Task Discord_ApplicationCommandCreated(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command created: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }

        public static Task Ac_SlashCommandExecuted(ApplicationCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            Console.WriteLine($"Slash/Info: {e.Context.CommandName}");
            return Task.CompletedTask;
        }

        public static Task Ac_SlashCommandErrored(ApplicationCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            Console.WriteLine($"Slash/Error: {e.Exception.Message} | CN: {e.Context.CommandName} | IID: {e.Context.InteractionId}");
            return Task.CompletedTask;
        }

        public static Task Ac_ContextMenuExecuted(ApplicationCommandsExtension sender, ContextMenuExecutedEventArgs e)
        {
            Console.WriteLine($"Slash/Info: {e.Context.CommandName}");
            return Task.CompletedTask;
        }

        public static Task Ac_ContextMenuErrored(ApplicationCommandsExtension sender, ContextMenuErrorEventArgs e)
        {
            Console.WriteLine($"Slash/Error: {e.Exception.Message} | CN: {e.Context.CommandName} | IID: {e.Context.InteractionId}");
            return Task.CompletedTask;
        }

        public static Task Client_ApplicationCommandPermissionsUpdated(DiscordClient sender, ApplicationCommandPermissionsUpdateEventArgs e)
        {
            Console.WriteLine($"Application command permission '{e.Command.Name}' in guild '{e.Guild.Name}' got updated. New permissions:");
            foreach (var perm in e.Permissions)
            {
                Console.WriteLine($"Permission with type {perm.Type} got updated for {perm.Id}. Command {e.Command.Name} is {(perm.Permission ? "allowed" : "denied")}.");
            }
            return Task.CompletedTask;
        }
    }
}
