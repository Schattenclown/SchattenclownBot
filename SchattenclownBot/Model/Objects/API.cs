using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Objects
{
    internal class API
    {
        public int PUTiD { get; set; }
        public string Command { get; set; }
        public DateTime RequestTimeStamp { get; set; }
        public ulong RequestSecret { get; set; }

        internal static List<API> GET()
        {
            return DB_API.GET();
        }
        internal static void DELETE(int pUTiD)
        {
            DB_API.DELETE(pUTiD);
        }
    }
}
