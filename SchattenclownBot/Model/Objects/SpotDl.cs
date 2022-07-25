using Newtonsoft.Json;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Objects
{
    internal class SpotDl
    {
        public string name { get; set; }
        public List<string> artists { get; set; }
        public string artist { get; set; }
        public string album_name { get; set; }
        public string album_artist { get; set; }
        public List<string> genres { get; set; }
        public int disc_number { get; set; }
        public int disc_count { get; set; }
        public double duration { get; set; }
        public int year { get; set; }
        public string date { get; set; }
        public int track_number { get; set; }
        public int tracks_count { get; set; }
        public string song_id { get; set; }
        public string cover_url { get; set; }
        public bool @explicit { get; set; }
        public string publisher { get; set; }
        public string url { get; set; }
        public string isrc { get; set; }
        public string copyright_text { get; set; }
        public string download_url { get; set; }
        public object song_list { get; set; }
        public object lyrics { get; set; }
    }
}
