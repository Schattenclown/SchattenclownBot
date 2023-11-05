﻿using System;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.Main
{
    public static class Logging
    {
        public static Task Client_Ready(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
        {
            CustomLogger.Information($"Starting {discordClient.CurrentUser.Username}", ConsoleColor.Green);
            CustomLogger.Information("DiscordClient ready!", ConsoleColor.Green);
            CustomLogger.Information($"Shard {discordClient.ShardId}", ConsoleColor.Green);
            CustomLogger.Information("Loading Commands...", ConsoleColor.Green);

            discordClient.UpdateStatusAsync(new DiscordActivity("/help", ActivityType.Watching), UserStatus.Online);
            CustomLogger.Information("SchattenclownBot ready!", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public static Task Client_Resumed(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
        {
            CustomLogger.Information("MusicController resumed!", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public static Task Discord_ApplicationCommandUpdated(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            CustomLogger.Information($"Shard {discordClient.ShardId} sent application command updated: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public static Task Discord_ApplicationCommandDeleted(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            CustomLogger.Information($"Shard {discordClient.ShardId} sent application command deleted: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public static Task Discord_ApplicationCommandCreated(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            CustomLogger.Information($"Shard {discordClient.ShardId} sent application command created: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public static Task Slash_SlashCommandExecuted(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandExecutedEventArgs slashCommandExecutedEventArgs)
        {
            CustomLogger.Information($"Slash/Info: {slashCommandExecutedEventArgs.Context.CommandName}", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public static Task Slash_SlashCommandError(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
        {
            CustomLogger.Information($"Slash/Error: {slashCommandErrorEventArgs.Exception.Message} | CN: {slashCommandErrorEventArgs.Context.CommandName} | IID: {slashCommandErrorEventArgs.Context.InteractionId}", ConsoleColor.Red);
            return Task.CompletedTask;
        }

        public static Task Client_SocketOpened(DiscordClient discordClient, SocketEventArgs socketEventArgs)
        {
            CustomLogger.Information("Socket opened", ConsoleColor.Green);
            return Task.CompletedTask;
        }

        public static Task Client_SocketError(DiscordClient discordClient, SocketErrorEventArgs socketErrorEventArgs)
        {
            CustomLogger.Information("Socket has an error! " + socketErrorEventArgs.Exception.Message, ConsoleColor.Red);
            return Task.CompletedTask;
        }

        public static Task Client_SocketClosed(DiscordClient discordClient, SocketCloseEventArgs socketCloseEventArgs)
        {
            CustomLogger.Information("Socket closed: " + socketCloseEventArgs.CloseMessage, ConsoleColor.Red);
            return Task.CompletedTask;
        }

        public static Task Client_Heartbeat(DiscordClient discordClient, HeartbeatEventArgs heartbeatEventArgs)
        {
            //CustomLogger.Green("Received Heartbeat:" + heartbeatEventArgs.Ping);
            return Task.CompletedTask;
        }
    }
}