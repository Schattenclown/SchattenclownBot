using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchattenclownBot.Models;

namespace SchattenclownBot.Persistence.DatabaseAccess
{
    internal class SympathySystemDBA : DbContext
    {
        public DbSet<SympathySystem> SympathySystems { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.Config["ConnectionStrings:MSSQL"]);
        }

        public void AddOrUpdate(SympathySystem sympathySystem)
        {
            if (SympathySystems.Any(x => x.TargetMemberID == sympathySystem.TargetMemberID && x.VotingMemberID == sympathySystem.VotingMemberID))
            {
                SympathySystem newSympathySystem = SympathySystems.FirstOrDefault(x => x.TargetMemberID == sympathySystem.TargetMemberID && x.VotingMemberID == sympathySystem.VotingMemberID);
                if (newSympathySystem != null)
                {
                    newSympathySystem.Rating = sympathySystem.Rating;
                    SympathySystems.Update(newSympathySystem);
                }
            }
            else
            {
                SympathySystems.Add(sympathySystem);
            }

            SaveChanges();
        }

        public List<SympathySystem> ReadBasedOnTargetMemberID(ulong memberID)
        {
            return SympathySystems.Where(x => x.TargetMemberID == memberID).ToList();
        }

        public int GetMemberRatingsByRatingValue(ulong memberID, int ratingValue)
        {
            return SympathySystems.Count(x => x.TargetMemberID == memberID && x.Rating == ratingValue);
        }
    }
}