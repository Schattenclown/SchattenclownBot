using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.VoiceNext;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Model.AsyncFunction;
using SchattenclownBot.Model.Discord.AppCommands;
using SchattenclownBot.Model.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        public static DiscordClient DiscordClient;
        public static ApplicationCommandsExtension AppCommands;
        private InteractivityExtension _interactivityExtension;
        private CommandsNextExtension _commandsNextExtension;
        private VoiceNextExtension _voiceNextExtension;

        private static string _token = "";
        private static int _virgin = 0;
        public static UserStatus CustomStatus = UserStatus.Online;
        public static bool Custom = false;
        public static string CustomState = $"/help";

        /// <summary>
        /// Initializes a new instance of the <see cref="Bot"/> class.
        /// </summary>
        public Bot()
        {
            if (_virgin == 0)
            {
                Connections connections = Connections.GetConnections();
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
            DiscordConfiguration discordConfiguration = new()
            {
                Token = _token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MessageCacheSize = 4096,
                MinimumLogLevel = logLevel,
                ShardCount = 1,
                ShardId = 0,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.GuildMembers | DiscordIntents.GuildPresences,
                MobileStatus = false,
                UseCanary = true,
                UsePtb = false,
                AutoRefreshChannelCache = false,
                HttpTimeout = TimeSpan.FromSeconds(60),
                ReconnectIndefinitely = true
            };

            DiscordClient = new DiscordClient(discordConfiguration);

            AppCommands = DiscordClient.UseApplicationCommands(new ApplicationCommandsConfiguration()
            {
                EnableDefaultHelp = true,
                DebugStartup = true,
                ManualOverride = true,
                CheckAllGuilds = true
            });

            _commandsNextExtension = DiscordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { Prefix },
                CaseSensitive = true,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
                DefaultHelpChecks = null,
                EnableDefaultHelp = true,
                EnableDms = true
            });

            _interactivityExtension = DiscordClient.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteMessage,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                ButtonBehavior = ButtonPaginationBehavior.Disable
            });

            _voiceNextExtension = DiscordClient.UseVoiceNext(new VoiceNextConfiguration
            {

            });

            RegisterEventListener(DiscordClient, AppCommands, _commandsNextExtension);
            RegisterCommands(_commandsNextExtension, AppCommands);
        }

        /// <summary>
        /// Disposes the Bot.
        /// </summary>
        public void Dispose()
        {
            DiscordClient.Dispose();
            _interactivityExtension = null;
            _commandsNextExtension = null;
            DiscordClient = null;
            AppCommands = null;
            Environment.Exit(0);
        }

        /// <summary>
        /// Starts the Bot.
        /// </summary>
        public async Task RunAsync()
        {
            await DiscordClient.ConnectAsync();

            DiscordClient.ChannelCreated += GetItRightMee6.ItRight;
            DiscordClient.VoiceStateUpdated += PlayMusic.ResumePlaying;
            DiscordClient.VoiceStateUpdated += PlayMusic.GotKicked;
            DiscordClient.ComponentInteractionCreated += PlayMusic.NextSongPerButton;
#pragma warning disable CS4014
            BotTimer.BotTimerRunAsync();
            BotAlarmClock.BotAlarmClockRunAsync();
            GetItRightMee6.CheckHighQualityAvailable(9);
            WhereIs.WhereIsClownRunAsync(19);
            UserLevelSystem.LevelSystemRunAsync(29);
            UserLevelSystem.LevelSystemRoleDistributionRunAsync(39);
            SympathySystem.SympathySystemRunAsync(59);
            BirthdayList.GenerateBirthdayList();
#pragma warning restore CS4014

            while (!ShutdownRequest.IsCancellationRequested)
            {
                await Task.Delay(2000);
            }
            await DiscordClient.UpdateStatusAsync(activity: null, userStatus: UserStatus.Offline, idleSince: null);
            await DiscordClient.DisconnectAsync();
            await Task.Delay(2500);
            Dispose();
        }

        #region Register Commands & Events

        /// <summary>
        /// Registers the event listener.
        /// </summary>
        /// <param name="discordClient">The discordClient.</param>
        /// <param name="applicationCommandsExtension"></param>
        /// <param name="commandsNextExtension">The commandsNext extension.</param>
        private static void RegisterEventListener(DiscordClient discordClient, ApplicationCommandsExtension applicationCommandsExtension, CommandsNextExtension commandsNextExtension)
        {

            /* DiscordClient Basic Events */
            discordClient.SocketOpened += Client_SocketOpened;
            discordClient.SocketClosed += Client_SocketClosed;
            discordClient.SocketErrored += Client_SocketErrored;
            discordClient.Heartbeated += Client_Heartbeated;
            discordClient.Ready += Client_Ready;
            discordClient.Resumed += Client_Resumed;

            /* DiscordClient Events */
            //discordClient.GuildUnavailable += Client_GuildUnavailable;
            //discordClient.GuildAvailable += Client_GuildAvailable;

            /* CommandsNext Error */
            commandsNextExtension.CommandErrored += CNext_CommandErrored;

            /* Slash Infos */
            discordClient.ApplicationCommandCreated += Discord_ApplicationCommandCreated;
            discordClient.ApplicationCommandDeleted += Discord_ApplicationCommandDeleted;
            discordClient.ApplicationCommandUpdated += Discord_ApplicationCommandUpdated;
            applicationCommandsExtension.SlashCommandErrored += Slash_SlashCommandErrored;
            applicationCommandsExtension.SlashCommandExecuted += Slash_SlashCommandExecuted;

            //discordClient.ComponentInteractionCreated += Discord.AppCommands.Main.Discord_ComponentInteractionCreated;
        }

        /// <summary>
        /// Registers the commands.
        /// </summary>
        /// <param name="commandsNextExtension">The commandsnext extension.</param>
        /// <param name="applicationCommandsExtension">The appcommands extension.</param>
        private static void RegisterCommands(CommandsNextExtension commandsNextExtension, ApplicationCommandsExtension applicationCommandsExtension)
        {
            commandsNextExtension.RegisterCommands<Commands.Main>(); // Commands.Main = Ordner.Class

            /*applicationCommandsExtension.RegisterGuildCommands<AppCommands.Main>(DevGuild); // use to register on guild
            applicationCommandsExtension.RegisterGuildCommands<AppCommands.Alarm>(DevGuild); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGuildCommands<AppCommands.Main>(DevGuild); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGuildCommands<AppCommands.Move>(DevGuild); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGuildCommands<AppCommands.PlayMusic>(DevGuild); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGuildCommands<AppCommands.Poke>(DevGuild); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGuildCommands<AppCommands.Timer>(DevGuild); // use to register global (can take up to an hour)
            //applicationCommandsExtension.RegisterGuildCommands<AppCommands.TriggerHelp>(DevGuild); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGuildCommands<AppCommands.UserLevel>(DevGuild); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGuildCommands<AppCommands.VoteSystem>(DevGuild); // use to register global (can take up to an hour)*/

            applicationCommandsExtension.RegisterGlobalCommands<Alarm>(); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGlobalCommands<AppCommands.Main>(); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGlobalCommands<Move>(); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGlobalCommands<PlayMusic>(); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGlobalCommands<Poke>(); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGlobalCommands<AppCommands.Timer>(); // use to register global (can take up to an hour)
            //applicationCommandsExtension.RegisterGlobalCommands<AppCommands.TriggerHelp>(); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGlobalCommands<UserLevel>(); // use to register global (can take up to an hour)
            applicationCommandsExtension.RegisterGlobalCommands<VoteSystem>(); // use to register global (can take up to an hour)

        }

        private static Task Client_Ready(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Starting with Prefix {Prefix} :3");
            Console.WriteLine($"Starting {DiscordClient.CurrentUser.Username}");
            Console.WriteLine("DiscordClient ready!");
            Console.WriteLine($"Shard {discordClient.ShardId}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Loading Commands...");
            Console.ForegroundColor = ConsoleColor.Magenta;
            IReadOnlyDictionary<string, Command> registeredCommands = discordClient.GetCommandsNext().RegisteredCommands;
            foreach (KeyValuePair<string, Command> command in registeredCommands)
            {
                Console.WriteLine($"Command {command.Value.Name} loaded.");
            }
            DiscordActivity discordActivity = new()
            {
                Name = Bot.Custom ? Bot.CustomState : $"/help",
                ActivityType = ActivityType.Competing
            };
            discordClient.UpdateStatusAsync(activity: discordActivity, userStatus: Bot.Custom ? Bot.CustomStatus : UserStatus.Online, idleSince: null);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Bot ready!");
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }

        private static Task Client_Resumed(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Bot resumed!");
            return Task.CompletedTask;
        }

        private static Task Discord_ApplicationCommandUpdated(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            discordClient.Logger.LogInformation($"Shard {discordClient.ShardId} sent application command updated: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandDeleted(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            discordClient.Logger.LogInformation($"Shard {discordClient.ShardId} sent application command deleted: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Discord_ApplicationCommandCreated(DiscordClient discordClient, ApplicationCommandEventArgs applicationCommandEventArgs)
        {
            discordClient.Logger.LogInformation($"Shard {discordClient.ShardId} sent application command created: {applicationCommandEventArgs.Command.Name}: {applicationCommandEventArgs.Command.Id} for {applicationCommandEventArgs.Command.ApplicationId}");
            return Task.CompletedTask;
        }
        private static Task Slash_SlashCommandExecuted(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandExecutedEventArgs slashCommandExecutedEventArgs)
        {
            Console.WriteLine($"Slash/Info: {slashCommandExecutedEventArgs.Context.CommandName}");
            return Task.CompletedTask;
        }

        private static Task Slash_SlashCommandErrored(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
        {
            Console.WriteLine($"Slash/Error: {slashCommandErrorEventArgs.Exception.Message} | CN: {slashCommandErrorEventArgs.Context.CommandName} | IID: {slashCommandErrorEventArgs.Context.InteractionId}");
            return Task.CompletedTask;
        }

        private static Task CNext_CommandErrored(CommandsNextExtension commandsNextExtension, CommandErrorEventArgs commandErrorEventArgs)
        {
            if (commandErrorEventArgs.Command == null)
            {
                Console.WriteLine($"{commandErrorEventArgs.Exception.Message}");
            }
            else
            {
                Console.WriteLine($"{commandErrorEventArgs.Command.Name}: {commandErrorEventArgs.Exception.Message}");
            }
            return Task.CompletedTask;
        }

        private static Task Client_SocketOpened(DiscordClient discordClient, SocketEventArgs socketEventArgs)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Socket opened");
            return Task.CompletedTask;
        }

        private static Task Client_SocketErrored(DiscordClient discordClient, SocketErrorEventArgs socketErrorEventArgs)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Socket has an error! " + socketErrorEventArgs.Exception.Message.ToString());
            return Task.CompletedTask;
        }

        private static Task Client_SocketClosed(DiscordClient discordClient, SocketCloseEventArgs socketCloseEventArgs)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Socket closed: " + socketCloseEventArgs.CloseMessage);
            return Task.CompletedTask;
        }

        private static Task Client_Heartbeated(DiscordClient discordClient, HeartbeatEventArgs heartbeatEventArgs)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Received Heartbeat:" + heartbeatEventArgs.Ping);
            Console.ForegroundColor = ConsoleColor.Gray;
            return Task.CompletedTask;
        }
        #endregion
    }
}

