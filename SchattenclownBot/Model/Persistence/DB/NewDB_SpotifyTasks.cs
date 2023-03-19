using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.Objects;
using System;
using System.Data.SqlClient;

namespace SchattenclownBot.Model.Persistence.DB
{
   internal class NewDB_SpotifyTasks
   {
      public static void CreateDatabaseAndTable()
      {
         string createDatabaseAndTableScript = $@"
                -- Create database
                IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'db_schattenclownbot')
                BEGIN
                    CREATE DATABASE db_schattenclownbot;
                END
                GO

                -- Set the new database as the default for the following statements
                USE db_schattenclownbot;
                GO

                -- Create table
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SpotifyRecordingTasks')
                BEGIN
                    CREATE TABLE SpotifyRecordingTasks (
                        Id INT IDENTITY(1, 1) NOT NULL,
                        DiscordUserId BIGINT NOT NULL,
                        DiscordChannelId BIGINT NOT NULL,
                        DiscordGuildId BIGINT NOT NULL,
                        DurationInMs INT NOT NULL,
                        TrackId NVARCHAR(420) NOT NULL,
                        ExternalId NVARCHAR(420) NOT NULL,
                        Title NVARCHAR(420) NULL,
                        Album NVARCHAR(420) NULL,
                        AlbumArtist NVARCHAR(420) NULL,
                        Comment NVARCHAR(420) NULL,
                        Genre NVARCHAR(420) NULL,
                        TrackNumber SMALLINT NULL,
                        Subtitle NVARCHAR(420) NULL,
                        ReleaseYear NVARCHAR(420) NULL,
                        NotAvailable BIT NULL,
                        Success BIT NULL,
                        TaskCreationTimestamp DATETIME2 DEFAULT SYSDATETIME(),
                        PRIMARY KEY (Id, TrackId)
                    );
                END
                GO";

         SqlConnection connection = new(Bot.Connections.MSSQLConnectionString);
         connection.Open();

         // Split the script into separate batches using the 'GO' keyword as a separator
         string[] batches = createDatabaseAndTableScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

         foreach (string batch in batches)
         {
            SqlCommand command = new(batch, connection);
            command.ExecuteNonQuery();
         }
      }

      public static void Insert(SpotifyTasks spotifyTasks)
      {
         string sqlCommand = @"
        IF NOT EXISTS (SELECT 1 FROM [db_schattenclownbot].[dbo].[SpotifyRecordingTasks] WHERE [TrackId] = @TrackId)
        BEGIN
            INSERT INTO [db_schattenclownbot].[dbo].[SpotifyRecordingTasks]
                ([DiscordUserId], [DiscordChannelId], [DiscordGuildId], [DurationInMs], [TrackId], [ExternalId], [Title],
                 [Album], [AlbumArtist], [Comment], [Genre], [TrackNumber], [Subtitle], [ReleaseYear], [NotAvailable], [Success])
            VALUES
                (@DiscordUserId, @DiscordChannelId, @DiscordGuildId, @DurationInMs, @TrackId, @ExternalId, @Title,
                 @Album, @AlbumArtist, @Comment, @Genre, @TrackNumber, @Subtitle, @ReleaseYear, @NotAvailable, @Success);
        END";

         SqlConnection connection = new(Bot.Connections.MSSQLConnectionString);
         connection.Open();

         SqlCommand command = new(sqlCommand, connection);
         command.Parameters.AddWithValue("@DiscordUserId", (long)spotifyTasks.DiscordUserId);
         command.Parameters.AddWithValue("@DiscordChannelId", (long)spotifyTasks.DiscordChannelId);
         command.Parameters.AddWithValue("@DiscordGuildId", (long)spotifyTasks.DiscordGuildId);
         command.Parameters.AddWithValue("@DurationInMs", spotifyTasks.DurationInMs);
         command.Parameters.AddWithValue("@TrackId", spotifyTasks.TrackId);
         command.Parameters.AddWithValue("@ExternalId", spotifyTasks.ExternalId);
         command.Parameters.AddWithValue("@Title", spotifyTasks.Title);
         command.Parameters.AddWithValue("@Album", spotifyTasks.Album);
         command.Parameters.AddWithValue("@AlbumArtist", spotifyTasks.AlbumArtist);
         command.Parameters.AddWithValue("@Comment", spotifyTasks.Comment);
         command.Parameters.AddWithValue("@Genre", spotifyTasks.Genre);
         command.Parameters.AddWithValue("@TrackNumber", spotifyTasks.TrackNumber);
         command.Parameters.AddWithValue("@Subtitle", spotifyTasks.Subtitle);
         command.Parameters.AddWithValue("@ReleaseYear", spotifyTasks.ReleaseYear);
         command.Parameters.AddWithValue("@NotAvailable", false);
         command.Parameters.AddWithValue("@Success", false);

         command.ExecuteNonQuery();
      }
   }
}
