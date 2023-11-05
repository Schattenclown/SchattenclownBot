using System;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Integrations.Discord.ApplicationCommands;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Models;

namespace SchattenclownBot.Integrations.Discord.Main
{
    public class DiscordBot
    {
        public DiscordBot()
        {
#if DEBUG
            Token = Config["APIKeys:DiscordAPIKeyDebug"];
#else
            Token = Config["APIKeys:DiscordAPIKey"];
#endif

            DiscordConfiguration discordConfiguration = new()
            {
                        Token = Token ?? throw new InvalidOperationException(),
                        TokenType = TokenType.Bot,
                        AutoReconnect = true,
                        MessageCacheSize = 4096,
                        MinimumLogLevel = LogLevel.Error,
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

            ApplicationCommandsExtension = DiscordClient.UseApplicationCommands(new ApplicationCommandsConfiguration
            {
                        EnableDefaultHelp = false,
                        DebugStartup = false,
                        ManualOverride = true,
                        CheckAllGuilds = true
            });

            InteractivityExtension = DiscordClient.UseInteractivity(new InteractivityConfiguration
            {
                        PaginationBehaviour = PaginationBehaviour.WrapAround,
                        PaginationDeletion = PaginationDeletion.DeleteMessage,
                        PollBehaviour = PollBehaviour.DeleteEmojis,
                        ButtonBehavior = ButtonPaginationBehavior.Disable
            });

            RegisterEventListener();
            RegisterCommands();
        }

        public static IConfigurationRoot Config { get; } = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true).Build();
        public static DiscordClient DiscordClient { get; set; }
        public static DiscordGuild EmojiDiscordGuild { get; set; }
        public static ApplicationCommandsExtension ApplicationCommandsExtension { get; set; }
        public InteractivityExtension InteractivityExtension { get; set; }
        private static string Token { get; set; }

        public async Task RunAsync()
        {
            await DiscordClient.ConnectAsync();

            while (!DiscordClient.Guilds.Values.Any())
            {
                await Task.Delay(1000);
            }

            EmojiDiscordGuild = DiscordClient.Guilds.Values.FirstOrDefault(x => x.Id == 881868642600505354);

            LastMinuteCheck.RunAsync(0);
            GreenCheck.RunAsync(5);
            GetItRightMee6.RunAsync(9);
            WhereIs.RunAsync(19);
            UserLevelSystem.LevelSystemRunAsync(29);
            UserLevelSystem.LevelSystemRoleDistributionRunAsync(39);
            BirthdayList.RunAsync(49);
            SympathySystem.RunAsync(59);
            TwitchNotifier.RunAsync();
            BotTimer.RunAsync();
            BotAlarmClock.RunAsync();
            await BirthdayList.GenerateBirthdayList();
        }

        private static void RegisterEventListener()
        {
            DiscordClient.SocketOpened += Logging.Client_SocketOpened;
            DiscordClient.SocketClosed += Logging.Client_SocketClosed;
            DiscordClient.SocketErrored += Logging.Client_SocketError;
            DiscordClient.Heartbeated += Logging.Client_Heartbeat;
            DiscordClient.Ready += Logging.Client_Ready;
            DiscordClient.Resumed += Logging.Client_Resumed;

            DiscordClient.ApplicationCommandCreated += Logging.Discord_ApplicationCommandCreated;
            DiscordClient.ApplicationCommandDeleted += Logging.Discord_ApplicationCommandDeleted;
            DiscordClient.ApplicationCommandUpdated += Logging.Discord_ApplicationCommandUpdated;

            ApplicationCommandsExtension.SlashCommandExecuted += Logging.Slash_SlashCommandExecuted;
            ApplicationCommandsExtension.SlashCommandErrored += Logging.Slash_SlashCommandError;

            DiscordClient.ChannelCreated += GetItRightMee6.OnChannelCreated;
            DiscordClient.ComponentInteractionCreated += RegisterKey.ButtonPressEvent;
            DiscordClient.ComponentInteractionCreated += VoteSystem.GaveRating;
        }

        private static void RegisterCommands()
        {
            ApplicationCommandsExtension.RegisterGlobalCommands<Alarm>();
            ApplicationCommandsExtension.RegisterGlobalCommands<ApplicationCommands.Main>();
            ApplicationCommandsExtension.RegisterGlobalCommands<Move>();
            ApplicationCommandsExtension.RegisterGlobalCommands<Poke>();
            ApplicationCommandsExtension.RegisterGlobalCommands<Timer>();
            ApplicationCommandsExtension.RegisterGlobalCommands<UserLevel>();
            ApplicationCommandsExtension.RegisterGlobalCommands<VoteSystem>();
            ApplicationCommandsExtension.RegisterGlobalCommands<RegisterTwitch>();
            ApplicationCommandsExtension.RegisterGlobalCommands<RegisterKey>();
        }
    }
}