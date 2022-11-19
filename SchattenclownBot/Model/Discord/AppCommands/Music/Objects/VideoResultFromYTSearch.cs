using System;
using YoutubeExplode.Search;

namespace SchattenclownBot.Model.Discord.AppCommands.Music.Objects;

public class VideoResultFromYTSearch
{
   public VideoSearchResult VideoSearchResult { get; set; }
   public TimeSpan OffsetTimeSpan { get; set; }
   public int Hits { get; set; }

   public VideoResultFromYTSearch(VideoSearchResult videoSearchResult, TimeSpan offsetTimeSpan, int hits)
   {
      VideoSearchResult = videoSearchResult;
      OffsetTimeSpan = offsetTimeSpan;
      Hits = hits;
   }
}