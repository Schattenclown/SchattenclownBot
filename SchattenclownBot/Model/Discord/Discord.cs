using DisCatSharp;
using DisCatSharp.ApplicationCommands;
using DisCatSharp.CommandsNext;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using DisCatSharp.Interactivity;
using DisCatSharp.Interactivity.Enums;
using DisCatSharp.Interactivity.EventHandling;
using DisCatSharp.Interactivity.Extensions;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence;

using static SchattenclownBot.Model.Discord.Events.ApplicationCommandsEvents;
using static SchattenclownBot.Model.Discord.Events.ClientEvents;
using static SchattenclownBot.Model.Discord.Events.GuildEvents;

namespace SchattenclownBot.Model.Discord
{
    #region MultiDict
    /// <summary>
    /// Multidictionary
    /// </summary>
    /// <typeparam name="TKey">Key</typeparam>
    /// <typeparam name="TValue">Value</typeparam>
    public class MultiDict<TKey, TValue>
    {
        private readonly Dictionary<TKey, List<TValue>> _data = new Dictionary<TKey, List<TValue>>();

        /// <summary>
        /// Adds a <see cref="List{T}"/> to an <see cref="Dictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="k">Key</param>
        /// <param name="v">Value</param>
        public void Add(TKey k, TValue v)
        {
            if (_data.ContainsKey(k))
                _data[k].Add(v);
            else
                _data.Add(k, new List<TValue>() { v });
        }

        /// <summary>
        /// Deletes a <see cref="List{T}"/> from  an <see cref="Dictionary{TKey, TValue}"/>
        /// </summary>
        /// <param name="k">Key</param>
        /// <param name="v">Value</param>
        public void Del(TKey k, TValue v)
        {
            if (_data.ContainsKey(k))
                _data[k].Remove(v);
        }

        /// <summary>
        /// Gets a <see cref="Dictionary{TKey, TValue}"/>
        /// </summary>
        /// <returns>Dictionary</returns>
        public Dictionary<TKey, List<TValue>> Get()
        {
            return _data;
        }
    }
    #endregion

    public class DiscordBot : IDisposable
    {
        public static string token = "";
        public static int virgin = 0;
        public static DiscordClient Client { get; internal set; }
        public static List<ScTimer> scTimers;
        public static List<ScAlarmClock> scAlarmClocks;
        public static ApplicationCommandsExtension ApplicationCommands { get; internal set; }
        public static CommandsNextExtension CNext { get; internal set; }
        public static InteractivityExtension INext { get; internal set; }
        public static CancellationTokenSource ShutdownRequest;
        public static readonly ulong testguild = 881868642600505354;
        public static string prefix = "sc/";

        /// <summary>
        /// Binarie to text.
        /// </summary>
        /// <param name="data">The binary data.</param>
        public static string BinaryToText(byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public DiscordBot()
        {
            if (virgin == 0)
            {
                Connections connections = Connections.GetConnections();
                token = connections.DiscordBotKey;
#if DEBUG
                token = connections.DiscordBotDebug;
#endif
                virgin = 69;
            }
            ShutdownRequest = new CancellationTokenSource();

            LogLevel logLevel;
#if DEBUG
            logLevel = LogLevel.Debug;
#else
            logLevel = LogLevel.Error;
#endif
            var cfg = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MessageCacheSize = 2048,
                MinimumLogLevel = logLevel,
                ShardCount = 1,
                ShardId = 0,
                Intents = DiscordIntents.AllUnprivileged,
                MobileStatus = false,
                UseCanary = false,
                AutoRefreshChannelCache = false
            };

            Client = new DiscordClient(cfg);
            ApplicationCommands = Client.UseApplicationCommands();
            CNext = Client.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { prefix },
                CaseSensitive = true,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
                DefaultHelpChecks = null,
                EnableDefaultHelp = true,
                EnableDms = true
            });

            INext = Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2),
                PaginationBehaviour = PaginationBehaviour.WrapAround,
                PaginationDeletion = PaginationDeletion.DeleteEmojis,
                PollBehaviour = PollBehaviour.DeleteEmojis,
                AckPaginationButtons = true,
                ButtonBehavior = ButtonPaginationBehavior.Disable,
                PaginationButtons = new PaginationButtons()
                {
                    SkipLeft = new DiscordButtonComponent(ButtonStyle.Primary, "pgb-skip-left", "First", false, new DiscordComponentEmoji("⏮️")),
                    Left = new DiscordButtonComponent(ButtonStyle.Primary, "pgb-left", "Previous", false, new DiscordComponentEmoji("◀️")),
                    Stop = new DiscordButtonComponent(ButtonStyle.Danger, "pgb-stop", "Cancel", false, new DiscordComponentEmoji("⏹️")),
                    Right = new DiscordButtonComponent(ButtonStyle.Primary, "pgb-right", "Next", false, new DiscordComponentEmoji("▶️")),
                    SkipRight = new DiscordButtonComponent(ButtonStyle.Primary, "pgb-skip-right", "Last", false, new DiscordComponentEmoji("⏭️"))
                },
                ResponseMessage = "Something went wrong.",
                ResponseBehavior = InteractionResponseBehavior.Respond
            });

            RegisterEventListener(Client, ApplicationCommands, CNext);
            RegisterCommands(CNext, ApplicationCommands);

        }
        public void Dispose()
        {
            Client.Dispose();
            INext = null;
            CNext = null;
            Client = null;
            ApplicationCommands = null;
        }

        public async Task RunAsync()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            await Client.ConnectAsync();
            Console.WriteLine($"Starting with Prefix {prefix}");
            Console.WriteLine($"Starting {Client.CurrentUser.Username}");

#pragma warning disable CS4014 // Da auf diesen Aufruf nicht gewartet wird, wird die Ausführung der aktuellen Methode vor Abschluss des Aufrufs fortgesetzt.
            LevelSystem();
            ScTimersRunAsync();
            ScAlarmClocksRunAsync();
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

        public async Task ScTimersRunAsync()
        {
            scTimers = DB_ScTimers.ReadAll();

            await Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var scTimer in scTimers)
                    {
                        if (scTimer.NotificationTime < DateTime.Now)
                        {
                            var chn = await Client.GetChannelAsync(scTimer.ChannelId);
                            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();
                            eb.Color = DiscordColor.Red;
                            eb.WithDescription($"<@{scTimer.MemberId}> Timer for {scTimer.NotificationTime} is up!");

                            ScTimer.Delete(scTimer);
                            for (int i = 0; i < 3; i++)
                            {
                                await chn.SendMessageAsync(eb.Build());
                                await Task.Delay(50);
                            }
                        }
                    }

                    if (DateTime.Now.Second == 15)
                        scTimers = DB_ScTimers.ReadAll();

                    await Task.Delay(1000 * 1);
                }
            });
        }
        public void ScTimersDBRefresh()
        {
            scTimers = DB_ScTimers.ReadAll();
        }

        public async Task ScAlarmClocksRunAsync()
        {
            scAlarmClocks = DB_ScAlarmClocks.ReadAll();

            await Task.Run(async () =>
            {
                while (true)
                {
                    foreach (var scAlarmClock in scAlarmClocks)
                    {
                        if (scAlarmClock.NotificationTime < DateTime.Now)
                        {
                            var chn = await Client.GetChannelAsync(scAlarmClock.ChannelId);
                            DiscordEmbedBuilder eb = new DiscordEmbedBuilder();
                            eb.Color = DiscordColor.Red;
                            eb.WithDescription($"<@{scAlarmClock.MemberId}> Alarm for {scAlarmClock.NotificationTime} rings!");

                            ScAlarmClock.Delete(scAlarmClock);
                            for (int i = 0; i < 3; i++)
                            {
                                await chn.SendMessageAsync(eb.Build());
                                await Task.Delay(50);
                            }
                        }
                    }

                    if (DateTime.Now.Second == 30)
                        scAlarmClocks = DB_ScAlarmClocks.ReadAll();

                    await Task.Delay(1000 * 1);
                }
            });
        }
        public void ScAlarmClocksDBRefresh()
        {
            scAlarmClocks = DB_ScAlarmClocks.ReadAll();
        }

        public async Task LevelSystem()
        {
            bool levelSystemVirign = true;

            await Task.Run(async () =>
            {
                do
                {
                    if (Client.Guilds.ToList().Count != 0)
                    {
                        if (levelSystemVirign)
                        {
                            var guildsList = Client.Guilds.ToList();
                            foreach (var guildItem in guildsList)
                            {
                                DcLevelSystem.CreateTable(guildItem.Value.Id);
                            }
                            levelSystemVirign = false;
                        }
                    }

                    await Task.Delay(1000);
                } while (levelSystemVirign);

                while (true)
                {
                    var guildsList = Client.Guilds.ToList();
                    foreach (var guildItem in guildsList)
                    {
                        List<DcLevelSystem> dcLevelSystemsList = new List<DcLevelSystem>();
                        dcLevelSystemsList = DcLevelSystem.Read(guildItem.Value.Id);

                        var guildMembers = guildItem.Value.Members;
                        foreach (var memberItem in guildMembers)
                        {
                            if (memberItem.Value.VoiceState != null)
                            {
                                DcLevelSystem dcLevelSystem = new DcLevelSystem();
                                dcLevelSystem.MemberId = memberItem.Value.Id;
                                bool found = false;

                                foreach (DcLevelSystem dcLevelSystemItem in dcLevelSystemsList)
                                {
                                    if (memberItem.Value.Id == dcLevelSystemItem.MemberId)
                                    {
                                        dcLevelSystem.OnlineTicks = dcLevelSystemItem.OnlineTicks + 1;
                                        found = true;
                                    }
                                }

                                if (!found)
                                {
                                    dcLevelSystem.OnlineTicks = 1;
                                    DcLevelSystem.Add(guildItem.Value.Id, dcLevelSystem);
                                }

                                if (found)
                                {
                                    DcLevelSystem.Change(guildItem.Value.Id, dcLevelSystem);
                                }
                            }
                        }
                    }

                    while (DateTime.Now.Second != 59)
                    {
                        await Task.Delay(1000);
                    }
                }
            });
        }

        #region Register Commands & Events
        /// <summary>
        /// Registers the event listener.
        /// </summary>
        /// <param name="client">The discord client.</param>
        /// <param name="ac">The application commands extension.</param>
        /// <param name="cnext">The commands next extension.</param>
        private void RegisterEventListener(DiscordClient client, ApplicationCommandsExtension ac, CommandsNextExtension cnext)
        {

            client.Ready += Client_Ready;
            client.Resumed += Client_Resumed;
            client.SocketOpened += Client_SocketOpened;
            client.SocketClosed += Client_SocketClosed;
            client.SocketErrored += Client_SocketErrored;
            client.Heartbeated += Client_Heartbeated;
            client.GuildUnavailable += Client_GuildUnavailable;
            client.GuildAvailable += Client_GuildAvailable;
            client.GuildCreated += Client_GuildCreated;
            client.GuildDeleted += Client_GuildDeleted;
            client.ApplicationCommandCreated += Discord_ApplicationCommandCreated;
            client.ApplicationCommandDeleted += Discord_ApplicationCommandDeleted;
            client.ApplicationCommandUpdated += Discord_ApplicationCommandUpdated;
            client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            client.ApplicationCommandPermissionsUpdated += Client_ApplicationCommandPermissionsUpdated;

#if DEBUG
            client.UnknownEvent += Client_UnknownEvent;
#endif
            ac.SlashCommandErrored += Ac_SlashCommandErrored;
            ac.SlashCommandExecuted += Ac_SlashCommandExecuted;
            ac.ContextMenuErrored += Ac_ContextMenuErrored;
            ac.ContextMenuExecuted += Ac_ContextMenuExecuted;
            cnext.CommandErrored += CNext_CommandErrored;
        }

        /// <summary>
        /// Registers the commands.
        /// </summary>
        /// <param name="cnext">The commands next extension.</param>
        /// <param name="ac">The application commands extensions.</param>
        private static void RegisterCommands(CommandsNextExtension cnext, ApplicationCommandsExtension ac)
        {
            cnext.RegisterCommands<Discord.Interaction.Main>();
#if DEBUG
            ac.RegisterCommands<Discord.Interaction.Slash>(testguild, perms =>
            {
                perms.AddRole(889266812267663380, true);
            });
#else
            ac.RegisterCommands<Discord.Interaction.Slash>();
#endif
        }
        #endregion
    }
}

