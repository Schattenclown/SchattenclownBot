﻿using System;
using System.Collections.Generic;
using System.Text;

using MySql.Data.MySqlClient;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence
{
    public static class DB_UserLevelSystem
    {
        public static List<UserLevelSystem> Read(ulong guildId)
        {
            var sqlCommand = $"SELECT * FROM `{guildId}_levelSystem`";
            var userLevelSystemList = new List<UserLevelSystem>();
            var mySqlConnection = DB_Connection.OpenDB();
            var mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                var userLevelSystemObj = new UserLevelSystem()
                {
                    MemberId = mySqlDataReader.GetUInt64("MemberId"),
                    OnlineTicks = mySqlDataReader.GetInt32("OnlineTicks")                    
                };

                userLevelSystemList.Add(userLevelSystemObj);
            }

            DB_Connection.CloseDB(mySqlConnection);
            return userLevelSystemList;
        }
        public static void Add(ulong guildId, UserLevelSystem userLevelSystem)
        {
            var sqlCommand = $"INSERT INTO `{guildId}_levelSystem` (MemberId, OnlineTicks, OnlineTime) " +
                             $"VALUES ({userLevelSystem.MemberId}, {userLevelSystem.OnlineTicks}, '{userLevelSystem.OnlineTime}')";
            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void Change(ulong guildId, UserLevelSystem userLevelSystem)
        {
            var sqlCommand = $"UPDATE `{guildId}_levelSystem` SET OnlineTicks={userLevelSystem.OnlineTicks} WHERE MemberId={userLevelSystem.MemberId}";
            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void CreateTable_UserLevelSystem(ulong guildId)
        {
            var connetions = CSV_Connections.ReadAll();

            var database = StringCutter.RemoveUntilWord(connetions.MySqlConStr, "Database=", 9);
#if DEBUG
            database = StringCutter.RemoveUntilWord(connetions.MySqlConStrDebug, "Database=", 9);
#endif
            database = StringCutter.RemoveAfterWord(database, "; Uid", 0);

            var sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" +
                             $"USE `{database}`;" +
                             $"CREATE TABLE IF NOT EXISTS `{guildId}_levelSystem` (" +
                             "`MemberId` BIGINT NOT NULL," +
                             "`OnlineTicks` INT NOT NULL," +
                             "`OnlineTime` varchar(69) NOT NULL," +
                             "PRIMARY KEY (MemberId)" +
                             ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
    }
}