// Copyright (c) Schattenclown

using System;
using System.Threading.Tasks;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot;

/// <summary>
/// The program boot class.
/// </summary>
internal class Program
{
	private static Bot s_bot;
	/// <summary>
	/// the boot task
	/// </summary>
	/// <returns>Nothing</returns>
	private static async Task Main()
	{
		#region ConsoleSize
		try
		{
#pragma warning disable CA1416
			Console.SetWindowSize(300, 30);
		}
		catch (Exception)
		{
			Console.SetWindowSize(100, 10);
#pragma warning restore CA1416
		}
		#endregion
		await Task.Run(async () =>
		{
			try
			{
				s_bot = new Bot();
				s_bot.RunAsync().Wait();
				await Task.Delay(1000);
			}
			catch
			{
				Reset.RestartProgram();
			}
		});
	}
}
