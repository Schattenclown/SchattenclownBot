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
                Connections cons = new Connections();
                StreamReader sr = new StreamReader(_filepath.LocalPath);
                while (!sr.EndOfStream)
                {
                    string row = sr.ReadLine();
                    string[] infos = row.Split(';');

                    switch (infos[0])
                    {
                        case "DiscordBotKey":
                            cons.DiscordBotKey = infos[1];
                            break;
                        case "DiscordBotKeyDebug":
                            cons.DiscordBotDebug = infos[1];
                            break;
                        case "MySqlConStr":
                            cons.MySqlConStr = infos[1].Replace(',', ';');
                            break;
                        case "MySqlConStrDebug":
                            cons.MySqlConStrDebug = infos[1].Replace(',', ';');
                            break;
                        default:
                            break;
                    }
                }
                sr.Close();
                return cons;
            }
            catch (Exception)
            {
                DirectoryInfo dir = new DirectoryInfo(_path.LocalPath);
                if (!dir.Exists)
                    dir.Create();

                StreamWriter streamWriter = new StreamWriter(_filepath.LocalPath);
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
