namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects;

internal class DeterminedStreamingService
{
   public bool IsYouTube { get; set; }
   public bool IsYouTubePlaylist { get; set; }
   public bool IsYouTubePlaylistWithIndex { get; set; }
   public bool IsSpotify { get; set; }
   public bool IsSpotifyPlaylist { get; set; }
   public bool IsSpotifyAlbum { get; set; }
   public bool Nothing { get; set; }

   public DeterminedStreamingService IdentifyStreamingService(string webLink)
   {
      DeterminedStreamingService result = new();

      if (webLink.Contains("watch?v=") || webLink.Contains("youtu.be") || webLink.Contains("&list=") || webLink.Contains("playlist?list="))
      {
         result.IsYouTube = true;

         if (webLink.Contains("&list=") || webLink.Contains("playlist?list="))
         {
            result.IsYouTubePlaylist = true;

            if (webLink.Contains("&index="))
            {
               result.IsYouTubePlaylistWithIndex = true;
            }
         }
      }
      else if (webLink.Contains("/track/") || webLink.Contains("/playlist/") || webLink.Contains("/album/") || webLink.Contains(":album:"))
      {
         result.IsSpotify = true;

         if (webLink.Contains("/playlist/"))
         {
            result.IsSpotifyPlaylist = true;
         }
         else if (webLink.Contains("/album/") || webLink.Contains(":album:"))
         {
            result.IsSpotifyAlbum = true;
         }
      }
      else
      {
         result.Nothing = true;
      }

      return result;
   }
}