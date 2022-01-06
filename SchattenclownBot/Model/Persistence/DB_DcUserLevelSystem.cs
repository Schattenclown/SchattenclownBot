﻿using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using SchattenclownBot.HelpClasses;

namespace SchattenclownBot.Model.Persistence
{
    public static class DB_DcUserLevelSystem
    {
        public static List<DcUserLevelSystem> Read(ulong guildId)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}`";
            List<DcUserLevelSystem> dcUserLevelSystemList = new List<DcUserLevelSystem>();
            MySqlConnection mySqlConnection = DB_Connection.OpenDB();
            MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                DcUserLevelSystem dcUserLevelSystemObj = new DcUserLevelSystem()
                {
                    MemberId = mySqlDataReader.GetUInt64("MemberId"),
                    OnlineTicks = mySqlDataReader.GetInt32("OnlineTicks")                    
                };

                dcUserLevelSystemList.Add(dcUserLevelSystemObj);
            }

            DB_Connection.CloseDB(mySqlConnection);
            return dcUserLevelSystemList;
        }
        public static void Add(ulong guildId, DcUserLevelSystem dcLevelSystem)
        {
            string sqlCommand = $"INSERT INTO `{guildId}` (MemberId, OnlineTicks, OnlineTime) " +
                                $"VALUES ({dcLevelSystem.MemberId}, {dcLevelSystem.OnlineTicks}, '{dcLevelSystem.OnlineTime}')";
            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void Change(ulong guildId, DcUserLevelSystem dcLevelSystem)
        {
            string sqlCommand = $"UPDATE `{guildId}` SET OnlineTicks={dcLevelSystem.OnlineTicks} WHERE MemberId={dcLevelSystem.MemberId}";
            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void CreateTable(ulong guildsId)
        {
            Connections connetions = CSV_Connections.ReadAll();

#pragma warning disable CS8604 // Mögliches Nullverweisargument.
            string database = WordCutter.RemoveUntilWord(connetions.MySqlConStr, "Database=", 9);
#if DEBUG
            database = WordCutter.RemoveUntilWord(connetions.MySqlConStrDebug, "Database=", 9);
#pragma warning restore CS8604 // Mögliches Nullverweisargument.
#endif
            database = WordCutter.RemoveAfterWord(database, "; Uid", 0);

            string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" +
                                $"USE `{database}`;" +
                                $"CREATE TABLE IF NOT EXISTS `{guildsId}` (" +
                                "`MemberId` BIGINT NOT NULL," +
                                "`OnlineTicks` INT NOT NULL," +
                                "`OnlineTime` varchar(69) NOT NULL," +
                                "PRIMARY KEY (MemberId)" +
                                ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
    }
}