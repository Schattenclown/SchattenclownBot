using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using YoutubeExplode.Playlists;

namespace SchattenclownBot.Model.Discord.AppCommands.Music
{
   public class QueueTrack
   {
      public GCM GCM { get; set; }
      public string Title { get; set; }
      public string Artist { get; set; }
      public Uri YouTubeUri { get; set; }
      public Uri SpotifyUri { get; set; }
      public FullTrack FullTrack { get; set; }
      public bool IsAdded { get; set; }
      public bool HasBeenPlayed { get; set; }
      public QueueTrack(GCM gCM, FullTrack fullTrack)
      {
         GCM = gCM;
         SpotifyUri = new Uri(fullTrack.ExternalUrls.Values.FirstOrDefault());
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
      public QueueTrack(GCM gCM, Uri youTubeUri, string title, string artist)
      {
         GCM = gCM;
         YouTubeUri = youTubeUri;
         Title = title;
         Artist = artist;
         IsAdded = true;
      }
   }
}
