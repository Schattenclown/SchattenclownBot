using SchattenclownBot.Model.Objects;
using System;
using System.IO;

namespace SchattenclownBot.Model.Persistence.Connection
{
    public class CSV_Connections
    {
        private static Uri _path = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SchattenclownBot");
        private static Uri _filepath = new($"{_path}/Connections.csv");
        public static Connections ReadAll()
        {
            try
            {
                Connections connections = new();
                StreamReader streamReader = new(_filepath.LocalPath);
                while (!streamReader.EndOfStream)
                {
                    string row = streamReader.ReadLine();
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
                        default:
                            break;
                    }
                }
                streamReader.Close();
                return connections;
            }
            catch (Exception)
            {
                DirectoryInfo directory = new(_path.LocalPath);
                if (!directory.Exists)
                    directory.Create();

                StreamWriter streamWriter = new(_filepath.LocalPath);
                streamWriter.WriteLine("DiscordBotKey;<API Key here>\n" +
                                       "DiscordBotKeyDebug;<API Key here>\n" +
                                       "MySqlConStr;<DBConnectionString here>\n" +
                                       "MySqlConStrDebug;<DBConnectionString here>");

                streamWriter.Close();
                throw new Exception($"{_path.LocalPath}\n" +
                                    $"API key´s and database string not configurated!");
            }
        }
    }
}
