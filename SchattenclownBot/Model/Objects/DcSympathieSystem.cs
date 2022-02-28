using SchattenclownBot.Model.Discord;
using SchattenclownBot.Model.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchattenclownBot.Model.Objects
{
    internal class DcSympathieSystem
    {
        public int VoteTableID { get; set; }
        public ulong VotingUserID { get; set; }
        public ulong VotedUserID { get; set; }
        public int VoteRating { get; set; }
        public DcSympathieSystem()
        {

        }
        public static List<DcUserLevelSystem> Read(ulong guildsId)
        {
            return DB_DcSympathieSystem.Read(guildsId);
        }
        public static void Add(ulong guildsId, DcUserLevelSystem dcLevelSystem)
        {
            DB_DcSympathieSystem.Add(guildsId, dcLevelSystem);
        }
        public static void Change(ulong guildsId, DcUserLevelSystem dcLevelSystem)
        {
            DB_DcSympathieSystem.Change(guildsId, dcLevelSystem);
        }
        public static void CreateTable_DcSympathieSystem(ulong guildsId)
        {
            DB_DcSympathieSystem.CreateTable_DcSympathieSystem(guildsId);
        }
        public static async Task SympathieSystem()
        {
            bool levelSystemVirign = true;

            await Task.Run(async () =>
            {
                do
                {
                    if (DiscordBot.Client.Guilds.ToList().Count != 0)
                    {
                        if (levelSystemVirign)
                        {
                            var guildsList = DiscordBot.Client.Guilds.ToList();
                            foreach (var guildItem in guildsList)
                            {
                                DcSympathieSystem.CreateTable_DcSympathieSystem(guildItem.Value.Id);
                            }
                            levelSystemVirign = false;
                        }
                    }
                    await Task.Delay(1000);
                } while (levelSystemVirign);

                await Task.Delay(2000);
                
            });
        }
    }
}
