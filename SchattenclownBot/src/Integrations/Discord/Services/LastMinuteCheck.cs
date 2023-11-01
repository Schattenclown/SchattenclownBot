using System;
using System.Threading.Tasks;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Integrations.Discord.Services
{
    internal class LastMinuteCheck
    {
        public static bool BotTimerRunAsync;
        public static bool BotAlarmClockRunAsync;
        public static bool CheckGreenTask;
        public static bool CheckHighQualityAvailable;
        public static bool WhereIsClownRunAsync;
        public static bool LevelSystemRunAsync;
        public static bool LevelSystemRoleDistributionRunAsync;
        public static bool CheckBirthdayGz;
        public static bool SympathySystemRunAsync;
        public static bool TwitchNotifier;

        public static void Check(int executeSecond)
        {
            CustomLogger.ToConsole("Starting LastMinuteCheck...", ConsoleColor.Green);
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

                        await Task.Delay(60 * 1000);

                        if (BotTimerRunAsync)
                        {
                            CustomLogger.ToConsole("Last Minute Check BotTimerRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check BotTimerRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (BotAlarmClockRunAsync)
                        {
                            CustomLogger.ToConsole("Last Minute Check BotAlarmClockRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check BotAlarmClockRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (CheckGreenTask)
                        {
                            CustomLogger.ToConsole("Last Minute Check CheckGreenTask, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check CheckGreenTask, Failed", ConsoleColor.Red);
                        }

                        if (CheckHighQualityAvailable)
                        {
                            CustomLogger.ToConsole("Last Minute Check CheckHighQualityAvailable, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check CheckHighQualityAvailable, Failed", ConsoleColor.Red);
                        }

                        if (WhereIsClownRunAsync)
                        {
                            CustomLogger.ToConsole("Last Minute Check WhereIsClownRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check WhereIsClownRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (LevelSystemRunAsync)
                        {
                            CustomLogger.ToConsole("Last Minute Check LevelSystemRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check LevelSystemRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (LevelSystemRoleDistributionRunAsync)
                        {
                            CustomLogger.ToConsole("Last Minute Check LevelSystemRoleDistributionRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check LevelSystemRoleDistributionRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (SympathySystemRunAsync)
                        {
                            CustomLogger.ToConsole("Last Minute Check SympathySystemRunAsync, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check SympathySystemRunAsync, Failed", ConsoleColor.Red);
                        }

                        if (CheckBirthdayGz)
                        {
                            CustomLogger.ToConsole("Last Minute Check CheckBirthdayGz, Success", ConsoleColor.Green);
                        }
                        else
                        {
                            CustomLogger.ToConsole("Last Minute Check CheckBirthdayGz, Waiting", ConsoleColor.Green);
                        }
                    }
                }
            });
        }
    }
}