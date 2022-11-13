using DisCatSharp.Entities;
using System;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   internal class QueueItem
   {
      public DiscordGuild DiscordGuild { get; set; }
      public Uri YouTubeUri { get; set; }
      public Uri SpotifyUri { get; set; }
      public bool IsYouTube { get; set; }
      public bool IsSpotify { get; set; }

      internal QueueItem(DiscordGuild discordGuild, Uri youTubeUri, Uri spotifyUri)
      {
         DiscordGuild = discordGuild;
         YouTubeUri = youTubeUri;
         SpotifyUri = spotifyUri;
         IsYouTube = YouTubeUri != null;
         IsSpotify = SpotifyUri != null;
      }

      public QueueItem()
      {

      }
   }
}
