// Copyright (c) Schattenclown

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DisCatSharp.Entities;

using SchattenclownBot.Model.Discord.Main;

namespace SchattenclownBot.Model.Objects;

public class BirthdayList
{
	public static async Task GenerateBirthdayList()
	{
		var guildsList = Bot.DiscordClient.Guilds.ToList();
		List<KeyValuePair<ulong, DateTime>> birthdays = new();

		var levelSystemVirgin = true;
		do
		{
			if (Bot.DiscordClient.Guilds.ToList().Count != 0)
			{
				guildsList = Bot.DiscordClient.Guilds.ToList();

				levelSystemVirgin = false;
			}
			await Task.Delay(1000);
		} while (levelSystemVirgin);

		foreach (var guildItem in guildsList)
		{
			if (guildItem.Value.Id == 928930967140331590)
			{
				var discordGuildObj = Bot.DiscordClient.GetGuildAsync(guildItem.Value.Id).Result;
				var discordMembers = discordGuildObj.Members;
				foreach (var discordMemberItem in discordMembers)
				{
					var birthday = GenerateDateTime(discordMemberItem.Value);
					DateTime dateTimeComparer = new(9999, 9, 9);
					if (birthday != dateTimeComparer)
						birthdays.Add(new KeyValuePair<ulong, DateTime>(discordMemberItem.Value.Id, birthday));
				}
				break;
			}
		}

		if (birthdays.Count != 0)
			birthdays.Sort((ps1, ps2) => DateTime.Compare(ps1.Value.Date, ps2.Value.Date));

		var listString = "";

		foreach (var item in birthdays)
		{
			listString += $"``{item.Value.Day:00}.{item.Value.Month:00} -`` <@{item.Key}>\n";
		}

		foreach (var guildItem in guildsList)
		{
			if (guildItem.Value.Id == 928930967140331590)
			{
				var chn = await Bot.DiscordClient.GetChannelAsync(928938948221366334);
				DiscordEmbedBuilder eb = new()
				{
					Color = DiscordColor.Red
				};
				eb.WithDescription(listString);
				await chn.SendMessageAsync(eb.Build());
				break;
			}
		}
	}

	public static async Task CheckBirthdayGz(int executeSecond)
	{
		await Task.Run(async () =>
		{
			while (DateTime.Now.Second != executeSecond)
			{
				await Task.Delay(1000);
			}

			List<DiscordGuild> guildList;
			do
			{
				guildList = Bot.DiscordClient.Guilds.Values.ToList();
				await Task.Delay(1000);
			} while (guildList.Count == 0);

			while (true)
			{

				while (DateTime.Now.Second != executeSecond || DateTime.Now.Hour != 23 || DateTime.Now.Minute != 59)
				{
					await Task.Delay(1000);
				}

				foreach (var discordGuild in guildList.Where(x => x.Id == 928930967140331590))
				{
					var discordGuildObj = Bot.DiscordClient.GetGuildAsync(discordGuild.Id).Result;
					var discordMembers = discordGuildObj.Members;
					foreach (var discordMemberItem in discordMembers)
					{
						var birthday = GenerateDateTime(discordMemberItem.Value);
						if (DateTime.Now.Day == birthday.Day && DateTime.Now.Month == birthday.Month)
						{
							var discordChannel = discordGuildObj.GetChannel(928958632421363732);
							await discordChannel.SendMessageAsync($"{discordMemberItem.Value.Mention} Heute Burtseltag gehabt");
						}
					}
				}
				await Task.Delay(1000);
				if (!AsyncFunction.LastMinuteCheck.CheckBirthdayGz)
					AsyncFunction.LastMinuteCheck.CheckBirthdayGz = true;
			}
		});
	}
	public static DateTime GenerateDateTime(DiscordMember discordMember)
	{
		var birthdayDay = "";
		var birthdayMonth = 0;

		var discordMemberRoleList = discordMember.Roles.ToList();

		var discordGuildObj = Bot.DiscordClient.GetGuildAsync(discordMember.Guild.Id).Result;

		var zehner0 = discordGuildObj.GetRole(945301296330723348);
		var zehner1 = discordGuildObj.GetRole(945301296993427517);
		var zehner2 = discordGuildObj.GetRole(945301298012647444);
		var zehner3 = discordGuildObj.GetRole(945301298704683048);

		if (discordMemberRoleList.Contains(zehner0))
			birthdayDay += "0";
		if (discordMemberRoleList.Contains(zehner1))
			birthdayDay += "1";
		if (discordMemberRoleList.Contains(zehner2))
			birthdayDay += "2";
		if (discordMemberRoleList.Contains(zehner3))
			birthdayDay += "3";

		var einer0 = discordGuildObj.GetRole(945301303649787904);
		var einer1 = discordGuildObj.GetRole(945301749437177856);
		var einer2 = discordGuildObj.GetRole(945301753035903036);
		var einer3 = discordGuildObj.GetRole(945302992507252736);
		var einer4 = discordGuildObj.GetRole(945302996630249532);
		var einer5 = discordGuildObj.GetRole(945302999604002836);
		var einer6 = discordGuildObj.GetRole(945303002917535845);
		var einer7 = discordGuildObj.GetRole(945309817252237332);
		var einer8 = discordGuildObj.GetRole(945309808096063498);
		var einer9 = discordGuildObj.GetRole(945309830934044683);

		if (discordMemberRoleList.Contains(einer0))
			birthdayDay += "0";
		if (discordMemberRoleList.Contains(einer1))
			birthdayDay += "1";
		if (discordMemberRoleList.Contains(einer2))
			birthdayDay += "2";
		if (discordMemberRoleList.Contains(einer3))
			birthdayDay += "3";
		if (discordMemberRoleList.Contains(einer4))
			birthdayDay += "4";
		if (discordMemberRoleList.Contains(einer5))
			birthdayDay += "5";
		if (discordMemberRoleList.Contains(einer6))
			birthdayDay += "6";
		if (discordMemberRoleList.Contains(einer7))
			birthdayDay += "7";
		if (discordMemberRoleList.Contains(einer8))
			birthdayDay += "8";
		if (discordMemberRoleList.Contains(einer9))
			birthdayDay += "9";

		var month1 = discordGuildObj.GetRole(945301816285990983);
		var month2 = discordGuildObj.GetRole(945301818559324211);
		var month3 = discordGuildObj.GetRole(945301830152372244);
		var month4 = discordGuildObj.GetRole(945309841403043870);
		var month5 = discordGuildObj.GetRole(945309844200624139);
		var month6 = discordGuildObj.GetRole(945309847048577114);
		var month7 = discordGuildObj.GetRole(945309847467999243);
		var month8 = discordGuildObj.GetRole(945309848273297458);
		var month9 = discordGuildObj.GetRole(945309848864718891);
		var month10 = discordGuildObj.GetRole(945329632205475900);
		var month11 = discordGuildObj.GetRole(945309850273988648);
		var month12 = discordGuildObj.GetRole(945310176163004476);

		if (discordMemberRoleList.Contains(month1))
			birthdayMonth = 1;
		if (discordMemberRoleList.Contains(month2))
			birthdayMonth = 2;
		if (discordMemberRoleList.Contains(month3))
			birthdayMonth = 3;
		if (discordMemberRoleList.Contains(month4))
			birthdayMonth = 4;
		if (discordMemberRoleList.Contains(month5))
			birthdayMonth = 5;
		if (discordMemberRoleList.Contains(month6))
			birthdayMonth = 6;
		if (discordMemberRoleList.Contains(month7))
			birthdayMonth = 7;
		if (discordMemberRoleList.Contains(month8))
			birthdayMonth = 8;
		if (discordMemberRoleList.Contains(month9))
			birthdayMonth = 9;
		if (discordMemberRoleList.Contains(month10))
			birthdayMonth = 10;
		if (discordMemberRoleList.Contains(month11))
			birthdayMonth = 11;
		if (discordMemberRoleList.Contains(month12))
			birthdayMonth = 12;

		DateTime dateTime = new(9999, 9, 9);
		int birthdayDayInt;

		try
		{
			if (birthdayDay != "" && birthdayDay != "01230123456789")
				birthdayDayInt = Convert.ToInt32(birthdayDay);
			else
				return dateTime;

		}
		catch
		{
			return dateTime;
		}


		if (birthdayMonth is >= 1 and <= 12 && birthdayDayInt is >= 1 and <= 31)
		{
			try
			{
				dateTime = new DateTime(1, birthdayMonth, birthdayDayInt);
				return dateTime;
			}
			catch
			{
				return dateTime;
			}
		}

		return dateTime;
	}
}
