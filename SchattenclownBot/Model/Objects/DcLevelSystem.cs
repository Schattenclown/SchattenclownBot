using System;
using System.Collections.Generic;
using System.Text;
using SchattenclownBot.Model.Persistence;

namespace SchattenclownBot.Model.Objects
{
    public class DcLevelSystem
    {
        public ulong MemberId { get; set; }
        public ulong GuildId { get; set; }
        public int OnlineTicks { get; set; }
        public TimeSpan OnlineTime { get; set; }
        public DcLevelSystem()
        {

        }
        public static List<DcLevelSystem> Read(ulong guildsId)
        {
            return DB_DcLevelSystem.Read(guildsId);
        }
        public static void Add(ulong guildsId, DcLevelSystem dcLevelSystem)
        {
            DB_DcLevelSystem.Add(guildsId, dcLevelSystem);
        }
        public static void Change(ulong guildsId, DcLevelSystem dcLevelSystem)
        {
            DB_DcLevelSystem.Change(guildsId, dcLevelSystem);
        }
        public static void CreateTable(ulong guildsId)
        {
            DB_DcLevelSystem.CreateTable(guildsId);
        }
    }
}
