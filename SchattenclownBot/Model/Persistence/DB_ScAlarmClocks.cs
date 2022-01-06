using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using SchattenclownBot.HelpClasses;

namespace SchattenclownBot.Model.Persistence
{
    public class DB_ScAlarmClocks
    {
        public static List<ScAlarmClock> ReadAll()
        {
            string sql = "SELECT * FROM ScAlarmClocks";

            List<ScAlarmClock> lstScAlarmClocks = new List<ScAlarmClock>();
            MySqlConnection connection = DB_Connection.OpenDB();
            MySqlDataReader dataReader = DB_Connection.ExecuteReader(sql, connection);

            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    ScAlarmClock scAlarmClock = new ScAlarmClock
                    {
                        DBEntryID = dataReader.GetInt32("DBEntryID"),
                        NotificationTime = dataReader.GetDateTime("NotificationTime"),
                        ChannelId = dataReader.GetUInt64("ChannelId"),
                        MemberId = dataReader.GetUInt64("MemberId")
                    };
                    lstScAlarmClocks.Add(scAlarmClock);
                }
            }

            return lstScAlarmClocks;
        }
        public static void Add(ScAlarmClock scAlarmClock)
        {
            string sql = $"INSERT INTO ScAlarmClocks (NotificationTime, ChannelId, MemberId) " +
                         $"VALUES ('{scAlarmClock.NotificationTime:yyyy-MM-dd HH:mm:ss}', {scAlarmClock.ChannelId}, {scAlarmClock.MemberId})";
            DB_Connection.ExecuteNonQuery(sql);
        }
        public static void Delete(ScAlarmClock scAlarmClock)
        {
            string sql = $"DELETE FROM ScAlarmClocks WHERE `DBEntryID` = '{scAlarmClock.DBEntryID}'";
            DB_Connection.ExecuteNonQuery(sql);
        }
        public static void CreateTable_ScAlarmClocks()
        {
            CSV_Connections cSV_Connections = new CSV_Connections();
            Connections cons = new Connections();
            cons = CSV_Connections.ReadAll();

            string database = WordCutter.RemoveUntilWord(cons.MySqlConStr, "Database=", 9);
#if DEBUG
            database = WordCutter.RemoveUntilWord(cons.MySqlConStrDebug, "Database=", 9);
#endif
            database = WordCutter.RemoveAfterWord(database, "; Uid", 0);

            string sql = $"CREATE DATABASE IF NOT EXISTS `{database}`;" +
                         $"USE `{database}`;" +
                          "CREATE TABLE IF NOT EXISTS `ScAlarmClocks` (" +
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
