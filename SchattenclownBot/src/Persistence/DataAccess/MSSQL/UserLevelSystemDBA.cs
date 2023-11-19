#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchattenclownBot.Models;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Persistence.DataAccess.MSSQL
{
    public class UserLevelSystemDBA : DbContext
    {
        public DbSet<UserLevelSystem> UserLevelSystems { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Program.Config["ConnectionStrings:MSSQL"]);
        }

        public void AddOrUpdate(UserLevelSystem userLevelSystem)
        {
            UserLevelSystem? userLevelSystemTemp = UserLevelSystems.FirstOrDefault(x => x.DiscordGuildID == userLevelSystem.DiscordGuildID && x.DiscordMemberID == userLevelSystem.DiscordMemberID);
            if (userLevelSystemTemp == null)
            {
                UserLevelSystems.Add(userLevelSystem);
                SaveChanges();
                new CustomLogger().Debug($"{userLevelSystem}", ConsoleColor.Green);
                return;
            }

            userLevelSystemTemp.OnlineTicks = userLevelSystem.OnlineTicks;
            UserLevelSystems.Update(userLevelSystemTemp);
            SaveChanges();
            new CustomLogger().Debug($"{userLevelSystemTemp}", ConsoleColor.Cyan);
        }

        public List<UserLevelSystem> GetByGuildId(ulong guildId)
        {
            return UserLevelSystems.Where(x => x.DiscordGuildID == guildId).ToList();
        }
    }
}