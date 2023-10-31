using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Integrations.Discord.ApplicationCommands;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Models;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;
using Timer = SchattenclownBot.Integrations.Discord.ApplicationCommands.Timer;

namespace SchattenclownBot.Integrations.Discord.Main
{
    public class DiscordBot : IDisposable
    {
#if DEBUG
        public const string Prefix = "%";
#else
        public const string Prefix = "%";
#endif
        //public static readonly ulong DevGuild = 881868642600505354;
        public static Logger ErrorLogger;
        public static readonly Connections Connections = Connections.GetConnections();
        public static CancellationTokenSource ShutdownRequest;
        public static DiscordClient DiscordClient;
        public static DiscordGuild EmojiDiscordGuild;
        public static DiscordChannel DebugDiscordChannel;
        public static ApplicationCommandsExtension AppCommands;
        public InteractivityExtension Extension { get; private set; }
        private CommandsNextExtension _commandsNextExtension;
        private const ulong DevGuild = 881868642600505354;
        public static UserStatus CustomStatus = UserStatus.Online;
        public static bool Custom = false;
        public static string CustomState = "/help";
#if DEBUG
        public const string isDevBot = "";
#else
        public const string isDevBot = "";
#endif

        /// <summary>
        ///     Initializes a new instance of the <see cref="DiscordBot" /> class.
        /// </summary>
        public DiscordBot()
        {
            Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle> defaultThemeStyle = new()
            {
                        {
                                    ConsoleThemeStyle.Text, new SystemConsoleThemeStyle
                                    {
                                                Foreground = ConsoleColor.Green
                                    }
                        },
                        {
                                    ConsoleThemeStyle.String, new SystemConsoleThemeStyle
                                    {
                                                Foreground = ConsoleColor.Yellow
                                    }
                        }
            };

            Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle> errorThemeStyle = new()
            {
                        {
                                    ConsoleThemeStyle.Text, new SystemConsoleThemeStyle
                                    {
                                                Foreground = ConsoleColor.Red
                                    }
                        },
                        {
                                    ConsoleThemeStyle.String, new SystemConsoleThemeStyle
                                    {
                                                Foreground = ConsoleColor.Yellow
                                    }
                        }
            };

            SystemConsoleTheme defaultTheme = new(defaultThemeStyle);
            SystemConsoleTheme errorTheme = new(errorThemeStyle);

            Log.Logger = new LoggerConfiguration().WriteTo.Console(theme: defaultTheme).CreateLogger();
            ErrorLogger = new LoggerConfiguration().WriteTo.Console(theme: errorTheme).CreateLogger();

            string token = Connections.DiscordBotKey;
#if DEBUG
            token = Connections.DiscordBotDebug;
#endif
            ShutdownRequest = new CancellationTokenSource();

#if DEBUG
            const LogLevel logLevel = LogLevel.Warning;
#else
            const LogLevel logLevel = LogLevel.Information;
#endif
            DiscordConfiguration discordConfiguration = new()
            {
                        Token = token,
                        TokenType = TokenType.Bot,
                        AutoReconnect = true,
                        MessageCacheSize = 4096,
                        MinimumLogLevel = logLevel,
                        ShardCount = 1,
                        ShardId = 0,
                        Intents = DiscordIntents.All,
                        MobileStatus = false,
                        UseCanary = true,
                        UsePtb = false,
                        AutoRefreshChannelCache = false,
                        HttpTimeout = TimeSpan.FromSeconds(60),
                        ReconnectIndefinitely = true
            };

            DiscordClient = new DiscordClient(discordConfiguration);

            AppCommands = DiscordClient.UseApplicationCommands(new ApplicationCommandsConfiguration
            {
                        EnableDefaultHelp = true,
                        DebugStartup = true,
                        ManualOverride = true,
                        CheckAllGuilds = true
            });

            _commandsNextExtension = DiscordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                        StringPrefixes = new List<string>
                        {
                                    Prefix
                        },
                        CaseSensitive = true,
                        EnableMentionPrefix = true,
                        IgnoreExtraArguments = true,
                        DefaultHelpChecks = null,
                        EnableDefaultHelp = true,
                        EnableDms = true
            });

            Extension = DiscordClient.UseInteractivity(new InteractivityConfiguration
            {
                        PaginationBehaviour = PaginationBehaviour.WrapAround,
                        PaginationDeletion = PaginationDeletion.DeleteMessage,
                        PollBehaviour = PollBehaviour.DeleteEmojis,
                        ButtonBehavior = ButtonPaginationBehavior.Disable
            });

            RegisterEventListener(DiscordClient, AppCommands, _commandsNextExtension);
            RegisterCommands(_commandsNextExtension, AppCommands);
        }

        /// <summary>
        ///     Disposes the DiscordBot.
        /// </summary>
        public void Dispose()
        {
            DiscordClient.Dispose();
            Extension = null;
            _commandsNextExtension = null;
            DiscordClient = null;
            AppCommands = null;
            Environment.Exit(0);
        }

        /// <summary>
        ///     Starts the DiscordBot.
        /// </summary>
        public async Task RunAsync()
        {
            await DiscordClient.ConnectAsync();

            bool levelSystemVirgin = true;
            do
            {
                if (DiscordClient.Guilds.ToList().Count != 0)
                {
                    EmojiDiscordGuild = DiscordClient.Guilds.Values.FirstOrDefault(x => x.Id == 881868642600505354);
                    levelSystemVirgin = false;
                }

                await Task.Delay(1000);
            } while (levelSystemVirgin);

            TwitchNotifier.TwitchNotifierRunAsync();
            BotTimer.BotTimerRunAsync();
            BotAlarmClock.BotAlarmClockRunAsync();
            GreenCheck.CheckGreenTask(5);
            GetItRightMee6.CheckHighQualityAvailable(9);
            WhereIs.WhereIsClownRunAsync(19);
            UserLevelSystem.LevelSystemRunAsync(29);
            UserLevelSystem.LevelSystemRoleDistributionRunAsync(39);
            BirthdayList.CheckBirthdayGz(49);
            SympathySystem.SympathySystemRunAsync(59);
            LastMinuteCheck.Check(0);
            await BirthdayList.GenerateBirthdayList();

#if RELEASE
            DebugDiscordChannel = await DiscordClient.GetChannelAsync(1042762701329412146);
#elif DEBUG
            DebugDiscordChannel = await DiscordClient.GetChannelAsync(881876137297477642);
#endif

            /*DiscordMember dcm = DiscordBot.DiscordClient.GetUserAsync(797971024175824936).Result.ConvertToMember(DiscordBot.DiscordClient.GetGuildAsync(985978911840141372).Result).Result;
            await dcm.DisconnectFromVoiceAsync();*/

            while (!ShutdownRequest.IsCancellationRequested)
            {
                await Task.Delay(2000);
            }

            await DiscordClient.UpdateStatusAsync(null, UserStatus.Offline);
            await DiscordClient.DisconnectAsync();
            await Task.Delay(2500);
            Dispose();
        }

        /// <summary>
        ///     Registers the event listener.
        /// </summary>
        /// <param name="discordClient">The discordClient.</param>
        /// <param name="applicationCommandsExtension"></param>
        /// <param name="commandsNextExtension">The commandsNext extension.</param>
        private static void RegisterEventListener(DiscordClient discordClient, ApplicationCommandsExtension applicationCommandsExtension, CommandsNextExtension commandsNextExtension)
        {
            //Custom Events
            DiscordClient.ChannelCreated += GetItRightMee6.ItRight;
            DiscordClient.ComponentInteractionCreated += RegisterKey.ButtonPressEvent;
            DiscordClient.ComponentInteractionCreated += VoteSystem.GaveRating;

            /* DiscordClient Basic Events */
            discordClient.SocketOpened += Logging.Client_SocketOpened;
            discordClient.SocketClosed += Logging.Client_SocketClosed;
            discordClient.SocketErrored += Logging.Client_SocketError;
            discordClient.Heartbeated += Logging.Client_Heartbeat;
            discordClient.Ready += Logging.Client_Ready;
            discordClient.Resumed += Logging.Client_Resumed;

            /* Slash Infos */
            discordClient.ApplicationCommandCreated += Logging.Discord_ApplicationCommandCreated;
            discordClient.ApplicationCommandDeleted += Logging.Discord_ApplicationCommandDeleted;
            discordClient.ApplicationCommandUpdated += Logging.Discord_ApplicationCommandUpdated;
            applicationCommandsExtension.SlashCommandErrored += Logging.Slash_SlashCommandError;
            applicationCommandsExtension.SlashCommandExecuted += Logging.Slash_SlashCommandExecuted;
        }

        /// <summary>
        ///     Registers the commands.
        /// </summary>
        /// <param name="commandsNextExtension">The commandsnext extension.</param>
        /// <param name="applicationCommandsExtension">The appcommands extension.</param>
        private static void RegisterCommands(CommandsNextExtension commandsNextExtension, ApplicationCommandsExtension applicationCommandsExtension)
        {
            applicationCommandsExtension.RegisterGlobalCommands<Alarm>();
            applicationCommandsExtension.RegisterGlobalCommands<ApplicationCommands.Main>();
            applicationCommandsExtension.RegisterGlobalCommands<Move>();
            applicationCommandsExtension.RegisterGlobalCommands<Poke>();
            applicationCommandsExtension.RegisterGlobalCommands<Timer>();
            applicationCommandsExtension.RegisterGlobalCommands<UserLevel>();
            applicationCommandsExtension.RegisterGlobalCommands<VoteSystem>();
            applicationCommandsExtension.RegisterGlobalCommands<RegisterTwitch>();
            applicationCommandsExtension.RegisterGlobalCommands<RegisterKey>();
        }
    }
}