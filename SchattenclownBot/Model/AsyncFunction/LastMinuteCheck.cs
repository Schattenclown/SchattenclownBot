using DisCatSharp;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.AsyncFunction
{
   internal class LastMinuteCheck
   {
      public static bool BotTimerRunAsync = false;
      public static bool BotAlarmClockRunAsync = false;
      public static bool CheckGreenTask = false;
      public static bool CheckHighQualityAvailable = false;
      public static bool WhereIsClownRunAsync = false;
      public static bool LevelSystemRunAsync = false;
      public static bool LevelSystemRoleDistributionRunAsync = false;
      public static bool CheckBirthdayGz = false;
      public static bool SympathySystemRunAsync = false;

      public static async Task Check(int executeSecond)
      {
         await Task.Run(async () =>
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
                     CWLogger.Write("Last Minute Check", "BotTimerRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "BotTimerRunAsync", ConsoleColor.Red);

                  if (BotAlarmClockRunAsync)
                     CWLogger.Write("Last Minute Check", "BotAlarmClockRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "BotAlarmClockRunAsync", ConsoleColor.Red);

                  if (CheckGreenTask)
                     CWLogger.Write("Last Minute Check", "CheckGreenTask", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "CheckGreenTask", ConsoleColor.Red);

                  if (CheckHighQualityAvailable)
                     CWLogger.Write("Last Minute Check", "CheckHighQualityAvailable", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "CheckHighQualityAvailable", ConsoleColor.Red);

                  if (WhereIsClownRunAsync)
                     CWLogger.Write("Last Minute Check", "WhereIsClownRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "WhereIsClownRunAsync", ConsoleColor.Red);

                  if (LevelSystemRunAsync)
                     CWLogger.Write("Last Minute Check", "LevelSystemRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "LevelSystemRunAsync", ConsoleColor.Red);

                  if (LevelSystemRoleDistributionRunAsync)
                     CWLogger.Write("Last Minute Check", "LevelSystemRoleDistributionRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "LevelSystemRoleDistributionRunAsync", ConsoleColor.Red);

                  if (SympathySystemRunAsync)
                     CWLogger.Write("Last Minute Check", "SympathySystemRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "SympathySystemRunAsync", ConsoleColor.Red);

                  if (CheckBirthdayGz)
                     CWLogger.Write("Last Minute Check", "CheckBirthdayGz", ConsoleColor.Green);
                  else
                     CWLogger.Write("Last Minute Check", "CheckBirthdayGz", ConsoleColor.Red);
               }
            }
         });
      }
   }
}
