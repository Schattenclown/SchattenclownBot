using MySql.Data.MySqlClient;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Persistence
{
    public class DbBotTimer
    {
        public static List<BotTimer> ReadAll()
        {
            string sql = "SELECT * FROM ScTimers";

            List<BotTimer> botTimerList = new();
            MySqlConnection mySqlConnection = DbConnection.OpenDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sql, mySqlConnection);

            if (mySqlDataReader != null)
            {
                while (mySqlDataReader.Read())
                {
                    BotTimer botTimer = new()
                    {
                        DbEntryId = mySqlDataReader.GetInt32("DBEntryID"),
                        NotificationTime = mySqlDataReader.GetDateTime("NotificationTime"),
                        ChannelId = mySqlDataReader.GetUInt64("ChannelId"),
                        MemberId = mySqlDataReader.GetUInt64("MemberId")
                    };
                    botTimerList.Add(botTimer);
                }
            }

            DbConnection.CloseDb(mySqlConnection);
            return botTimerList;
        }
        public static void Add(BotTimer botTimer)
        {
            string sql = "INSERT INTO ScTimers (NotificationTime, ChannelId, MemberId) " +
                      $"VALUES ('{botTimer.NotificationTime:yyyy-MM-dd HH:mm:ss}', {botTimer.ChannelId}, {botTimer.MemberId})";
            DbConnection.ExecuteNonQuery(sql);
        }
        public static void Delete(BotTimer botTimer)
        {
            string sql = $"DELETE FROM ScTimers WHERE `DBEntryID` = '{botTimer.DbEntryId}'";
            DbConnection.ExecuteNonQuery(sql);
        }
        public static void CreateTable_BotTimer()
        {
            Connections connections = CsvConnections.ReadAll();


#if DEBUG
            string database = StringCutter.RemoveUntilWord(connections.MySqlConStrDebug, "Database=", 9);
#else
            string database = StringCutter.RemoveUntilWord(connections.MySqlConStr, "Database=", 9);
#endif
            database = StringCutter.RemoveAfterWord(database, "; Uid", 0);

            string sql = $"CREATE DATABASE IF NOT EXISTS `{database}`;" +
                      $"USE `{database}`;" +
                      "CREATE TABLE IF NOT EXISTS `ScTimers` (" +
                      "`DBEntryID` int(12) NOT NULL AUTO_INCREMENT," +
                      "`NotificationTime` DATETIME NOT NULL," +
                      "`ChannelId` bigint(20) NOT NULL," +
                      "`MemberId` bigint(20) NOT NULL," +
                      "PRIMARY KEY (`DBEntryID`)) " +
                      "ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=latin1;";

            DbConnection.ExecuteNonQuery(sql);
        }
    }
}
