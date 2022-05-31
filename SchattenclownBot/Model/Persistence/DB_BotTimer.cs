using System;
using System.Collections.Generic;
using System.Text;

using MySql.Data.MySqlClient;

using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using SchattenclownBot.HelpClasses;

namespace SchattenclownBot.Model.Persistence
{
    public class DB_BotTimer
    {
        public static List<BotTimer> ReadAll()
        {
            string sql = "SELECT * FROM ScTimers";

            List<BotTimer> botTimerList = new List<BotTimer>();
            MySqlConnection mySqlConnection = DB_Connection.OpenDB();
            MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sql, mySqlConnection);

            if (mySqlDataReader != null)
            {
                while (mySqlDataReader.Read())
                {
                    BotTimer botTimer = new BotTimer
                    {
                        DBEntryID = mySqlDataReader.GetInt32("DBEntryID"),
                        NotificationTime = mySqlDataReader.GetDateTime("NotificationTime"),
                        ChannelId = mySqlDataReader.GetUInt64("ChannelId"),
                        MemberId = mySqlDataReader.GetUInt64("MemberId")
                    };
                    botTimerList.Add(botTimer);
                }
            }

            DB_Connection.CloseDB(mySqlConnection);
            return botTimerList;
        }
        public static void Add(BotTimer botTimer)
        {
            string sql = $"INSERT INTO ScTimers (NotificationTime, ChannelId, MemberId) " +
                         $"VALUES ('{botTimer.NotificationTime:yyyy-MM-dd HH:mm:ss}', {botTimer.ChannelId}, {botTimer.MemberId})";
            DB_Connection.ExecuteNonQuery(sql);
        }
        public static void Delete(BotTimer botTimer)
        {
            string sql = $"DELETE FROM ScTimers WHERE `DBEntryID` = '{botTimer.DBEntryID}'";
            DB_Connection.ExecuteNonQuery(sql);
        }
        public static void CreateTable_BotTimer()
        {
            CSV_Connections cSV_Connections = new CSV_Connections();
            Connections connections = new Connections();
            connections = CSV_Connections.ReadAll();

            string database = StringCutter.RemoveUntilWord(connections.MySqlConStr, "Database=", 9);
#if DEBUG
            database = StringCutter.RemoveUntilWord(connections.MySqlConStrDebug, "Database=", 9);
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

            DB_Connection.ExecuteNonQuery(sql);
        }
    }
}
