using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchattenclownBot.Models;

namespace SchattenclownBot.Persistence.DatabaseAccess
{
    internal class AlarmDBA : DbContext
    {
        public DbSet<Alarm> Alarms { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.Config["ConnectionStrings:MSSQL"]);
        }

        public void Add(Alarm alarm)
        {
            Alarms.Add(alarm);
            SaveChanges();
        }

        public void Delete(Alarm alarm)
        {
            Alarms.Remove(alarm);
            SaveChanges();
        }

        public List<Alarm> ReadAll()
        {
            return Alarms.ToList();
        }
    }
}