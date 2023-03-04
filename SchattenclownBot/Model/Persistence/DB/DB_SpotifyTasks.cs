using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence.DB;

internal class DB_SpotifyTasks
{
   public static List<SpotifyTasks> ReadAll()
   {
      string sqlCommand = "SELECT * FROM `SpotifyRecordingTasks`";

      List<SpotifyTasks> spotifyTasks = new();
      MySqlConnection mySqlConnection = DB_Connection.OpenDB();
      MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

      while (mySqlDataReader.Read())
      {
         SpotifyTasks spotifyTask = new()
         {
            Id = mySqlDataReader.GetInt32("Id"),
            DiscordUserId = mySqlDataReader.GetUInt64("DiscordUserId"),
            DiscordGuildId = mySqlDataReader.GetUInt64("DiscordGuildId"),
            DiscordChannelId = mySqlDataReader.GetUInt64("DiscordChannelId"),
            TrackId = mySqlDataReader.GetString("TrackId"),
            ExternalId = mySqlDataReader.GetString("ExternalId"),
            Title = mySqlDataReader.GetString("Title"),
            Album = mySqlDataReader.GetString("Album"),
            AlbumArtist = mySqlDataReader.GetString("AlbumArtist"),
            Comment = mySqlDataReader.GetString("Comment"),
            Genre = mySqlDataReader.GetString("Genre"),
            TrackNumber = mySqlDataReader.GetInt16("TrackNumber"),
            Subtitle = mySqlDataReader.GetString("Subtitle"),
            ReleaseYear = mySqlDataReader.GetString("ReleaseYear"),
            NotAvailable = mySqlDataReader.GetBoolean("NotAvailable"),
            Success = mySqlDataReader.GetBoolean("Success"),
            TaskCreationTimestamp = mySqlDataReader.GetString("TaskCreationTimestamp")
         };

         spotifyTasks.Add(spotifyTask);
      }

      DB_Connection.CloseDB(mySqlConnection);
      return spotifyTasks;
   }

   public static void INSERT(SpotifyTasks spotifyTasks)
   {
      string sqlCommand = "INSERT INTO `db_schattenclownbot`.`SpotifyRecordingTasks`" + "(`DiscordUserId`," + "`DiscordChannelId`," + "`DiscordGuildId`," + "`TrackId`," + "`ExternalId`," + "`Title`," + "`Album`," + "`AlbumArtist`," + "`Comment`," + "`Genre`," + "`TrackNumber`," + "`Subtitle`," + "`ReleaseYear`," + "`NotAvailable`," + "`Success`)" + "VALUES" + $"('{spotifyTasks.DiscordUserId}'," + $"'{spotifyTasks.DiscordChannelId}'," + $"'{spotifyTasks.DiscordGuildId}'," + $"'{spotifyTasks.TrackId}'," + $"'{spotifyTasks.ExternalId}'," + $"'{spotifyTasks.Title.Replace(@"'", @"\'")}'," + $"'{spotifyTasks.Album.Replace(@"'", @"\'")}'," + $"'{spotifyTasks.AlbumArtist.Replace(@"'", @"\'")}'," + $"'{spotifyTasks.Comment.Replace(@"'", @"\'")}'," + $"'{spotifyTasks.Genre.Replace(@"'", @"\'")}'," + $"'{spotifyTasks.TrackNumber}'," + $"'{spotifyTasks.Subtitle.Replace(@"'", @"\'")}'," + $"'{spotifyTasks.ReleaseYear}'," + "'0'," + "'0');";

      DB_Connection.ExecuteNonQuery(sqlCommand);
   }

   public static void CreateTable_SpotifyTasks()
   {
      Connections connections = CSV_Connections.ReadAll();

#if DEBUG
      string database = StringCutter.RmUntil(connections.MySqlConStrDebug, "Database=", 9);
#else
      string database = StringCutter.RmUntil(connections.MySqlConStr, "Database=", 9);
#endif
      database = StringCutter.RmAfter(database, "; Uid", 0);

      string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + "CREATE TABLE IF NOT EXISTS `SpotifyRecordingTasks` (" + "`Id` int NOT NULL AUTO_INCREMENT," + "`DiscordUserId` bigint NOT NULL," + "`DiscordChannelId` bigint NOT NULL," + "`DiscordGuildId` bigint NOT NULL," + "`TrackId` varchar(420) NOT NULL," + "`ExternalId` varchar(420) NOT NULL," + "`Title` varchar(420) DEFAULT NULL," + "`Album` varchar(420) DEFAULT NULL," + "`AlbumArtist` varchar(420) DEFAULT NULL," + "`Comment` varchar(420) DEFAULT NULL," + "`Genre` varchar(420) DEFAULT NULL," + "`TrackNumber` smallint DEFAULT NULL," + "`Subtitle` varchar(420) DEFAULT NULL," + "`ReleaseYear` varchar(420) DEFAULT NULL," + "`NotAvailable` tinyint DEFAULT NULL," + "`Success` tinyint DEFAULT NULL," + "`TaskCreationTimestamp` timestamp NULL DEFAULT CURRENT_TIMESTAMP," + "PRIMARY KEY (`Id`,`TrackId`)) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;";

      DB_Connection.ExecuteNonQuery(sqlCommand);
   }
}