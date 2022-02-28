using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using SchattenclownBot.HelpClasses;

namespace SchattenclownBot.Model.Persistence
{
    public static class DB_DcSympathieSystem
    {
        public static List<DcUserLevelSystem> Read(ulong guildId)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_votes`";
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
            return null;
        }
        public static void Add(ulong guildId, DcUserLevelSystem dcLevelSystem)
        {
            string sqlCommand = $"INSERT INTO `{guildId}_votes` (MemberId, OnlineTicks, OnlineTime) " +
                                $"VALUES ({dcLevelSystem.MemberId}, {dcLevelSystem.OnlineTicks}, '{dcLevelSystem.OnlineTime}')";
            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void Change(ulong guildId, DcUserLevelSystem dcLevelSystem)
        {
            string sqlCommand = $"UPDATE `{guildId}_votes` SET OnlineTicks={dcLevelSystem.OnlineTicks} WHERE MemberId={dcLevelSystem.MemberId}";
            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void CreateTable_DcSympathieSystem(ulong guildsId)
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
                                $"CREATE TABLE IF NOT EXISTS `{guildsId}_votes` (" +
                                "`VoteTableID` INT NOT NULL AUTO_INCREMENT," +
                                "`VotingUserID` BIGINT NOT NULL," +
                                "`VotedUserID` BIGINT NOT NULL," +
                                "`VoteRating` INT NOT NULL," +
                                "PRIMARY KEY (VoteTableID)" +
                                ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
    }
}
