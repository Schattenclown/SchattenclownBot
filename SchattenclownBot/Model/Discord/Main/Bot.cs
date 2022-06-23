using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.EventHandling;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.EventArgs;
using DisCatSharp.ApplicationCommands.EventArgs;
using SchattenclownBot.Model.AsyncFunction;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot.Model.Discord.Main
{
    public class Bot : IDisposable
    {

#if DEBUG
        public const string Prefix = "!";
#else 
        public const string Prefix = "%";
#endif
        public static readonly ulong DevGuild = 881868642600505354;

        public static CancellationTokenSource ShutdownRequest;
        public static DiscordClient Client;
        public static ApplicationCommandsExtension AppCommands;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "<Pending>")]
        private InteractivityExtension INext;
        private CommandsNextExtension CNext;

        private static string _token = "";
        private static int _virgin = 0;
        public static UserStatus CustomStatus = UserStatus.Streaming;
        public static bool Custom = false;
        public static string CustomState = $"/help";

        /// <summary>
        /// Initializes a new instance of the <see cref="Bot"/> class.
        /// </summary>
        public Bot()
        {
            if (_virgin == 0)
            {
                var connections = Connections.GetConnections();
                _token = connections.DiscordBotKey;
#if DEBUG
                _token = connections.DiscordBotDebug;
#endif
                _virgin = 69;
            }

            ShutdownRequest = new CancellationTokenSource();

#if DEBUG
            const LogLevel logLevel = LogLevel.Debug;
#else
            const LogLevel logLevel = LogLevel.Debug;
#endif
            var cfg = new DiscordConfiguration
            {
                Token = _token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MessageCacheSize = 2048,
                MinimumLogLevel = logLevel,
                ShardCount = 1,
                ShardId = 0,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.GuildPresences,
                MobileStatus = false,
                UseCanary = false,
                AutoRefreshChannelCache = false
            };

            Client = new DiscordClient(cfg);

            AppCommands = Client.UseApplicationCommands(new ApplicationCommandsConfiguration()
            {
                EnableDefaultHelp = true,
                DebugStartup = true,
                ManualOverride = true,
                CheckAllGuilds = true
            });

            CNext = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { Prefix },
                CaseSensitive = true,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
                DefaultHelpChecks = null,
                EnableDefaultHelp = true,
                EnableDms = true
            });

            INext = Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                ButtonBehavior = ButtonPaginationBehavior.Disable
            });

            RegisterEventListener(Client, AppCommands, CNext);
            RegisterCommands(CNext, AppCommands);
        }

        /// <summary>
        /// Disposes the Bot.
        /// </summary>
        public void Dispose()
        {
            Client.Dispose();
            INext = null;
            CNext = null;
            Client = null;
            AppCommands = null;
            Environment.Exit(0);
        }

        /// <summary>
        /// Starts the Bot.
        /// </summary>
        public async Task RunAsync()
        {
            await Client.ConnectAsync();

#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
            BotTimer.BotTimerRunAsync();
            BotAlarmClock.BotAlarmClockRunAsync();
            WhereIsClown.WhereIsClownRunAsync(19);
            UserLevelSystem.LevelSystemRunAsync(29);
            UserLevelSystem.LevelSystemRoleDistributionRunAsync(39);
            SympathySystem.SympathySystemRunAsync(59);
            BirthdayList.GenerateBirthdayList();
#pragma warning restore CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.

            while (!ShutdownRequest.IsCancellationRequested)
            {
                await Task.Delay(2000);
            }
            await Client.UpdateStatusAsync(activity: null, userStatus: UserStatus.Offline, idleSince: null);
            await Client.DisconnectAsync();
            await Task.Delay(2500);
            Dispose();
        }

        #region Register Commands & Events
        /// <summary>
        /// Registers the event listener.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="cnext">The commandsNext extension.</param>
        private void RegisterEventListener(DiscordClient client, ApplicationCommandsExtension appCommands, CommandsNextExtension cnext)
        {

            /* Client Basic Events */
            client.SocketOpened += Client_SocketOpened;
            client.SocketClosed += Client_SocketClosed;
            client.SocketErrored += Client_SocketErrored;
            client.Heartbeated += Client_Heartbeated;
            client.Ready += Client_Ready;
            client.Resumed += Client_Resumed;

            /* Client Events */
            //client.GuildUnavailable += Client_GuildUnavailable;
            //client.GuildAvailable += Client_GuildAvailable;

            /* CommandsNext Error */
            cnext.CommandErrored += CNext_CommandErrored;

            /* Slash Infos */
            client.ApplicationCommandCreated += Discord_ApplicationCommandCreated;
            client.ApplicationCommandDeleted += Discord_ApplicationCommandDeleted;
            client.ApplicationCommandUpdated += Discord_ApplicationCommandUpdated;
            appCommands.SlashCommandErrored += Slash_SlashCommandErrored;
            appCommands.SlashCommandExecuted += Slash_SlashCommandExecuted;

            client.ComponentInteractionCreated += Discord.AppCommands.Main.Discord_ComponentInteractionCreated;
        }

        /// <summary>
        /// Registers the commands.
        /// </summary>
        /// <param name="cnext">The commandsnext extension.</param>
        /// <param name="appCommands">The appcommands extension.</param>
        private void RegisterCommands(CommandsNextExtension cnext, ApplicationCommandsExtension appCommands)
        {
            cnext.RegisterCommands<Commands.Main>(); // Commands.Main = Ordner.Class
#if DEBUG
            appCommands.RegisterGuildCommands<AppCommands.Main>(DevGuild); // use to register on guild
#else
            appCommands.RegisterGlobalCommands<AppCommands.Main>(); // use to register global (can take up to an hour)
#endif
        }

        private static Task Client_Ready(DiscordClient dcl, ReadyEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Starting with Prefix {Prefix} :3");
            Console.WriteLine($"Starting {Client.CurrentUser.Username}");
            Console.WriteLine("Client ready!");
            Console.WriteLine($"Shard {dcl.ShardId}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Loading Commands...");
            Console.ForegroundColor = ConsoleColor.Magenta;
            var commandlist = dcl.GetCommandsNext().RegisteredCommands;
            foreach (var command in commandlist)
            {
                Console.WriteLine($"Command {command.Value.Name} loaded.");
            }
            var activity = new DiscordActivity()
            {
                Name = Bot.Custom ? Bot.CustomState : $"/help",
                ActivityType = ActivityType.Streaming
            };
            dcl.UpdateStatusAsync(activity: activity, userStatus: Bot.Custom ? Bot.CustomStatus : UserStatus.Online, idleSince: null);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bot ready!");
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }

        private static Task Client_Resumed(DiscordClient dcl, ReadyEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Bot resumed!");
            return Task.CompletedTask;
        }

        private static Task Discord_ApplicationCommandUpdated(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command updated: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandDeleted(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command deleted: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandCreated(DiscordClient sender, ApplicationCommandEventArgs e)
        {
            sender.Logger.LogInformation($"Shard {sender.ShardId} sent application command created: {e.Command.Name}: {e.Command.Id} for {e.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Slash_SlashCommandExecuted(ApplicationCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            Console.WriteLine($"Slash/Info: {e.Context.CommandName}");
            return Task.CompletedTask;
        }

        private static Task Slash_SlashCommandErrored(ApplicationCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            Console.WriteLine($"Slash/Error: {e.Exception.Message} | CN: {e.Context.CommandName} | IID: {e.Context.InteractionId}");
            return Task.CompletedTask;
        }

        private static Task CNext_CommandErrored(CommandsNextExtension ex, CommandErrorEventArgs e)
        {
            if (e.Command == null)
            {
                Console.WriteLine($"{e.Exception.Message}");
            }
            else
            {
                Console.WriteLine($"{e.Command.Name}: {e.Exception.Message}");
            }
            return Task.CompletedTask;
        }

        private static Task Client_SocketOpened(DiscordClient dcl, SocketEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Socket opened");
            return Task.CompletedTask;
        }

        private static Task Client_SocketErrored(DiscordClient dcl, SocketErrorEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Socket has an error! " + e.Exception.Message.ToString());
            return Task.CompletedTask;
        }

        private static Task Client_SocketClosed(DiscordClient dcl, SocketCloseEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Socket closed: " + e.CloseMessage);
            return Task.CompletedTask;
        }

        private static Task Client_Heartbeated(DiscordClient dcl, HeartbeatEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Received Heartbeat:" + e.Ping);
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }
        #endregion
    }
}

