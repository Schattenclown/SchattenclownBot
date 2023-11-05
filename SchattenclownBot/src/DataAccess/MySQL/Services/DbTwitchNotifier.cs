using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using SchattenclownBot.Integrations.Discord.Main;
using SchattenclownBot.Integrations.Discord.Services;
using SchattenclownBot.Utils;

namespace SchattenclownBot.DataAccess.MySQL.Services
{
    internal class DbTwitchNotifier
    {
        public static List<TwitchNotifier> Read(ulong guildId)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_TwitchNotifier`";
            List<TwitchNotifier> twitchNotifierList = new();
            MySqlConnection mySqlConnection = DbConnection.OpenDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                TwitchNotifier twitchNotifierObj = new()
                {
                            DiscordGuildId = mySqlDataReader.GetUInt64("DiscordGuildId"),
                            DiscordMemberId = mySqlDataReader.GetUInt64("DiscordMemberId"),
                            DiscordChannelId = mySqlDataReader.GetUInt64("DiscordChannelId"),
                            DiscordRoleId = mySqlDataReader.GetUInt64("DiscordRoleId"),
                            TwitchUserId = mySqlDataReader.GetUInt64("TwitchUserId"),
                            TwitchChannelUrl = mySqlDataReader.GetString("TwitchChannelUrl")
                };

                twitchNotifierList.Add(twitchNotifierObj);
            }

            DbConnection.CloseDb(mySqlConnection);
            return twitchNotifierList;
        }

        public static void Add(TwitchNotifier twitchNotifier)
        {
            string sqlCommand = $"INSERT INTO `{twitchNotifier.DiscordGuildId}_TwitchNotifier` (`DiscordGuildId`, `DiscordMemberId`, `DiscordChannelId`, `DiscordRoleId`, `TwitchUserId`, `TwitchChannelUrl`) " + $"VALUES ({twitchNotifier.DiscordGuildId}, {twitchNotifier.DiscordMemberId}, {twitchNotifier.DiscordChannelId}, {twitchNotifier.DiscordRoleId}, {twitchNotifier.TwitchUserId}, '{twitchNotifier.TwitchChannelUrl}')";
            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        /*
        public static void Change(ulong guildId, UserLevelSystem userLevelSystem)
        {
           string sqlCommand = $"UPDATE `{guildId}_levelSystem` SET OnlineTicks={userLevelSystem.OnlineTicks} WHERE MemberId={userLevelSystem.MemberId}";
           DB_Connection.ExecuteNonQuery(sqlCommand);
        }*/

        public static Task CreateTable(ulong guildId)
        {
#if DEBUG
            string database = StringCutter.RemoveUntil(DiscordBot.Config["ConnectionStrings:MySqlDebug"], "Database=", "Database=".Length);
#else
            string database = StringCutter.RemoveUntil(DiscordBot.Config["ConnectionStrings:MySql"], "Database=", "Database=".Length);
#endif
            database = StringCutter.RemoveAfter(database, "; Uid", 0);

            string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + $"CREATE TABLE IF NOT EXISTS `{guildId}_TwitchNotifier` (" + "`DiscordGuildId` BIGINT NOT NULL," + "`DiscordMemberId` BIGINT NOT NULL," + "`DiscordChannelId` BIGINT NOT NULL," + "`DiscordRoleId` BIGINT NOT NULL," + "`TwitchUserId` BIGINT," + "`TwitchChannelUrl` VARCHAR(64))" + " ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DbConnection.ExecuteNonQuery(sqlCommand);

            return Task.CompletedTask;
        }
    }
}