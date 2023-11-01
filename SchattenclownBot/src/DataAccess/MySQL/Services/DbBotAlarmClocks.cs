using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.DataAccess.CSV;
using SchattenclownBot.Models;
using SchattenclownBot.Utils;

namespace SchattenclownBot.DataAccess.MySQL.Services
{
    public class DbBotAlarmClocks
    {
        public static List<BotAlarmClock> ReadAll()
        {
            string sql = "SELECT * FROM ScAlarmClocks";

            List<BotAlarmClock> botAlarmClockList = new();
            MySqlConnection mySqlConnection = DbConnection.OpenDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sql, mySqlConnection);

            if (mySqlDataReader != null)
            {
                while (mySqlDataReader.Read())
                {
                    BotAlarmClock botAlarmClock = new()
                    {
                                DbEntryId = mySqlDataReader.GetInt32("DBEntryID"),
                                NotificationTime = mySqlDataReader.GetDateTime("NotificationTime"),
                                ChannelId = mySqlDataReader.GetUInt64("ChannelId"),
                                MemberId = mySqlDataReader.GetUInt64("MemberId")
                    };
                    botAlarmClockList.Add(botAlarmClock);
                }
            }

            DbConnection.CloseDb(mySqlConnection);
            return botAlarmClockList;
        }

        public static void Add(BotAlarmClock botAlarmClock)
        {
            string sql = "INSERT INTO ScAlarmClocks (NotificationTime, ChannelId, MemberId) " + $"VALUES ('{botAlarmClock.NotificationTime:yyyy-MM-dd HH:mm:ss}', {botAlarmClock.ChannelId}, {botAlarmClock.MemberId})";
            DbConnection.ExecuteNonQuery(sql);
        }

        public static void Delete(BotAlarmClock botAlarmClock)
        {
            string sql = $"DELETE FROM ScAlarmClocks WHERE `DBEntryID` = '{botAlarmClock.DbEntryId}'";
            DbConnection.ExecuteNonQuery(sql);
        }

        public static void CreateTable_BotAlarmClock()
        {
            CustomLogger.ToConsole("Creating table ScAlarmClocks...", ConsoleColor.Green);
            Connections connections = CsvConnections.ReadAll();

#if DEBUG
            string database = StringCutter.RemoveUntil(connections.MySqlConStrDebug, "Database=", 9);
#else
            string database = StringCutter.RemoveUntil(connections.MySqlConStr, "Database=", 9);

#endif
            database = StringCutter.RemoveAfter(database, "; Uid", 0);

            string sql = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + "CREATE TABLE IF NOT EXISTS `ScAlarmClocks` (" + "`DBEntryID` int(12) NOT NULL AUTO_INCREMENT," + "`NotificationTime` DATETIME NOT NULL," + "`ChannelId` bigint(20) NOT NULL," + "`MemberId` bigint(20) NOT NULL," + "PRIMARY KEY (`DBEntryID`)) " + "ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=latin1;";

            DbConnection.ExecuteNonQuery(sql);
        }
    }
}