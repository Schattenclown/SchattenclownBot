using SchattenclownBot.Model.Objects;
using System;
using System.IO;

namespace SchattenclownBot.Model.Persistence.Connection
{
    public class CsvConnections
    {
        private static readonly Uri Path = new($"{Environment.CurrentDirectory}");
        private static readonly Uri Filepath = new($"{Path}/Connections.csv");
        public static Connections ReadAll()
        {
            try
            {
                StreamReader streamReader = new(Filepath.LocalPath);
                Connections connections = new();
                while (!streamReader.EndOfStream)
                {
                    string row = streamReader.ReadLine();
                    if (row != null)
                    {
                        string[] infos = row.Split(';');

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
                            case "AcoustIdApiKey":
                                connections.AcoustIdApiKey = infos[1];
                                break;
                            case "SpotifyOAuth2":
                                connections.Token = new Connections.SpotifyOAuth2();
                                string[] spotifyOAuth2 = infos[1].Split('-');
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
                throw new Exception($"{Path.LocalPath}\n" + "API key´s and database strings not configured!");
            }
        }
    }
}
