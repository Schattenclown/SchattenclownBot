using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.Models;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Persistence.DataAccess.MySQL.Services
{
    public class DbBotAlarmClocks
    {
        public List<BotAlarmClock> ReadAll()
        {
            string sql = "SELECT * FROM ScAlarmClocks";

            List<BotAlarmClock> botAlarmClockList = new();
            MySqlConnection mySqlConnection = new DbConnection().OpenDb();
            MySqlDataReader mySqlDataReader = new DbConnection().ExecuteReader(sql, mySqlConnection);

            if (mySqlDataReader != null)
            {
                while (mySqlDataReader.Read())
                {
                    BotAlarmClock botAlarmClock = new()
                    {
                                DbEntryId = mySqlDataReader.GetInt32("DBEntryID"),
                                NotificationTime = mySqlDataReader.GetDateTime("NotificationTime"),
                                ChannelId = mySqlDataReader.GetUInt64("ChannelId"),
                                MemberId = mySqlDataReader.GetUInt64("DiscordMemberID")
                    };
                    botAlarmClockList.Add(botAlarmClock);
                }
            }

            new DbConnection().CloseDb(mySqlConnection);
            return botAlarmClockList;
        }

        public void Add(BotAlarmClock botAlarmClock)
        {
            string sql = "INSERT INTO ScAlarmClocks (NotificationTime, ChannelId, DiscordMemberID) " + $"VALUES ('{botAlarmClock.NotificationTime:yyyy-MM-dd HH:mm:ss}', {botAlarmClock.ChannelId}, {botAlarmClock.MemberId})";
            new DbConnection().ExecuteNonQuery(sql);
        }

        public void Delete(BotAlarmClock botAlarmClock)
        {
            string sql = $"DELETE FROM ScAlarmClocks WHERE `DBEntryID` = '{botAlarmClock.DbEntryId}'";
            new DbConnection().ExecuteNonQuery(sql);
        }

        public void CreateTable()
        {
            new CustomLogger().Information("Creating table ScAlarmClocks...", ConsoleColor.Green);

#if DEBUG
            string database = new StringCutter().RemoveUntil(Program.Config["ConnectionStrings:MySqlDebug"], "Database=", "Database=".Length);
#else
            string database = new StringCutter().RemoveUntil(Program.Config["ConnectionStrings:MySql"], "Database=", "Database=".Length);

#endif
            database = new StringCutter().RemoveAfter(database, "; Uid", 0);

            string sql = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + "CREATE TABLE IF NOT EXISTS `ScAlarmClocks` (" + "`DBEntryID` int(12) NOT NULL AUTO_INCREMENT," + "`NotificationTime` DATETIME NOT NULL," + "`ChannelId` bigint(20) NOT NULL," + "`DiscordMemberID` bigint(20) NOT NULL," + "PRIMARY KEY (`DBEntryID`)) " + "ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=latin1;";

            new DbConnection().ExecuteNonQuery(sql);
        }
    }
}