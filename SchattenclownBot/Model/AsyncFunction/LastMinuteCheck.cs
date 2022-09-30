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
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "BotTimerRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "BotTimerRunAsync", ConsoleColor.Red);

                  if (BotAlarmClockRunAsync)
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "BotAlarmClockRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "BotAlarmClockRunAsync", ConsoleColor.Red);

                  if (CheckGreenTask)
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "CheckGreenTask", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "CheckGreenTask", ConsoleColor.Red);

                  if (CheckHighQualityAvailable)
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "CheckHighQualityAvailable", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "CheckHighQualityAvailable", ConsoleColor.Red);

                  if (WhereIsClownRunAsync)
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "WhereIsClownRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "WhereIsClownRunAsync", ConsoleColor.Red);

                  if (LevelSystemRunAsync)
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "LevelSystemRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "LevelSystemRunAsync", ConsoleColor.Red);

                  if (LevelSystemRoleDistributionRunAsync)
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "LevelSystemRoleDistributionRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "LevelSystemRoleDistributionRunAsync", ConsoleColor.Red);

                  if (SympathySystemRunAsync)
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "SympathySystemRunAsync", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "SympathySystemRunAsync", ConsoleColor.Red);

                  if (CheckBirthdayGz)
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "CheckBirthdayGz", ConsoleColor.Green);
                  else
                     CWLogger.Write(MethodBase.GetCurrentMethod()?.DeclaringType?.Name, "CheckBirthdayGz", ConsoleColor.Red);
               }
            }
         });
      }
   }
}
