using System;
using System.Collections.Generic;
using System.Text;
using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Objects
{
    public class DcUserLevelSystem
    {
        public ulong MemberId { get; set; }
        public ulong GuildId { get; set; }
        public int OnlineTicks { get; set; }
        public TimeSpan OnlineTime { get; set; }
        public DcUserLevelSystem()
        {

        }
        public static List<DcUserLevelSystem> Read(ulong guildsId)
        {
            return DB_DcUserLevelSystem.Read(guildsId);
        }
        public static void Add(ulong guildsId, DcUserLevelSystem dcLevelSystem)
        {
            DB_DcUserLevelSystem.Add(guildsId, dcLevelSystem);
        }
        public static void Change(ulong guildsId, DcUserLevelSystem dcLevelSystem)
        {
            DB_DcUserLevelSystem.Change(guildsId, dcLevelSystem);
        }
        public static void CreateTable(ulong guildsId)
        {
            DB_DcUserLevelSystem.CreateTable(guildsId);
        }
    }
}
