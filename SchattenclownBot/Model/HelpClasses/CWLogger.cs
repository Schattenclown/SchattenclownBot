// Copyright (c) Schattenclown

using System;

using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot;

internal class CWLogger
{
	public static void Write(string writeLineString, string callerFunction, ConsoleColor color)
	{
		if (writeLineString.Contains("CREATE TABLE IF NOT EXISTS"))
		{
			writeLineString = StringCutter.RemoveAfterWord(writeLineString, "` (`", 0);
			writeLineString = StringCutter.RemoveAfterWord(writeLineString, " (`", 0);
			writeLineString = StringCutter.RemoveUntilWord(writeLineString, "CREATE TABLE IF NOT EXISTS `", "CREATE TABLE IF NOT EXISTS `".Length);
		}

		if (callerFunction.Contains("<<") || callerFunction.Contains(">b__0>d"))
		{
			callerFunction = StringCutter.RemoveUntilWord(callerFunction, "<<", "<<".Length);
			callerFunction = StringCutter.RemoveAfterWord(callerFunction, ">b__0>d", 0);
			color = ConsoleColor.Cyan;
		}

		Console.ForegroundColor = ConsoleColor.Gray;
		Console.Write($"[{DateTime.Now} +02:00] [69  /{"Info",-12}]");
		Console.ForegroundColor = color;
		Console.Write($" [{callerFunction}] ");
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine($"{writeLineString}");
	}

	public static void Write(Exception ex, string callerFunction, ConsoleColor color)
	{
		if (callerFunction.Contains("<<") || callerFunction.Contains(">b__0>d"))
		{
			callerFunction = StringCutter.RemoveUntilWord(callerFunction, "<<", "<<".Length);
			callerFunction = StringCutter.RemoveAfterWord(callerFunction, ">b__0>d", 0);
			color = ConsoleColor.Cyan;
		}
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.Write($"[{DateTime.Now} +02:00] [420 /{"Exception",-12}]");
		Console.ForegroundColor = color;
		Console.Write($" [{callerFunction}] ");
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine($"{ex.Message}");
	}
}
