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
            ConsoleLogger.WriteLine($"Starting {discordClient.CurrentUser.Username}");
            ConsoleLogger.WriteLine("DiscordClient ready!");
            ConsoleLogger.WriteLine($"Shard {discordClient.ShardId}");
            ConsoleLogger.WriteLine("Loading Commands...");

            discordClient.UpdateStatusAsync(new DiscordActivity("/help", ActivityType.Watching), UserStatus.Online);
            ConsoleLogger.WriteLine("SchattenclownBot ready!");
            return Task.CompletedTask;
        }

        public static Task Client_Resumed(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
        {
            ConsoleLogger.WriteLine("MusicController resumed!");
            return Task.CompletedTask;
        }

        public static Task Discord_ApplicationCommandUpdated(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            ConsoleLogger.WriteLine($"Shard {discordClient.ShardId} sent application command updated: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}");
            return Task.CompletedTask;
        }

        public static Task Discord_ApplicationCommandDeleted(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            ConsoleLogger.WriteLine($"Shard {discordClient.ShardId} sent application command deleted: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}");
            return Task.CompletedTask;
        }

        public static Task Discord_ApplicationCommandCreated(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            ConsoleLogger.WriteLine($"Shard {discordClient.ShardId} sent application command created: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}");
            return Task.CompletedTask;
        }

        public static Task Slash_SlashCommandExecuted(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandExecutedEventArgs slashCommandExecutedEventArgs)
        {
            ConsoleLogger.WriteLine($"Slash/Info: {slashCommandExecutedEventArgs.Context.CommandName}");
            return Task.CompletedTask;
        }

        public static Task Slash_SlashCommandError(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
        {
            ConsoleLogger.WriteLine($"Slash/Error: {slashCommandErrorEventArgs.Exception.Message} | CN: {slashCommandErrorEventArgs.Context.CommandName} | IID: {slashCommandErrorEventArgs.Context.InteractionId}", true);
            return Task.CompletedTask;
        }

        public static Task Client_SocketOpened(DiscordClient discordClient, SocketEventArgs socketEventArgs)
        {
            ConsoleLogger.WriteLine("Socket opened");
            return Task.CompletedTask;
        }

        public static Task Client_SocketError(DiscordClient discordClient, SocketErrorEventArgs socketErrorEventArgs)
        {
            ConsoleLogger.WriteLine("Socket has an error! " + socketErrorEventArgs.Exception.Message, true);
            return Task.CompletedTask;
        }

        public static Task Client_SocketClosed(DiscordClient discordClient, SocketCloseEventArgs socketCloseEventArgs)
        {
            ConsoleLogger.WriteLine("Socket closed: " + socketCloseEventArgs.CloseMessage);
            return Task.CompletedTask;
        }

        public static Task Client_Heartbeat(DiscordClient discordClient, HeartbeatEventArgs heartbeatEventArgs)
        {
            //ConsoleLogger.WriteLine("Received Heartbeat:" + heartbeatEventArgs.Ping);
            return Task.CompletedTask;
        }
    }
}