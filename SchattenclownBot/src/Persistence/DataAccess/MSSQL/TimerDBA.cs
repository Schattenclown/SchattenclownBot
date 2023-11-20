using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchattenclownBot.Models;

namespace SchattenclownBot.Persistence.DataAccess.MSSQL
{
    internal class TimerDBA : DbContext
    {
        public DbSet<Timer> Timers { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.Config["ConnectionStrings:MSSQL"]);
        }

        public void Add(Timer timer)
        {
            Timers.Add(timer);
            SaveChanges();
        }

        public void Delete(Timer timer)
        {
            Timers.Remove(timer);
            SaveChanges();
        }

        public List<Timer> ReadAll()
        {
            return Timers.ToList();
        }
    }
}