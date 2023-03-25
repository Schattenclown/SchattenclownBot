using System;
using System.Linq;
using SpotifyAPI.Web;

namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects
{
   public class QueueTrack
   {
      public QueueTrack(GMC gMC, FullTrack fullTrack)
      {
         if (fullTrack == null)
         {
            return;
         }

         GMC = gMC;
         if (fullTrack.ExternalUrls.Values.FirstOrDefault() != null)
         {
            SpotifyUri = new Uri(fullTrack.ExternalUrls.Values.FirstOrDefault());
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

      public QueueTrack(GMC gMC, Uri youTubeUri, string title, string artist)
      {
         GMC = gMC;
         YouTubeUri = youTubeUri;
         Title = title;
         Artist = artist;
         IsAdded = true;
      }

      public QueueTrack(GMC gMC, string localPath, string title, string artist)
      {
         GMC = gMC;
         LocalPath = localPath;
         Title = title;
         Artist = artist;
         IsAdded = true;
      }

      public GMC GMC { get; set; }
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