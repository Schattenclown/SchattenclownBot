using System;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.AsyncFunction;

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
						CWLogger.Write("Last Minute Check, Success", "BotTimerRunAsync", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "BotTimerRunAsync", ConsoleColor.Red);

					if (BotAlarmClockRunAsync)
						CWLogger.Write("Last Minute Check, Success", "BotAlarmClockRunAsync", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "BotAlarmClockRunAsync", ConsoleColor.Red);

					if (CheckGreenTask)
						CWLogger.Write("Last Minute Check, Success", "CheckGreenTask", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "CheckGreenTask", ConsoleColor.Red);

					if (CheckHighQualityAvailable)
						CWLogger.Write("Last Minute Check, Success", "CheckHighQualityAvailable", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "CheckHighQualityAvailable", ConsoleColor.Red);

					if (WhereIsClownRunAsync)
						CWLogger.Write("Last Minute Check, Success", "WhereIsClownRunAsync", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "WhereIsClownRunAsync", ConsoleColor.Red);

					if (LevelSystemRunAsync)
						CWLogger.Write("Last Minute Check, Success", "LevelSystemRunAsync", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "LevelSystemRunAsync", ConsoleColor.Red);

					if (LevelSystemRoleDistributionRunAsync)
						CWLogger.Write("Last Minute Check, Success", "LevelSystemRoleDistributionRunAsync", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "LevelSystemRoleDistributionRunAsync", ConsoleColor.Red);

					if (SympathySystemRunAsync)
						CWLogger.Write("Last Minute Check, Success", "SympathySystemRunAsync", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "SympathySystemRunAsync", ConsoleColor.Red);

					if (CheckBirthdayGz)
						CWLogger.Write("Last Minute Check, Success", "CheckBirthdayGz", ConsoleColor.Green);
					else
						CWLogger.Write("Last Minute Check, Failed", "CheckBirthdayGz", ConsoleColor.Yellow);
				}
			}
		});
	}
}
