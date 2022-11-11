using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Objects
{
    internal class API_Object
    {
        public int idPUT { get; set; }
        public string command { get; set; }
        public DateTime RequestTimeStamp { get; set; }
        public ulong RequestSecret { get; set; }

        internal static List<API_Object> GET()
        {
            return DB_API.GET();
        }
        internal static void DELETE(int idPUT)
        {
            DB_API.DELETE(idPUT);
        }
    }
}
