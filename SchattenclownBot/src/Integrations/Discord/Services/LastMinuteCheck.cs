using System;
using System.Threading.Tasks;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.Services
{
    public class LastMinuteCheck
    {
        public static bool BotTimerRunAsync { get; set; }
        public static bool BotAlarmClockRunAsync { get; set; }
        public static bool CheckGreenTask { get; set; }
        public static bool CheckHighQualityAvailable { get; set; }
        public static bool WhereIsClownRunAsync { get; set; }
        public static bool LevelSystemRunAsync { get; set; }
        public static bool LevelSystemRoleDistributionRunAsync { get; set; }
        public static bool BrixLevelSystemRoleDistributionRunAsync { get; set; }
        public static bool CheckBirthdayGz { get; set; }
        public static bool SympathySystemRunAsync { get; set; }
        public static bool TwitchNotifier { get; set; }

        public void RunAsync(int executeSecond)
        {
            new CustomLogger().Information("Starting LastMinuteCheck...", ConsoleColor.Green);
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(TimeSpan.FromSeconds(executeSecond - DateTime.Now.Second % executeSecond));

                    while (true)
                    {
                        BotTimerRunAsync = false;
                        BotAlarmClockRunAsync = false;
                        CheckGreenTask = false;
                        CheckHighQualityAvailable = false;
                        WhereIsClownRunAsync = false;
                        LevelSystemRunAsync = false;
                        LevelSystemRoleDistributionRunAsync = false;
                        BrixLevelSystemRoleDistributionRunAsync = false;
                        CheckBirthdayGz = false;
                        SympathySystemRunAsync = false;
                        TwitchNotifier = false;

                        await Task.Delay(90 * 1000);

                        if (BotTimerRunAsync)
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (BotAlarmClockRunAsync)
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (CheckGreenTask)
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (CheckHighQualityAvailable)
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (WhereIsClownRunAsync)
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (LevelSystemRunAsync)
                        {
                            new CustomLogger().Information("Last Minute RunAsync LevelSystemRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync LevelSystemRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (LevelSystemRoleDistributionRunAsync)
                        {
                            new CustomLogger().Information("Last Minute RunAsync LevelSystemRoleDistributionRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync LevelSystemRoleDistributionRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (BrixLevelSystemRoleDistributionRunAsync)
                        {
                            new CustomLogger().Information("Last Minute RunAsync LevelSystemRoleDistributionRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync LevelSystemRoleDistributionRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (SympathySystemRunAsync)
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (CheckBirthdayGz)
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            new CustomLogger().Information("Last Minute RunAsync RunAsync, Waiting", ConsoleColor.Green);
                        }
                    }
                }
            });
        }
    }
}