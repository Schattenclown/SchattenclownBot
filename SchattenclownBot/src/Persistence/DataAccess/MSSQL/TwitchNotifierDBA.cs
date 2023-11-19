using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchattenclownBot.Models;

namespace SchattenclownBot.Persistence.DataAccess.MSSQL
{
    internal class TwitchNotifierDBA : DbContext
    {
        public DbSet<TwitchNotifier> TwitchNotifiers { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.Config["ConnectionStrings:MSSQL"]);
        }

        public List<TwitchNotifier> ReadBasedOnGuild(ulong guildId)
        {
            return TwitchNotifiers.Where(x => x.DiscordGuildID == guildId).ToList();
        }

        public List<TwitchNotifier> Read()
        {
            return TwitchNotifiers.ToList();
        }

        public void Add(TwitchNotifier twitchNotifier)
        {
            TwitchNotifiers.Add(twitchNotifier);
            SaveChanges();
        }
    }
}