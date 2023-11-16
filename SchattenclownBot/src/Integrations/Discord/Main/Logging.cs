using System;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.Main
{
    public class Logging
    {
        public Task Client_Ready(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
        {
            new CustomLogger().Information($"Starting {discordClient.CurrentUser.Username}", ConsoleColor.Green);
            new CustomLogger().Information("DiscordClient ready!", ConsoleColor.Green);
            new CustomLogger().Information($"Shard {discordClient.ShardId}", ConsoleColor.Green);
            new CustomLogger().Information("Loading Commands...", ConsoleColor.Green);

            discordClient.UpdateStatusAsync(new DiscordActivity("/help", ActivityType.Watching), UserStatus.Online);
            new CustomLogger().Information("SchattenclownBot ready!", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public Task Client_Resumed(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
        {
            new CustomLogger().Information("SchattenclownBot resumed!", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public Task Discord_ApplicationCommandUpdated(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            new CustomLogger().Information($"Shard {discordClient.ShardId} sent application command updated: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public Task Discord_ApplicationCommandDeleted(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            new CustomLogger().Information($"Shard {discordClient.ShardId} sent application command deleted: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public Task Discord_ApplicationCommandCreated(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            new CustomLogger().Information($"Shard {discordClient.ShardId} sent application command created: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public Task Slash_SlashCommandExecuted(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandExecutedEventArgs slashCommandExecutedEventArgs)
        {
            new CustomLogger().Information($"Slash/Info: {slashCommandExecutedEventArgs.Context.CommandName}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public Task Slash_SlashCommandError(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
        {
            new CustomLogger().Information($"Slash/Error: {slashCommandErrorEventArgs.Exception.Message} | CN: {slashCommandErrorEventArgs.Context.CommandName} | IID: {slashCommandErrorEventArgs.Context.InteractionId}", ConsoleColor.Red);
            return Task.CompletedTask;
        }

        public Task Client_SocketOpened(DiscordClient discordClient, SocketEventArgs socketEventArgs)
        {
            new CustomLogger().Information("Socket opened", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public Task Client_SocketError(DiscordClient discordClient, SocketErrorEventArgs socketErrorEventArgs)
        {
            new CustomLogger().Information("Socket has an error! " + socketErrorEventArgs.Exception.Message, ConsoleColor.Red);
            return Task.CompletedTask;
        }

        public Task Client_SocketClosed(DiscordClient discordClient, SocketCloseEventArgs socketCloseEventArgs)
        {
            new CustomLogger().Information("Socket closed: " + socketCloseEventArgs.CloseMessage, ConsoleColor.Red);
            return Task.CompletedTask;
        }

        public Task Client_Heartbeat(DiscordClient discordClient, HeartbeatEventArgs heartbeatEventArgs)
        {
            //CustomLogger.Green("Received Heartbeat:" + heartbeatEventArgs.Ping);
            return Task.CompletedTask;
        }
    }
}