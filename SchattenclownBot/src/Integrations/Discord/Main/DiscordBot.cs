﻿using System;
using System.Linq;
using System.Threading.Tasks;
using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using SchattenclownBot.Integrations.Discord.ApplicationCommands.ModelBased;
using SchattenclownBot.Integrations.Discord.ApplicationCommands.Standalone;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Models;

namespace SchattenclownBot.Integrations.Discord.Main
{
    public class DiscordBot
    {
        public DiscordBot()
        {
#if DEBUG
            Token = Program.Config["APIKeys:DiscordAPIKeyDebug"];
#else
            Token = Program.Config["APIKeys:DiscordAPIKey"];
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
                        ApiChannel = ApiChannel.Canary,
                        AutoRefreshChannelCache = false,
                        HttpTimeout = TimeSpan.FromSeconds(60),
                        ReconnectIndefinitely = true
            };

            DiscordClient = new DiscordClient(discordConfiguration);

            ApplicationCommandsExtension = DiscordClient.UseApplicationCommands(new ApplicationCommandsConfiguration
            {
                        EnableDefaultHelp = false,
                        DebugStartup = false,
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

        public static DiscordClient DiscordClient { get; set; }
        public static DiscordGuild EmojiDiscordGuild { get; set; }
        public static ApplicationCommandsExtension ApplicationCommandsExtension { get; set; }
        public static InteractivityExtension InteractivityExtension { get; set; }
        public static string Token { get; set; }

        public async Task RunAsync()
        {
            await DiscordClient.ConnectAsync();

            while (!DiscordClient.Guilds.Values.Any())
            {
                await Task.Delay(1000);
            }

            EmojiDiscordGuild = DiscordClient.Guilds.Values.FirstOrDefault(x => x.Id == 881868642600505354);

            new LastMinuteCheck().RunAsync(1);
            new GreenCheck().RunAsync(5);
            new GetItRightMee6().RunAsync(9);
            new WhereIs().RunAsync(19);
            new UserLevelSystem().LevelSystemRunAsync(29);
            new UserLevelSystem().LevelSystemRoleDistributionRunAsync(39);
            new UserLevelSystem().BrixLevelSystemRoleDistributionRunAsync(42);
            new BirthdayList().RunAsync(49);
            new SympathySystem().RunAsync(59);
            new TwitchNotifier().RunAsync();
            new Timer().RunAsync();
            new Alarm().RunAsync();
            await new BirthdayList().GenerateBirthdayList();
        }

        public void RegisterEventListener()
        {
            DiscordClient.SocketOpened += new Logging().Client_SocketOpened;
            DiscordClient.SocketClosed += new Logging().Client_SocketClosed;
            DiscordClient.SocketErrored += new Logging().Client_SocketError;
            DiscordClient.Heartbeated += new Logging().Client_Heartbeat;
            DiscordClient.Ready += new Logging().Client_Ready;
            DiscordClient.Resumed += new Logging().Client_Resumed;

            DiscordClient.ApplicationCommandCreated += new Logging().Discord_ApplicationCommandCreated;
            DiscordClient.ApplicationCommandDeleted += new Logging().Discord_ApplicationCommandDeleted;
            DiscordClient.ApplicationCommandUpdated += new Logging().Discord_ApplicationCommandUpdated;

            ApplicationCommandsExtension.SlashCommandExecuted += new Logging().Slash_SlashCommandExecuted;
            ApplicationCommandsExtension.SlashCommandErrored += new Logging().Slash_SlashCommandError;

            DiscordClient.ChannelCreated += new GetItRightMee6().OnChannelCreated;
            DiscordClient.ComponentInteractionCreated += new RegisterKeyAC().ButtonPressEvent;
            DiscordClient.ComponentInteractionCreated += new SympathySystemAC().GaveRating;
        }

        public void RegisterCommands()
        {
            ApplicationCommandsExtension.RegisterGlobalCommands<AlarmAC>();
            ApplicationCommandsExtension.RegisterGlobalCommands<SympathySystemAC>();
            ApplicationCommandsExtension.RegisterGlobalCommands<TimerAC>();
            ApplicationCommandsExtension.RegisterGlobalCommands<TwitchNotifierAC>();
            ApplicationCommandsExtension.RegisterGlobalCommands<UserLevelSystemAC>();

            ApplicationCommandsExtension.RegisterGlobalCommands<MainAC>();
            ApplicationCommandsExtension.RegisterGlobalCommands<MoveAC>();
            ApplicationCommandsExtension.RegisterGlobalCommands<PokeAC>();
            ApplicationCommandsExtension.RegisterGlobalCommands<RegisterKeyAC>();
            ApplicationCommandsExtension.RegisterGlobalCommands<ResetAC>();
        }
    }
}