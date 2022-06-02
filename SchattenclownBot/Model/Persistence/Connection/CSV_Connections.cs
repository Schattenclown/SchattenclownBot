using System;
using System.IO;

using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.Persistence.Connection
{
    public class CSV_Connections
    {
        private static Uri _path = new Uri($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SchattenclownBot");
        private static Uri _filepath = new Uri($"{_path}/Connections.csv");
        public static Connections ReadAll()
        {
            try
            {
                var connections = new Connections();
                var streamReader = new StreamReader(_filepath.LocalPath);
                while (!streamReader.EndOfStream)
                {
                    var row = streamReader.ReadLine();
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
                        default:
                            break;
                    }
                }
                streamReader.Close();
                return connections;
            }
            catch (Exception)
            {
                var directory = new DirectoryInfo(_path.LocalPath);
                if (!directory.Exists)
                    directory.Create();

                var streamWriter = new StreamWriter(_filepath.LocalPath);
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
