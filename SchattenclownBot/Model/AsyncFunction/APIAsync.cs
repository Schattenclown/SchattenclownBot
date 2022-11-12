using DisCatSharp.Entities;

using SchattenclownBot.Model.Discord.AppCommands;
using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Objects;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.AsyncFunction;

internal class APIAsync
{
	public static async Task ReadFromAPIAsync()
	{
		await Task.Run(async () =>
		{
			List<DiscordGuild> guildList;
			do
			{
				guildList = Bot.DiscordClient.Guilds.Values.ToList();
				await Task.Delay(1000);
			} while (guildList.Count == 0);

			while (true)
			{
				List<API> aPI_Objects = API.GET();
				foreach (var item in aPI_Objects)
				{
					switch (item.Command)
					{
						case "Next_Song":
							PlayMusic.NextRequestAPI(item);
							break;
						case "RequestUserName":
							RequestUserNameAwnser(item);
							break;
						default:
							break;
					}

				}
				await Task.Delay(100);
			}
		});
	}
	public static async void RequestUserNameAwnser(API aPI)
	{
		API.DELETE(aPI.CommandRequestID);
		DiscordUser discordUser = await Bot.DiscordClient.GetUserAsync(aPI.RequestDiscordUserId);
		API aPIresult = aPI;
		aPIresult.Data = discordUser.Username;
		aPIresult.Command = "RequestUserNameAwnser";
		API.PUT(aPIresult);
	}
}
