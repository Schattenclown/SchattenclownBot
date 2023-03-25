using System.Collections.Generic;
using Newtonsoft.Json;

namespace SchattenclownBot.Model.Objects
{
   public class MusicBrainz
   {
      public static Root CreateObj(string content)
      {
         Root lst = JsonConvert.DeserializeObject<Root>(content);

         if (lst == null)
         {
            return null;
         }

         Root obj = new()
         {
            Images = lst.Images, Release = lst.Release
         };

         return obj;
      }

      public class Image
      {
         public bool Approved { get; set; }
         public bool Back { get; set; }
         public string Comment { get; set; }
         public int Edit { get; set; }
         public bool Front { get; set; }
         public long Id { get; set; }
         public string ImageString { get; set; }
         public Thumbnails Thumbnails { get; set; }
         public List<string> Types { get; set; }
      }

      public class Root
      {
         public List<Image> Images { get; set; }
         public string Release { get; set; }
      }

      public class Thumbnails
      {
         public string _1200 { get; set; }
         public string _250 { get; set; }
         public string _500 { get; set; }
         public string Large { get; set; }
         public string Small { get; set; }
      }
   }
}