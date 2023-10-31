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
            ConsoleLogger.WriteLine("Starting LastMinuteCheck...");
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
                            ConsoleLogger.WriteLine("Last Minute Check BotTimerRunAsync, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check BotTimerRunAsync, Failed", true);
                        }

                        if (BotAlarmClockRunAsync)
                        {
                            ConsoleLogger.WriteLine("Last Minute Check BotAlarmClockRunAsync, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check BotAlarmClockRunAsync, Failed", true);
                        }

                        if (CheckGreenTask)
                        {
                            ConsoleLogger.WriteLine("Last Minute Check CheckGreenTask, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check CheckGreenTask, Failed", true);
                        }

                        if (CheckHighQualityAvailable)
                        {
                            ConsoleLogger.WriteLine("Last Minute Check CheckHighQualityAvailable, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check CheckHighQualityAvailable, Failed", true);
                        }

                        if (WhereIsClownRunAsync)
                        {
                            ConsoleLogger.WriteLine("Last Minute Check WhereIsClownRunAsync, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check WhereIsClownRunAsync, Failed", true);
                        }

                        if (LevelSystemRunAsync)
                        {
                            ConsoleLogger.WriteLine("Last Minute Check LevelSystemRunAsync, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check LevelSystemRunAsync, Failed", true);
                        }

                        if (LevelSystemRoleDistributionRunAsync)
                        {
                            ConsoleLogger.WriteLine("Last Minute Check LevelSystemRoleDistributionRunAsync, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check LevelSystemRoleDistributionRunAsync, Failed", true);
                        }

                        if (SympathySystemRunAsync)
                        {
                            ConsoleLogger.WriteLine("Last Minute Check SympathySystemRunAsync, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check SympathySystemRunAsync, Failed", true);
                        }

                        if (CheckBirthdayGz)
                        {
                            ConsoleLogger.WriteLine("Last Minute Check CheckBirthdayGz, Success");
                        }
                        else
                        {
                            ConsoleLogger.WriteLine("Last Minute Check CheckBirthdayGz, Waiting");
                        }
                    }
                }
            });
        }
    }
}