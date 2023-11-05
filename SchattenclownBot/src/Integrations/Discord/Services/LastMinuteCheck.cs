using System;
using System.Threading.Tasks;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.Services
{
    public static class LastMinuteCheck
    {
        public static bool BotTimerRunAsync { get; set; }
        public static bool BotAlarmClockRunAsync { get; set; }
        public static bool CheckGreenTask { get; set; }
        public static bool CheckHighQualityAvailable { get; set; }
        public static bool WhereIsClownRunAsync { get; set; }
        public static bool LevelSystemRunAsync { get; set; }
        public static bool LevelSystemRoleDistributionRunAsync { get; set; }
        public static bool CheckBirthdayGz { get; set; }
        public static bool SympathySystemRunAsync { get; set; }
        public static bool TwitchNotifier { get; set; }

        public static void RunAsync(int executeSecond)
        {
            CustomLogger.Information("Starting LastMinuteCheck...", ConsoleColor.Green);
            Task.Run(async () =>
            {
                while (true)
                {
                    while (DateTime.Now.Second != executeSecond)
                    {
                        await Task.Delay(1000);
                    }

                    while (true)
                    {
                        BotTimerRunAsync = false;
                        BotAlarmClockRunAsync = false;
                        CheckGreenTask = false;
                        CheckHighQualityAvailable = false;
                        WhereIsClownRunAsync = false;
                        LevelSystemRunAsync = false;
                        LevelSystemRoleDistributionRunAsync = false;
                        CheckBirthdayGz = false;
                        SympathySystemRunAsync = false;
                        TwitchNotifier = false;

                        await Task.Delay(90 * 1000);

                        if (BotTimerRunAsync)
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (BotAlarmClockRunAsync)
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (CheckGreenTask)
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (CheckHighQualityAvailable)
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (WhereIsClownRunAsync)
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (LevelSystemRunAsync)
                        {
                            CustomLogger.Information("Last Minute RunAsync LevelSystemRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync LevelSystemRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (LevelSystemRoleDistributionRunAsync)
                        {
                            CustomLogger.Information("Last Minute RunAsync LevelSystemRoleDistributionRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync LevelSystemRoleDistributionRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (SympathySystemRunAsync)
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Failed", ConsoleColor.Red);
                        }

                        if (CheckBirthdayGz)
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.Information("Last Minute RunAsync RunAsync, Waiting", ConsoleColor.Green);
                        }
                    }
                }
            });
        }
    }
}