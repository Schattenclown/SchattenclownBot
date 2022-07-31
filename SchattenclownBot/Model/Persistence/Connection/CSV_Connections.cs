using SchattenclownBot.Model.Objects;
using System;
using System.IO;

namespace SchattenclownBot.Model.Persistence.Connection
{
    public class CsvConnections
    {
        private static readonly Uri Path = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SchattenclownBot");
        private static readonly Uri Filepath = new($"{Path}/Connections.csv");

        public static Connections ReadAll()
        {
            Connections connections = new();

            if (Environment.GetEnvironmentVariable("ENV_SET").ToString() == "0")
            {
                try
                {
                    StreamReader streamReader = new(Filepath.LocalPath);
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
                    DirectoryInfo directory = new(Path.LocalPath);
                    if (!directory.Exists)
                        directory.Create();

                    StreamWriter streamWriter = new(Filepath.LocalPath);
                    streamWriter.WriteLine("DiscordBotKey;<API Key here>\n" +
                                           "DiscordBotKeyDebug;<API Key here>\n" +
                                           "MySqlConStr;<DBConnectionString here>\n" +
                                           "MySqlConStrDebug;<DBConnectionString here>\n" +
                                           "AcoustIdApiKey;<Api Key here>\n" +
                                           "SpotifyOAuth2;<ClientId-ClientSecret here>");

                    streamWriter.Close();
                    throw new Exception($"{Path.LocalPath}\n" +
                                        "API key´s and database strings not configured!");
                }
            }
            else if (Environment.GetEnvironmentVariable("ENV_SET").ToString() == "1")
            {
                /*
                services:
                version: '3.4'

                services:
                  oebibot:
                    image: elbrodark/schattenclownbot:latest
                    restart: unless-stopped
                    environment:
                    - ENV_SET="1"
                    - DiscordBotKey=""
                    - DiscordBotKeyDebug=""
                    - MySqlConStr=""
                    - MySqlConStrDebug=""
                    - AcoustIdApiKey=""
                    - SpotifyOAuth2=""
                 *
                 */

                connections.DiscordBotKey = Environment.GetEnvironmentVariable("DiscordBotKey").ToString();
                connections.DiscordBotDebug = Environment.GetEnvironmentVariable("DiscordBotDebug").ToString();
                connections.MySqlConStr = Environment.GetEnvironmentVariable("MySqlConStr").ToString().Replace(',', ';');
                connections.MySqlConStrDebug = Environment.GetEnvironmentVariable("MySqlConStrDebug").ToString().Replace(',', ';');
                connections.AcoustIdApiKey = Environment.GetEnvironmentVariable("AcoustIdApiKey").ToString();
                connections.Token = new Connections.SpotifyOAuth2();
                string[] spotifyOAuth2 = Environment.GetEnvironmentVariable("SpotifyOAuth2").ToString().Split('-');
                connections.Token.ClientId = spotifyOAuth2[0];
                connections.Token.ClientSecret = spotifyOAuth2[1];
                //connections.YouTubeApiKey = Environment.GetEnvironmentVariable("YouTubeApiKey").ToString();



                return connections;
            }
            return null;
        }
    }
}
