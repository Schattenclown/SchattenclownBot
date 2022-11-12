// Copyright (c) Schattenclown

using System;
using System.IO;

using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.Persistence.Connection;

public class CsvConnections
{
	private static readonly Uri s_path = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SchattenclownBot");
	private static readonly Uri s_filepath = new($"{s_path}/Connections.csv");
	public static Connections ReadAll()
	{
		try
		{
			Connections connections = new();
			StreamReader streamReader = new(s_filepath.LocalPath);
			while (!streamReader.EndOfStream)
			{
				var row = streamReader.ReadLine();
				if (row != null)
				{
					var infos = row.Split(';');

					switch (infos[0])
					{
						case "DiscordBotKey":
							connections.DiscordBotKey = infos[1];
							break;
						case "DiscordBotKeyDebug":
							connections.DiscordBotDebug = infos[1];
							break;
						case "MySqlConStr":
							connections.MySqlConStr = infos[1].Replace(',', ';');
							break;
						case "MySqlConStrDebug":
							connections.MySqlConStrDebug = infos[1].Replace(',', ';');
							break;
						case "MySqlAPIConStr":
							connections.MySqlAPIConStr = infos[1].Replace(',', ';');
							break;
						case "AcoustIdApiKey":
							connections.AcoustIdApiKey = infos[1];
							break;
						case "SpotifyOAuth2":
							connections.Token = new Connections.SpotifyOAuth2();
							var spotifyOAuth2 = infos[1].Split('-');
							connections.Token.ClientId = spotifyOAuth2[0];
							connections.Token.ClientSecret = spotifyOAuth2[1];
							break;
						case "YouTubeApiKey":
							connections.YouTubeApiKey = infos[1];
							break;
					}
				}
			}
			streamReader.Close();
			return connections;
		}
		catch (Exception)
		{
			DirectoryInfo directory = new(s_path.LocalPath);
			if (!directory.Exists)
				directory.Create();

			StreamWriter streamWriter = new(s_filepath.LocalPath);
			streamWriter.WriteLine("DiscordBotKey;<API Key here>\n" +
								   "DiscordBotKeyDebug;<API Key here>\n" +
								   "MySqlConStr;<DBConnectionString here>\n" +
								   "MySqlConStrDebug;<DBConnectionString here>\n" +
								   "AcoustIdApiKey;<Api Key here>\n" +
								   "SpotifyOAuth2;<ClientId-ClientSecret here>");

			streamWriter.Close();
			throw new Exception($"{s_path.LocalPath}\n" +
								"API key´s and database strings not configured!");
		}
	}
}
