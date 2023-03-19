﻿using SchattenclownBot.Model.Persistence.DB;

namespace SchattenclownBot.Model.Objects;

public class SpotifyTasks
{
   public int Id { get; set; }
   public ulong DiscordUserId { get; set; }
   public ulong DiscordGuildId { get; set; }
   public ulong DiscordChannelId { get; set; }
   public int DurationInMs { get; set; }
   public string TrackId { get; set; }
   public string ExternalId { get; set; }
   public string Title { get; set; }
   public string Album { get; set; }
   public string AlbumArtist { get; set; }
   public string Comment { get; set; }
   public string Genre { get; set; }
   public int TrackNumber { get; set; }
   public string Subtitle { get; set; }
   public string ReleaseYear { get; set; }
   public bool NotAvailable { get; set; }
   public bool Success { get; set; }
   public string TaskCreationTimestamp { get; set; }

   public static void CreateDatabaseAndTable()
   {
      NewDB_SpotifyTasks.CreateDatabaseAndTable();
   }

   public static void Insert(SpotifyTasks spotifyTasks)
   {
      NewDB_SpotifyTasks.Insert(spotifyTasks);
   }
}