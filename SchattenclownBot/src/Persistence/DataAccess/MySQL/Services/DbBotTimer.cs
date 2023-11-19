using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.Models;
using SchattenclownBot.Utils;

namespace SchattenclownBot.Persistence.DataAccess.MySQL.Services
{
    public class DbBotTimer
    {
        public List<BotTimer> ReadAll()
        {
            string sql = "SELECT * FROM ScTimers";

            List<BotTimer> botTimerList = new();
            MySqlConnection mySqlConnection = new DbConnection().OpenDb();
            MySqlDataReader mySqlDataReader = new DbConnection().ExecuteReader(sql, mySqlConnection);

            if (mySqlDataReader != null)
            {
                while (mySqlDataReader.Read())
                {
                    BotTimer botTimer = new()
                    {
                                DbEntryId = mySqlDataReader.GetInt32("DBEntryID"),
                                NotificationTime = mySqlDataReader.GetDateTime("NotificationTime"),
                                ChannelId = mySqlDataReader.GetUInt64("ChannelId"),
                                MemberId = mySqlDataReader.GetUInt64("DiscordMemberID")
                    };
                    botTimerList.Add(botTimer);
                }
            }

            new DbConnection().CloseDb(mySqlConnection);
            return botTimerList;
        }

        public void Add(BotTimer botTimer)
        {
            string sql = "INSERT INTO ScTimers (NotificationTime, ChannelId, DiscordMemberID) " + $"VALUES ('{botTimer.NotificationTime:yyyy-MM-dd HH:mm:ss}', {botTimer.ChannelId}, {botTimer.MemberId})";
            new DbConnection().ExecuteNonQuery(sql);
        }

        public void Delete(BotTimer botTimer)
        {
            string sql = $"DELETE FROM ScTimers WHERE `DBEntryID` = '{botTimer.DbEntryId}'";
            new DbConnection().ExecuteNonQuery(sql);
        }

        public void CreateTable()
        {
            new CustomLogger().Information("Creating table ScTimers...", ConsoleColor.Green);

#if DEBUG
            string database = new StringCutter().RemoveUntil(Program.Config["ConnectionStrings:MySqlDebug"], "Database=", "Database=".Length);
#else
            string database = new StringCutter().RemoveUntil(Program.Config["ConnectionStrings:MySql"], "Database=", "Database=".Length);
#endif
            database = new StringCutter().RemoveAfter(database, "; Uid", 0);

            string sql = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + "CREATE TABLE IF NOT EXISTS `ScTimers` (" + "`DBEntryID` int(12) NOT NULL AUTO_INCREMENT," + "`NotificationTime` DATETIME NOT NULL," + "`ChannelId` bigint(20) NOT NULL," + "`DiscordMemberID` bigint(20) NOT NULL," + "PRIMARY KEY (`DBEntryID`)) " + "ENGINE=InnoDB AUTO_INCREMENT=27 DEFAULT CHARSET=latin1;";

            new DbConnection().ExecuteNonQuery(sql);
        }
    }
}