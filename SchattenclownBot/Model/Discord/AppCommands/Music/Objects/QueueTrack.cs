using System;
using System.Linq;
using SpotifyAPI.Web;

namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects
{
   public class QueueTrack
   {
      public QueueTrack(Gmc gMc, FullTrack fullTrack)
      {
         if (fullTrack == null)
         {
            return;
         }

         Gmc = gMc;
         if (fullTrack.ExternalUrls.Values.FirstOrDefault() != null)
         {
            SpotifyUri = new Uri(fullTrack.ExternalUrls.Values.FirstOrDefault() ?? string.Empty);
         }

         FullTrack = fullTrack;
         Title = fullTrack.Name;

         for (int i = 0; i < fullTrack.Artists.Count; i++)
         {
            if (fullTrack.Artists.Count != i + 1)
            {
               Artist += FullTrack.Artists[i].Name + ", ";
            }
            else
            {
               Artist += FullTrack.Artists[i].Name;
            }
         }
      }

      public QueueTrack(Gmc gMc, Uri youTubeUri, string title, string artist)
      {
         Gmc = gMc;
         YouTubeUri = youTubeUri;
         Title = title;
         Artist = artist;
         IsAdded = true;
      }

      public QueueTrack(Gmc gMc, string localPath, string title, string artist)
      {
         Gmc = gMc;
         LocalPath = localPath;
         Title = title;
         Artist = artist;
         IsAdded = true;
      }

      public Gmc Gmc { get; set; }
      public string Title { get; set; }
      public string Artist { get; set; }
      public Uri YouTubeUri { get; set; }
      public Uri SpotifyUri { get; set; }
      public string LocalPath { get; set; }
      public FullTrack FullTrack { get; set; }
      public bool IsAdded { get; set; }
      public bool HasBeenPlayed { get; set; }
   }
}