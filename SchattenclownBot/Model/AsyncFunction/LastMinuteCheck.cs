using System;
using System.Threading.Tasks;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot.Model.AsyncFunction;

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

   public static void Check(int executeSecond)
   {
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

               await Task.Delay(60 * 1000);

               if (BotTimerRunAsync)
               {
                  CwLogger.Write("Last Minute Check, Success", "BotTimerRunAsync", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "BotTimerRunAsync", ConsoleColor.Red);
               }

               if (BotAlarmClockRunAsync)
               {
                  CwLogger.Write("Last Minute Check, Success", "BotAlarmClockRunAsync", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "BotAlarmClockRunAsync", ConsoleColor.Red);
               }

               if (CheckGreenTask)
               {
                  CwLogger.Write("Last Minute Check, Success", "CheckGreenTask", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "CheckGreenTask", ConsoleColor.Red);
               }

               if (CheckHighQualityAvailable)
               {
                  CwLogger.Write("Last Minute Check, Success", "CheckHighQualityAvailable", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "CheckHighQualityAvailable", ConsoleColor.Red);
               }

               if (WhereIsClownRunAsync)
               {
                  CwLogger.Write("Last Minute Check, Success", "WhereIsClownRunAsync", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "WhereIsClownRunAsync", ConsoleColor.Red);
               }

               if (LevelSystemRunAsync)
               {
                  CwLogger.Write("Last Minute Check, Success", "LevelSystemRunAsync", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "LevelSystemRunAsync", ConsoleColor.Red);
               }

               if (LevelSystemRoleDistributionRunAsync)
               {
                  CwLogger.Write("Last Minute Check, Success", "LevelSystemRoleDistributionRunAsync", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "LevelSystemRoleDistributionRunAsync", ConsoleColor.Red);
               }

               if (SympathySystemRunAsync)
               {
                  CwLogger.Write("Last Minute Check, Success", "SympathySystemRunAsync", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "SympathySystemRunAsync", ConsoleColor.Red);
               }

               if (CheckBirthdayGz)
               {
                  CwLogger.Write("Last Minute Check, Success", "CheckBirthdayGz", ConsoleColor.Green);
               }
               else
               {
                  CwLogger.Write("Last Minute Check, Failed", "CheckBirthdayGz", ConsoleColor.Yellow);
               }
            }
         }
      });
   }
}