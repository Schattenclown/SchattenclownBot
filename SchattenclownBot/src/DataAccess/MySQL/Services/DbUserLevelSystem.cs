using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.DataAccess.CSV;
using SchattenclownBot.Models;
using SchattenclownBot.Utils;

namespace SchattenclownBot.DataAccess.MySQL.Services
{
    public static class DbUserLevelSystem
    {
        public static List<UserLevelSystem> Read(ulong guildId)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_levelSystem`";
            List<UserLevelSystem> userLevelSystemList = new();
            MySqlConnection mySqlConnection = DbConnection.OpenDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                UserLevelSystem userLevelSystemObj = new()
                {
                            MemberId = mySqlDataReader.GetUInt64("MemberId"),
                            OnlineTicks = mySqlDataReader.GetInt32("OnlineTicks")
                };

                userLevelSystemList.Add(userLevelSystemObj);
            }

            DbConnection.CloseDb(mySqlConnection);
            return userLevelSystemList;
        }

        public static void Add(ulong guildId, UserLevelSystem userLevelSystem)
        {
            string sqlCommand = $"INSERT INTO `{guildId}_levelSystem` (MemberId, OnlineTicks, OnlineTime) " + $"VALUES ({userLevelSystem.MemberId}, {userLevelSystem.OnlineTicks}, '{userLevelSystem.OnlineTime}')";
            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        public static void Change(ulong guildId, UserLevelSystem userLevelSystem)
        {
            string sqlCommand = $"UPDATE `{guildId}_levelSystem` SET OnlineTicks={userLevelSystem.OnlineTicks} WHERE MemberId={userLevelSystem.MemberId}";
            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        public static void CreateTable_UserLevelSystem(ulong guildId)
        {
            Connections connections = CsvConnections.ReadAll();

#if DEBUG
            string database = StringCutter.RemoveUntil(connections.MySqlConStrDebug, "Database=", 9);
#else
            string database = StringCutter.RemoveUntil(connections.MySqlConStr, "Database=", 9);
#endif
            database = StringCutter.RemoveAfter(database, "; Uid", 0);

            string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + $"CREATE TABLE IF NOT EXISTS `{guildId}_levelSystem` (" + "`MemberId` BIGINT NOT NULL," + "`OnlineTicks` INT NOT NULL," + "`OnlineTime` varchar(69) NOT NULL," + "PRIMARY KEY (MemberId)" + ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DbConnection.ExecuteNonQuery(sqlCommand);
        }
    }
}