// Copyright (c) Schattenclown

using System;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Objects;

// ReSharper disable UnusedMember.Global

namespace SchattenclownBot.Model.Discord.AppCommands;

internal class UserLevel : ApplicationCommandsModule
{
	/// <summary>
	///     Command to view your connection time Level.
	/// </summary>
	/// <param name="interactionContext">The interactionContext</param>
	/// <returns></returns>
	[SlashCommand("MyLevel" + Bot.IS_DEV_BOT, "Look up your level!")]
	public static async Task MyLevelAsync(InteractionContext interactionContext)
	{
		await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

		var userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);
		var userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
		userLevelSystemListSorted.Reverse();
		int calculatedXpOverCurrentLevel = 0, calculatedXpSpanToReachNextLevel = 0, level = 0;

		var rank = "N/A";

		await Bot.DiscordClient.GetUserAsync(interactionContext.Member.Id);

		foreach (var userLevelSystemItem in userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.MemberId == interactionContext.Member.Id))
		{
			rank = (userLevelSystemListSorted.IndexOf(userLevelSystemItem) + 1).ToString();
			calculatedXpOverCurrentLevel = UserLevelSystem.CalculateXpOverCurrentLevel(userLevelSystemItem.OnlineTicks);
			calculatedXpSpanToReachNextLevel = UserLevelSystem.CalculateXpSpanToReachNextLevel(userLevelSystemItem.OnlineTicks);
			level = UserLevelSystem.CalculateLevel(userLevelSystemItem.OnlineTicks);
			break;
		}

		var xpString = $"{calculatedXpOverCurrentLevel} / {calculatedXpSpanToReachNextLevel} XP ";

		var levelString = $"Level {level}";

		var xpPadLeft = xpString.PadLeft(11, ' ');

		var levelPadLeft = levelString.PadLeft(8, ' ');

		var temp = xpPadLeft + levelPadLeft;

		#region editLink

		//https://quickchart.io/sandbox/#%7B%22chart%22%3A%22%7B%5Cn%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%5C%22data%5C%22%3A%20%7B%5Cn%20%20%20%20%5C%22datasets%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22barPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22categoryPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22data%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%20%202500%5Cn%20%20%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22label%5C%22%3A%20%5C%22XP%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22rgba(80%2C%200%2C%20121%2C%200.4)%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderWidth%5C%22%3A%203%2C%5Cn%20%20%20%20%20%20%20%20%5C%22xAxisID%5C%22%3A%20%5C%22X1%5C%22%2C%5Cn%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%5Cn%20%20%20%20%20%20%20%20%5C%22barPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22categoryPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22data%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%20%205641%5Cn%20%20%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22label%5C%22%3A%20%5C%22XPNeeded%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22rgba(58%2C%200%2C%20179%2C%200.2)%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderWidth%5C%22%3A%203%2C%5Cn%20%20%20%20%20%20%20%20%5C%22xAxisID%5C%22%3A%20%5C%22X1%5C%22%5Cn%20%20%20%20%20%20%7D%5Cn%20%20%20%20%5D%2C%5Cn%20%20%20%20%5C%22labels%5C%22%3A%20%5B%5D%5Cn%20%20%7D%2C%5Cn%20%20%5C%22options%5C%22%3A%20%7B%5Cn%20%20%20%20%5C%22title%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22display%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%5C%22position%5C%22%3A%20%5C%22top%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontSize%5C%22%3A%2025%2C%5Cn%20%20%20%20%20%20%5C%22fontFamily%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontColor%5C%22%3A%20%5C%22%23ff00ff%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontStyle%5C%22%3A%20%5C%22bold%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22padding%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%5C%22lineHeight%5C%22%3A%201.2%2C%5Cn%20%20%20%20%20%20%5C%22text%5C%22%3A%20%5C%22Title%20be%20here%5C%22%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22layout%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22padding%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22left%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22right%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22top%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22bottom%5C%22%3A%2010%5Cn%20%20%20%20%20%20%7D%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22legend%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22display%5C%22%3A%20false%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22scales%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22xAxes%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22id%5C%22%3A%20%5C%22X1%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22display%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22position%5C%22%3A%20%5C%22bottom%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22linear%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22stacked%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22time%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22distribution%5C%22%3A%20%5C%22linear%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22gridLines%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22color%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22lineWidth%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawBorder%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawOnChartArea%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawTicks%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22tickMarkLength%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22zeroLineWidth%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22zeroLineColor%5C%22%3A%20%5C%22%23e100ff%5C%22%5Cn%20%20%20%20%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22ticks%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22display%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontSize%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontFamily%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontColor%5C%22%3A%20%5C%22%23ff00ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontStyle%5C%22%3A%20%5C%22bold%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22padding%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22suggestedMin%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22suggestedMax%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22stepSize%5C%22%3A%201000000%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22minRotation%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22maxRotation%5C%22%3A%2050%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22mirror%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22reverse%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22max%5C%22%3A%205641%5Cn%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%5C%22yAxes%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22stacked%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22time%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22displayFormats%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%5D%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22plugins%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22datalabels%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22anchor%5C%22%3A%20%5C%22end%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22%23e241ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e241ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderRadius%5C%22%3A%206%2C%5Cn%20%20%20%20%20%20%20%20%5C%22padding%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%5C%22color%5C%22%3A%20%5C%22%23282828%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22font%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22family%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22size%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22style%5C%22%3A%20%5C%22normal%5C%22%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%7D%2C%5Cn%20%20%7D%5Cn%7D%22%2C%22width%22%3A1000%2C%22height%22%3A180%2C%22version%22%3A%222%22%2C%22backgroundColor%22%3A%22%232f3136%22%7D

		#endregion

		#region apikey
		var urlString = "https://quickchart.io/chart?w=1000&h=180&bkg=%232f3136&c={\"type\":\"horizontalBar\",\"data\":{\"datasets\":[{\"barPercentage\":1,\"categoryPercentage\":1,\"data\":[" +
					  $"{calculatedXpOverCurrentLevel}" +
					  "],\"type\":\"horizontalBar\",\"label\":\"XP\",\"borderColor\":\"%23e100ff\",\"backgroundColor\":\"rgba(80,0,121,0.4)\",\"borderWidth\":3,\"xAxisID\":\"X1\",},{\"barPercentage\":1,\"categoryPercentage\":1,\"data\":[" +
					  $"{calculatedXpSpanToReachNextLevel}" +
					  "],\"type\":\"horizontalBar\",\"label\":\"XPNeeded\",\"borderColor\":\"%23e100ff\",\"backgroundColor\":\"rgba(58,0,179,0.2)\",\"borderWidth\":3,\"xAxisID\":\"X1\"}],\"labels\":[]},\"options\":{\"title\":{\"display\":false,\"position\":\"top\",\"fontSize\":25,\"fontFamily\":\"sans-serif\",\"fontColor\":\"%23ff00ff\",\"fontStyle\":\"bold\",\"padding\":10,\"lineHeight\":1.2,\"text\":\"Titlebehere\"},\"layout\":{\"padding\":{\"left\":30,\"right\":30,\"top\":30,\"bottom\":10}},\"legend\":{\"display\":false},\"scales\":{\"xAxes\":[{\"id\":\"X1\",\"display\":true,\"position\":\"bottom\",\"type\":\"linear\",\"stacked\":false,\"time\":{},\"distribution\":\"linear\",\"gridLines\":{\"color\":\"%23e100ff\",\"lineWidth\":4,\"drawBorder\":true,\"drawOnChartArea\":true,\"drawTicks\":true,\"tickMarkLength\":10,\"zeroLineWidth\":4,\"zeroLineColor\":\"%23e100ff\"},\"ticks\":{\"display\":true,\"fontSize\":30,\"fontFamily\":\"sans-serif\",\"fontColor\":\"%23ff00ff\",\"fontStyle\":\"bold\",\"padding\":0,\"suggestedMin\":0,\"suggestedMax\":0,\"stepSize\":1000000,\"minRotation\":0,\"maxRotation\":50,\"mirror\":false,\"reverse\":false,\"max\":" +
					  $"{calculatedXpSpanToReachNextLevel}" +
					  "}}],\"yAxes\":[{\"stacked\":true,\"time\":{\"displayFormats\":{}}}]},\"plugins\":{\"datalabels\":{\"anchor\":\"end\",\"backgroundColor\":\"%23e241ff\",\"borderColor\":\"%23e241ff\",\"borderRadius\":6,\"padding\":4,\"color\":\"%23282828\",\"font\":{\"family\":\"sans-serif\",\"size\":30,\"style\":\"normal\"}},},}}";
		#endregion

		DiscordEmbedBuilder discordEmbedBuilder = new();
		discordEmbedBuilder.WithTitle(temp);
		discordEmbedBuilder.WithDescription("<@" + interactionContext.Member.Id + ">");
		discordEmbedBuilder.WithFooter("Rank #" + rank);
		discordEmbedBuilder.WithImageUrl(urlString);
		discordEmbedBuilder.Color = DiscordColor.Purple;

		await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
	}

	/// <summary>
	///     Command to view ur connection time Level.
	/// </summary>
	/// <param name="interactionContext">The interactionContext</param>
	/// <param name="discordUser"></param>
	/// <returns></returns>
	[SlashCommand("Level" + Bot.IS_DEV_BOT, "Look up someones level!")]
	public static async Task LevelAsync(InteractionContext interactionContext, [Option("User", "@...")] DiscordUser discordUser)
	{
		await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

		var userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);
		var userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
		userLevelSystemListSorted.Reverse();
		int calculatedXpOverCurrentLevel = 0, calculatedXpSpanToReachNextLevel = 0, level = 0;

		var rank = "N/A";

		foreach (var userLevelSystemItem in userLevelSystemListSorted.Where(userLevelSystemItem => userLevelSystemItem.MemberId == discordUser.Id))
		{
			rank = (userLevelSystemListSorted.IndexOf(userLevelSystemItem) + 1).ToString();
			calculatedXpOverCurrentLevel = UserLevelSystem.CalculateXpOverCurrentLevel(userLevelSystemItem.OnlineTicks);
			calculatedXpSpanToReachNextLevel = UserLevelSystem.CalculateXpSpanToReachNextLevel(userLevelSystemItem.OnlineTicks);
			level = UserLevelSystem.CalculateLevel(userLevelSystemItem.OnlineTicks);
			break;
		}

		var xpString = $"{calculatedXpOverCurrentLevel} / {calculatedXpSpanToReachNextLevel} XP ";

		var levelString = $"Level {level}";

		var xpPadLeft = xpString.PadLeft(11, ' ');

		var levelPadLeft = levelString.PadLeft(8, ' ');

		var temp = xpPadLeft + levelPadLeft;

		#region editLink

		//https://quickchart.io/sandbox/#%7B%22chart%22%3A%22%7B%5Cn%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%5C%22data%5C%22%3A%20%7B%5Cn%20%20%20%20%5C%22datasets%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22barPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22categoryPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22data%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%20%202500%5Cn%20%20%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22label%5C%22%3A%20%5C%22XP%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22rgba(80%2C%200%2C%20121%2C%200.4)%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderWidth%5C%22%3A%203%2C%5Cn%20%20%20%20%20%20%20%20%5C%22xAxisID%5C%22%3A%20%5C%22X1%5C%22%2C%5Cn%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%5Cn%20%20%20%20%20%20%20%20%5C%22barPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22categoryPercentage%5C%22%3A%201%2C%5Cn%20%20%20%20%20%20%20%20%5C%22data%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%20%205641%5Cn%20%20%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22horizontalBar%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22label%5C%22%3A%20%5C%22XPNeeded%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22rgba(58%2C%200%2C%20179%2C%200.2)%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderWidth%5C%22%3A%203%2C%5Cn%20%20%20%20%20%20%20%20%5C%22xAxisID%5C%22%3A%20%5C%22X1%5C%22%5Cn%20%20%20%20%20%20%7D%5Cn%20%20%20%20%5D%2C%5Cn%20%20%20%20%5C%22labels%5C%22%3A%20%5B%5D%5Cn%20%20%7D%2C%5Cn%20%20%5C%22options%5C%22%3A%20%7B%5Cn%20%20%20%20%5C%22title%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22display%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%5C%22position%5C%22%3A%20%5C%22top%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontSize%5C%22%3A%2025%2C%5Cn%20%20%20%20%20%20%5C%22fontFamily%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontColor%5C%22%3A%20%5C%22%23ff00ff%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22fontStyle%5C%22%3A%20%5C%22bold%5C%22%2C%5Cn%20%20%20%20%20%20%5C%22padding%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%5C%22lineHeight%5C%22%3A%201.2%2C%5Cn%20%20%20%20%20%20%5C%22text%5C%22%3A%20%5C%22Title%20be%20here%5C%22%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22layout%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22padding%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22left%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22right%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22top%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%5C%22bottom%5C%22%3A%2010%5Cn%20%20%20%20%20%20%7D%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22legend%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22display%5C%22%3A%20false%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22scales%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22xAxes%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22id%5C%22%3A%20%5C%22X1%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22display%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22position%5C%22%3A%20%5C%22bottom%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22type%5C%22%3A%20%5C%22linear%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22stacked%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22time%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22distribution%5C%22%3A%20%5C%22linear%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22gridLines%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22color%5C%22%3A%20%5C%22%23e100ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22lineWidth%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawBorder%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawOnChartArea%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22drawTicks%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22tickMarkLength%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22zeroLineWidth%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22zeroLineColor%5C%22%3A%20%5C%22%23e100ff%5C%22%5Cn%20%20%20%20%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22ticks%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22display%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontSize%5C%22%3A%2030%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontFamily%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontColor%5C%22%3A%20%5C%22%23ff00ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22fontStyle%5C%22%3A%20%5C%22bold%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22padding%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22suggestedMin%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22suggestedMax%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22stepSize%5C%22%3A%201000000%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22minRotation%5C%22%3A%200%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22maxRotation%5C%22%3A%2050%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22mirror%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22reverse%5C%22%3A%20false%2C%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22max%5C%22%3A%205641%5Cn%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%5D%2C%5Cn%20%20%20%20%20%20%5C%22yAxes%5C%22%3A%20%5B%5Cn%20%20%20%20%20%20%20%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22stacked%5C%22%3A%20true%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22time%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%5C%22displayFormats%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%5D%5Cn%20%20%20%20%7D%2C%5Cn%20%20%20%20%5C%22plugins%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%5C%22datalabels%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%5C%22anchor%5C%22%3A%20%5C%22end%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22backgroundColor%5C%22%3A%20%5C%22%23e241ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderColor%5C%22%3A%20%5C%22%23e241ff%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22borderRadius%5C%22%3A%206%2C%5Cn%20%20%20%20%20%20%20%20%5C%22padding%5C%22%3A%204%2C%5Cn%20%20%20%20%20%20%20%20%5C%22color%5C%22%3A%20%5C%22%23282828%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%5C%22font%5C%22%3A%20%7B%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22family%5C%22%3A%20%5C%22sans-serif%5C%22%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22size%5C%22%3A%2010%2C%5Cn%20%20%20%20%20%20%20%20%20%20%5C%22style%5C%22%3A%20%5C%22normal%5C%22%5Cn%20%20%20%20%20%20%20%20%7D%5Cn%20%20%20%20%20%20%7D%2C%5Cn%20%20%20%20%7D%2C%5Cn%20%20%7D%5Cn%7D%22%2C%22width%22%3A1000%2C%22height%22%3A180%2C%22version%22%3A%222%22%2C%22backgroundColor%22%3A%22%232f3136%22%7D

		#endregion

		#region apikey
		var urlString = "https://quickchart.io/chart?w=1000&h=180&bkg=%232f3136&c={\"type\":\"horizontalBar\",\"data\":{\"datasets\":[{\"barPercentage\":1,\"categoryPercentage\":1,\"data\":[" +
					  $"{calculatedXpOverCurrentLevel}" +
					  "],\"type\":\"horizontalBar\",\"label\":\"XP\",\"borderColor\":\"%23e100ff\",\"backgroundColor\":\"rgba(80,0,121,0.4)\",\"borderWidth\":3,\"xAxisID\":\"X1\",},{\"barPercentage\":1,\"categoryPercentage\":1,\"data\":[" +
					  $"{calculatedXpSpanToReachNextLevel}" +
					  "],\"type\":\"horizontalBar\",\"label\":\"XPNeeded\",\"borderColor\":\"%23e100ff\",\"backgroundColor\":\"rgba(58,0,179,0.2)\",\"borderWidth\":3,\"xAxisID\":\"X1\"}],\"labels\":[]},\"options\":{\"title\":{\"display\":false,\"position\":\"top\",\"fontSize\":25,\"fontFamily\":\"sans-serif\",\"fontColor\":\"%23ff00ff\",\"fontStyle\":\"bold\",\"padding\":10,\"lineHeight\":1.2,\"text\":\"Titlebehere\"},\"layout\":{\"padding\":{\"left\":30,\"right\":30,\"top\":30,\"bottom\":10}},\"legend\":{\"display\":false},\"scales\":{\"xAxes\":[{\"id\":\"X1\",\"display\":true,\"position\":\"bottom\",\"type\":\"linear\",\"stacked\":false,\"time\":{},\"distribution\":\"linear\",\"gridLines\":{\"color\":\"%23e100ff\",\"lineWidth\":4,\"drawBorder\":true,\"drawOnChartArea\":true,\"drawTicks\":true,\"tickMarkLength\":10,\"zeroLineWidth\":4,\"zeroLineColor\":\"%23e100ff\"},\"ticks\":{\"display\":true,\"fontSize\":30,\"fontFamily\":\"sans-serif\",\"fontColor\":\"%23ff00ff\",\"fontStyle\":\"bold\",\"padding\":0,\"suggestedMin\":0,\"suggestedMax\":0,\"stepSize\":1000000,\"minRotation\":0,\"maxRotation\":50,\"mirror\":false,\"reverse\":false,\"max\":" +
					  $"{calculatedXpSpanToReachNextLevel}" +
					  "}}],\"yAxes\":[{\"stacked\":true,\"time\":{\"displayFormats\":{}}}]},\"plugins\":{\"datalabels\":{\"anchor\":\"end\",\"backgroundColor\":\"%23e241ff\",\"borderColor\":\"%23e241ff\",\"borderRadius\":6,\"padding\":4,\"color\":\"%23282828\",\"font\":{\"family\":\"sans-serif\",\"size\":30,\"style\":\"normal\"}},},}}";
		#endregion

		DiscordEmbedBuilder discordEmbedBuilder = new();
		discordEmbedBuilder.WithTitle(temp);
		discordEmbedBuilder.WithDescription("<@" + discordUser.Id + ">");
		discordEmbedBuilder.WithFooter("Rank #" + rank);
		discordEmbedBuilder.WithImageUrl(urlString);
		discordEmbedBuilder.Color = DiscordColor.Purple;

		await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbedBuilder.Build()));
	}

	/// <summary>
	///     Shows the leaderboard.
	/// </summary>
	/// <param name="interactionContext"></param>
	/// <returns></returns>
	[SlashCommand("Leaderboard" + Bot.IS_DEV_BOT, "Look up the leaderboard for connection time!")]
	public static async Task LeaderboardAsync(InteractionContext interactionContext)
	{
		//Create an Response.
		await interactionContext.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

		DiscordMember discordMember = null;

		//Create List where all users are listed.
		var userLevelSystemList = UserLevelSystem.Read(interactionContext.Guild.Id);

		//Order the list by online ticks.
		var userLevelSystemListSorted = userLevelSystemList.OrderBy(x => x.OnlineTicks).ToList();
		userLevelSystemListSorted.Reverse();

		var top30 = 0;
		var leaderboardString = "```css\n";

		var discordMemberList = Bot.DiscordClient.GetGuildAsync(interactionContext.Guild.Id).Result.Members.Values.ToList();

		//Create the Leaderboard string
		foreach (var userLevelSystemItem in userLevelSystemListSorted)
		{
			foreach (var discordMemberItem in discordMemberList.Where(discordMemberItem => discordMemberItem.Id == userLevelSystemItem.MemberId))
			{
				discordMember = discordMemberItem;
			}

			if (discordMember != null)
			{
				DateTime date1 = new(1969, 4, 20, 4, 20, 0);
				var date2 = new DateTime(1969, 4, 20, 4, 20, 0).AddMinutes(userLevelSystemItem.OnlineTicks);
				var timeSpan = date2 - date1;

				var calculatedLevel = UserLevelSystem.CalculateLevel(userLevelSystemItem.OnlineTicks);

				var daysString = "Days";
				if (Convert.ToInt32($"{timeSpan:ddd}") == 1)
					daysString = "Day ";

				leaderboardString += "{" + $"{Convert.ToInt32($"{timeSpan:ddd}"),3} {daysString} {timeSpan:hh}:{timeSpan:mm}" + "}" + $" Level {calculatedLevel,2} [{discordMember.DisplayName.Replace('\'', ' ')}]\n";
				top30++;
				if (top30 == 30)
					break;

				discordMember = null;
			}
		}

		leaderboardString += "\n```";

		await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().WithContent(leaderboardString));
	}
}
