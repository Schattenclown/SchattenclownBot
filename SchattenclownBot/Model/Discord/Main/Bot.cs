using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using DisCatSharp.VoiceNext;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Model.AsyncFunction;
using SchattenclownBot.Model.Discord.AppCommands;
using SchattenclownBot.Model.Discord.AppCommands.Music;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using Timer = SchattenclownBot.Model.Discord.AppCommands.Timer;

namespace SchattenclownBot.Model.Discord.Main
{
   public class Bot : IDisposable
   {
#if DEBUG
      public const string Prefix = "%";
#else
      public const string Prefix = "%";
#endif
      //public static readonly ulong DevGuild = 881868642600505354;
      public static readonly Connections Connections = Connections.GetConnections();
      public static CancellationTokenSource ShutdownRequest;
      public static DiscordClient DiscordClient;
      public static DiscordGuild EmojiDiscordGuild;
      public static DiscordChannel DebugDiscordChannel;
      public static ApplicationCommandsExtension AppCommands;
      public InteractivityExtension Extension { get; private set; }
      public VoiceNextExtension NextExtension { get; }
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
      ///    Initializes a new instance of the <see cref="Bot" /> class.
      /// </summary>
      public Bot()
      {
         string token = Connections.DiscordBotKey;
#if DEBUG
         token = Connections.DiscordBotDebug;
#endif
         ShutdownRequest = new CancellationTokenSource();

#if DEBUG
         const LogLevel logLevel = LogLevel.Information;
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
                  ReconnectIndefinitely = true,
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

         NextExtension = DiscordClient.UseVoiceNext(new VoiceNextConfiguration());

         RegisterEventListener(DiscordClient, AppCommands, _commandsNextExtension);
         RegisterCommands(_commandsNextExtension, AppCommands);
      }

      /// <summary>
      ///    Disposes the Bot.
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
      ///    Starts the Bot.
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

         SpotifyTasks.CreateDatabaseAndTable();
         await TwitchNotifier.CreateTable_TwitchNotifier();
         _ = TwitchNotifier.Run();

         BotTimer.BotTimerRunAsync();
         BotAlarmClock.BotAlarmClockRunAsync();
         ApiHandler.RunInnerHandlerAsync();
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

         /*DiscordMember dcm = Bot.DiscordClient.GetUserAsync(797971024175824936).Result.ConvertToMember(Bot.DiscordClient.GetGuildAsync(985978911840141372).Result).Result;
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

      #region RegisterForControlPanel Commands & Events

      /// <summary>
      ///    Registers the event listener.
      /// </summary>
      /// <param name="discordClient">The discordClient.</param>
      /// <param name="applicationCommandsExtension"></param>
      /// <param name="commandsNextExtension">The commandsNext extension.</param>
      private static void RegisterEventListener(DiscordClient discordClient, ApplicationCommandsExtension applicationCommandsExtension, CommandsNextExtension commandsNextExtension)
      {
         /* DiscordClient Basic Events */
         discordClient.SocketOpened += Client_SocketOpened;
         discordClient.SocketClosed += Client_SocketClosed;
         discordClient.SocketErrored += Client_SocketError;
         discordClient.Heartbeated += Client_Heartbeat;
         discordClient.Ready += Client_Ready;
         discordClient.Resumed += Client_Resumed;
         /* DiscordClient Events */
         //discordClient.GuildUnavailable += Client_GuildUnavailable;
         //discordClient.GuildAvailable += Client_GuildAvailable;

         /* CommandsNext Error */
         commandsNextExtension.CommandErrored += CNext_CommandError;

         //Custom Events
         DiscordClient.ChannelCreated += GetItRightMee6.ItRight;
         //DiscordClient.VoiceStateUpdated += NewChannelCheck.CheckTask;
         //DiscordClient.VoiceStateUpdated += Events.PanicLeaveEvent;
         //DiscordClient.VoiceStateUpdated += Events.GotKickedEvent;
         //DiscordClient.VoiceStateUpdated += AutoKickEvent.ConnectedEvent;
         DiscordClient.ComponentInteractionCreated += Events.ButtonPressEvent;
         DiscordClient.ComponentInteractionCreated += RegisterKey.ButtonPressEvent;
         DiscordClient.ComponentInteractionCreated += VoteSystem.GaveRating;
         DiscordClient.ComponentInteractionCreated += RegisterForControlPanel.RegisterEvent;

         /* Slash Infos */
         discordClient.ApplicationCommandCreated += Discord_ApplicationCommandCreated;
         discordClient.ApplicationCommandDeleted += Discord_ApplicationCommandDeleted;
         discordClient.ApplicationCommandUpdated += Discord_ApplicationCommandUpdated;
         applicationCommandsExtension.SlashCommandErrored += Slash_SlashCommandError;
         applicationCommandsExtension.SlashCommandExecuted += Slash_SlashCommandExecuted;
      }

      /// <summary>
      ///    Registers the commands.
      /// </summary>
      /// <param name="commandsNextExtension">The commandsnext extension.</param>
      /// <param name="applicationCommandsExtension">The appcommands extension.</param>
      private static void RegisterCommands(CommandsNextExtension commandsNextExtension, ApplicationCommandsExtension applicationCommandsExtension)
      {
         commandsNextExtension.RegisterCommands<Commands.Main>();
         applicationCommandsExtension.RegisterGlobalCommands<Alarm>();
         applicationCommandsExtension.RegisterGlobalCommands<AppCommands.Main>();
         applicationCommandsExtension.RegisterGlobalCommands<Move>();
         applicationCommandsExtension.RegisterGlobalCommands<DiscordRequests>();
         applicationCommandsExtension.RegisterGlobalCommands<Poke>();
         applicationCommandsExtension.RegisterGlobalCommands<Timer>();
         applicationCommandsExtension.RegisterGlobalCommands<UserLevel>();
         applicationCommandsExtension.RegisterGlobalCommands<VoteSystem>();
         applicationCommandsExtension.RegisterGlobalCommands<RegisterForControlPanel>();
         applicationCommandsExtension.RegisterGlobalCommands<RegisterTwitch>();
         applicationCommandsExtension.RegisterGlobalCommands<RegisterKey>();
      }

      private static Task Client_Ready(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
      {
         CwLogger.Write($"Starting with Prefix {Prefix} :3", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         CwLogger.Write($"Starting {DiscordClient.CurrentUser.Username}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         CwLogger.Write("DiscordClient ready!", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         CwLogger.Write($"Shard {discordClient.ShardId}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         CwLogger.Write("Loading Commands...", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);

         IReadOnlyDictionary<string, Command> registeredCommands = discordClient.GetCommandsNext().RegisteredCommands;
         foreach (KeyValuePair<string, Command> command in registeredCommands)
         {
            CwLogger.Write($"Command {command.Value.Name} loaded.", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         }

         DiscordActivity discordActivity = new()
         {
                  Name = Custom ? CustomState : "/help",
                  ActivityType = ActivityType.Competing
         };
         discordClient.UpdateStatusAsync(discordActivity, Custom ? CustomStatus : UserStatus.Online);
         CwLogger.Write("Bot ready!", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Green);
         return Task.CompletedTask;
      }

      private static Task Client_Resumed(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
      {
         CwLogger.Write("Bot resumed!", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Green);
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
         CwLogger.Write($"Slash/Info: {slashCommandExecutedEventArgs.Context.CommandName}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         return Task.CompletedTask;
      }

      private static Task Slash_SlashCommandError(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
      {
         CwLogger.Write($"Slash/Error: {slashCommandErrorEventArgs.Exception.Message} | CN: {slashCommandErrorEventArgs.Context.CommandName} | IID: {slashCommandErrorEventArgs.Context.InteractionId}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
         return Task.CompletedTask;
      }

      private static Task CNext_CommandError(CommandsNextExtension commandsNextExtension, CommandErrorEventArgs commandErrorEventArgs)
      {
         CwLogger.Write(commandErrorEventArgs.Command == null ? $"{commandErrorEventArgs.Exception.Message}" : $"{commandErrorEventArgs.Command.Name}: {commandErrorEventArgs.Exception.Message}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
         return Task.CompletedTask;
      }

      private static Task Client_SocketOpened(DiscordClient discordClient, SocketEventArgs socketEventArgs)
      {
         CwLogger.Write("Socket opened", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
         return Task.CompletedTask;
      }

      private static Task Client_SocketError(DiscordClient discordClient, SocketErrorEventArgs socketErrorEventArgs)
      {
         CwLogger.Write("Socket has an error! " + socketErrorEventArgs.Exception.Message, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
         return Task.CompletedTask;
      }

      private static Task Client_SocketClosed(DiscordClient discordClient, SocketCloseEventArgs socketCloseEventArgs)
      {
         CwLogger.Write("Socket closed: " + socketCloseEventArgs.CloseMessage, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
         return Task.CompletedTask;
      }

      private static Task Client_Heartbeat(DiscordClient discordClient, HeartbeatEventArgs heartbeatEventArgs)
      {
         CwLogger.Write("Received Heartbeat:" + heartbeatEventArgs.Ping, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.DarkRed);
         return Task.CompletedTask;
      }

      #endregion
   }
}