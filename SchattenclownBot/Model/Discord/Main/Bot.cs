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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Discord.Main;

public class Bot : IDisposable
{
#if DEBUG
	public const string Prefix = "%";
#else
        public const string Prefix = "%";
#endif
	//public static readonly ulong DevGuild = 881868642600505354;
	public static readonly Connections Connections = Connections.GetConnections();
	public static CancellationTokenSource ShutdownRequest { get; internal set; }
	public static DiscordClient DiscordClient { get; internal set; }
	public static ApplicationCommandsExtension AppCommands { get; internal set; }
	public InteractivityExtension Extension { get; internal set; }
	public VoiceNextExtension NextExtension { get; internal set; }
	private CommandsNextExtension _commandsNextExtension;
	private const ulong DevGuild = 881868642600505354;
	private static string _token = "";
	public static UserStatus CustomStatus { get; internal set; } = UserStatus.Online;
	public static bool Custom { get; internal set; } = false;
	public static string CustomState { get; internal set; } = "/help";
#if DEBUG
	public const string isDevBot = "";
#else
        public const string isDevBot = "";
#endif

	/// <summary>
	/// Initializes a new instance of the <see cref="Bot"/> class.
	/// </summary>
	public Bot()
	{

		_token = Connections.DiscordBotKey;
#if DEBUG
		_token = Connections.DiscordBotDebug;
#endif
		ShutdownRequest = new CancellationTokenSource();

#if DEBUG
		const LogLevel logLevel = LogLevel.Debug;
#else
            const LogLevel logLevel = LogLevel.Information;
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
			StringPrefixes = new List<string> { Prefix },
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
	/// Disposes the Bot.
	/// </summary>
	public void Dispose()
	{
		DiscordClient.Dispose();
		Extension = null;
		_commandsNextExtension = null;
		DiscordClient = null;
		AppCommands = null;
		GC.SuppressFinalize(this);
		Environment.Exit(0);
	}

	/// <summary>
	/// Starts the Bot.
	/// </summary>
	public async Task RunAsync()
	{
		await DiscordClient.ConnectAsync();

		await BotTimer.BotTimerRunAsync();
		await BotAlarmClock.BotAlarmClockRunAsync();
		await APIAsync.ReadFromAPIAsync();
		await GreenCheck.CheckGreenTask(5);
		await GetItRightMee6.CheckHighQualityAvailable(9);
		await WhereIs.WhereIsClownRunAsync(19);
		await UserLevelSystem.LevelSystemRunAsync(29);
		await UserLevelSystem.LevelSystemRoleDistributionRunAsync(39);
		await BirthdayList.CheckBirthdayGz(49);
		await SympathySystem.SympathySystemRunAsync(59);
		await LastMinuteCheck.Check(0);
		await BirthdayList.GenerateBirthdayList();
		//await PlayMusic.TestTask();

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
		discordClient.SocketErrored += Client_SocketError;
		discordClient.Heartbeated += Client_Heartbeat;
		discordClient.Ready += Client_Ready;
		discordClient.Resumed += Client_Resumed;
		/* DiscordClient Events */
		//discordClient.GuildUnavailable += Client_GuildUnavailable;
		//discordClient.GuildAvailable += Client_GuildAvailable;

		/* CommandsNext Error */
		commandsNextExtension.CommandErrored += CNext_CommandError;

		/* Slash Infos */
		discordClient.ApplicationCommandCreated += Discord_ApplicationCommandCreated;
		discordClient.ApplicationCommandDeleted += Discord_ApplicationCommandDeleted;
		discordClient.ApplicationCommandUpdated += Discord_ApplicationCommandUpdated;
		applicationCommandsExtension.SlashCommandErrored += Slash_SlashCommandError;
		applicationCommandsExtension.SlashCommandExecuted += Slash_SlashCommandExecuted;

		//Custom Events
		DiscordClient.ChannelCreated += GetItRightMee6.ItRight;
		//DiscordClient.VoiceStateUpdated += NewChannelCheck.CheckTask;
		DiscordClient.VoiceStateUpdated += PlayMusic.PanicLeaveEvent;
		DiscordClient.VoiceStateUpdated += PlayMusic.GotKickedEvent;
		DiscordClient.ComponentInteractionCreated += PlayMusic.ButtonPressEvent;
		DiscordClient.ComponentInteractionCreated += VoteSystem.GaveRating;
	}

	/// <summary>
	/// Registers the commands.
	/// </summary>
	/// <param name="commandsNextExtension">The commandsnext extension.</param>
	/// <param name="applicationCommandsExtension">The appcommands extension.</param>
	private static void RegisterCommands(CommandsNextExtension commandsNextExtension, ApplicationCommandsExtension applicationCommandsExtension)
	{
		commandsNextExtension.RegisterCommands<Commands.Main>();
		applicationCommandsExtension.RegisterGlobalCommands<Alarm>();
		applicationCommandsExtension.RegisterGlobalCommands<AppCommands.Main>();
		applicationCommandsExtension.RegisterGlobalCommands<Move>();
		applicationCommandsExtension.RegisterGlobalCommands<PlayMusic>();
		applicationCommandsExtension.RegisterGlobalCommands<Poke>();
		applicationCommandsExtension.RegisterGlobalCommands<AppCommands.Timer>();
		applicationCommandsExtension.RegisterGlobalCommands<UserLevel>();
		applicationCommandsExtension.RegisterGlobalCommands<VoteSystem>();
	}

	private static Task Client_Ready(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
	{
		CWLogger.Write($"Starting with Prefix {Prefix} :3", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		CWLogger.Write($"Starting {DiscordClient.CurrentUser.Username}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		CWLogger.Write($"DiscordClient ready!", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		CWLogger.Write($"Shard {discordClient.ShardId}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		CWLogger.Write($"Loading Commands...", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);

		IReadOnlyDictionary<string, Command> registeredCommands = discordClient.GetCommandsNext().RegisteredCommands;
		foreach (KeyValuePair<string, Command> command in registeredCommands)
		{
			CWLogger.Write($"Command {command.Value.Name} loaded.", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		}
		DiscordActivity discordActivity = new()
		{
			Name = Custom ? CustomState : "/help",
			ActivityType = ActivityType.Competing
		};
		discordClient.UpdateStatusAsync(activity: discordActivity, userStatus: Custom ? CustomStatus : UserStatus.Online, idleSince: null);
		CWLogger.Write($"Bot ready!", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Green);
		return Task.CompletedTask;
	}

	private static Task Client_Resumed(DiscordClient discordClient, ReadyEventArgs readyEventArgs)
	{
		CWLogger.Write($"Bot resumed!", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Green);
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
		CWLogger.Write($"Slash/Info: {slashCommandExecutedEventArgs.Context.CommandName}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		return Task.CompletedTask;
	}

	private static Task Slash_SlashCommandError(ApplicationCommandsExtension applicationCommandsExtension, SlashCommandErrorEventArgs slashCommandErrorEventArgs)
	{
		CWLogger.Write($"Slash/Error: {slashCommandErrorEventArgs.Exception.Message} | CN: {slashCommandErrorEventArgs.Context.CommandName} | IID: {slashCommandErrorEventArgs.Context.InteractionId}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
		return Task.CompletedTask;
	}

	private static Task CNext_CommandError(CommandsNextExtension commandsNextExtension, CommandErrorEventArgs commandErrorEventArgs)
	{
		CWLogger.Write(commandErrorEventArgs.Command == null ? $"{commandErrorEventArgs.Exception.Message}" : $"{commandErrorEventArgs.Command.Name}: {commandErrorEventArgs.Exception.Message}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
		return Task.CompletedTask;
	}

	private static Task Client_SocketOpened(DiscordClient discordClient, SocketEventArgs socketEventArgs)
	{
		CWLogger.Write("Socket opened", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		return Task.CompletedTask;
	}

	private static Task Client_SocketError(DiscordClient discordClient, SocketErrorEventArgs socketErrorEventArgs)
	{
		CWLogger.Write("Socket has an error! " + socketErrorEventArgs.Exception.Message, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
		return Task.CompletedTask;
	}

	private static Task Client_SocketClosed(DiscordClient discordClient, SocketCloseEventArgs socketCloseEventArgs)
	{
		CWLogger.Write("Socket closed: " + socketCloseEventArgs.CloseMessage, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
		return Task.CompletedTask;
	}

	private static Task Client_Heartbeat(DiscordClient discordClient, HeartbeatEventArgs heartbeatEventArgs)
	{
		CWLogger.Write("Received Heartbeat:" + heartbeatEventArgs.Ping, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.DarkRed);
		return Task.CompletedTask;
	}
	#endregion
}

