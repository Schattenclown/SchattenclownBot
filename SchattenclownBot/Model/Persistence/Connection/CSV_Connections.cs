using System;
using System.IO;
using SchattenclownBot.Model.Objects;

namespace SchattenclownBot.Model.Persistence.Connection;

public class CSV_Connections
{
   private static readonly Uri Path = new($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/SchattenclownBot");
   private static readonly Uri Filepath = new($"{Path}/Connections.csv");

   public static Connections ReadAll()
   {
      try
      {
         Connections connections = new();
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
                  case "MySqlAPIConStr":
                     connections.MySqlAPIConStr = infos[1].Replace(',', ';');
                     break;
                  case "MySqlAPIConStrDebug":
                     connections.MySqlAPIConStrDebug = infos[1].Replace(',', ';');
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
         streamWriter.WriteLine("DiscordBotKey;<API Key here>\n" + "DiscordBotKeyDebug;<API Key here>\n" + "MySqlConStr;<DBConnectionString here>\n" + "MySqlConStrDebug;<DBConnectionString here>\n" + "MySqlAPIConStr;<DBConnectionString here>\n" + "MySqlAPIConStrDebug;<DBConnectionString here>\n" + "AcoustIdApiKey;<API Key here>\n" + "SpotifyOAuth2;<ClientId-ClientSecret here>");

         streamWriter.Close();
         throw new Exception($"{Path.LocalPath}\n" + "API key´s and database strings not configured!");
      }
   }
}