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
        public static List<DcSympathieSystem> ReadAll(ulong guildId)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_votes`";

            List<DcSympathieSystem> dcSympathieSystemList = new List<DcSympathieSystem>();
            MySqlConnection mySqlConnection = DB_Connection.OpenDB();
            MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                DcSympathieSystem dcSympathieSystemObj = new DcSympathieSystem()
                {
                    VotingUserID = mySqlDataReader.GetUInt64("VotingUserID"),
                    VotedUserID = mySqlDataReader.GetUInt64("VotedUserID"),
                    GuildID = mySqlDataReader.GetUInt64("GuildID"),
                    VoteRating = mySqlDataReader.GetInt32("VoteRating")
                };

                dcSympathieSystemList.Add(dcSympathieSystemObj);
            }

            DB_Connection.CloseDB(mySqlConnection);
            return dcSympathieSystemList;
        }
        public static void Add(DcSympathieSystem dcSympathieSystem)
        {
            string sqlCommand = $"INSERT INTO `{dcSympathieSystem.GuildID}_votes` (VotingUserID, VotedUserID, GuildID, VoteRating)" +
                                $"VALUES({dcSympathieSystem.VotingUserID}, {dcSympathieSystem.VotedUserID}, {dcSympathieSystem.GuildID}, {dcSympathieSystem.VoteRating})";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void Change(DcSympathieSystem dcSympathieSystem)
        {
            string sqlCommand = $"UPDATE `{dcSympathieSystem.GuildID}_votes` SET VoteRating={dcSympathieSystem.VoteRating} WHERE VotingUserID={dcSympathieSystem.VotingUserID} AND VotedUserID={dcSympathieSystem.VotedUserID}";
            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void CreateTable_DcSympathieSystem(ulong guildsId)
        {
            Connections connetions = CSV_Connections.ReadAll();

            string database = WordCutter.RemoveUntilWord(connetions.MySqlConStr, "Database=", 9);
#if DEBUG
            database = WordCutter.RemoveUntilWord(connetions.MySqlConStrDebug, "Database=", 9);
#endif
            database = WordCutter.RemoveAfterWord(database, "; Uid", 0);

            string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" +
                                $"USE `{database}`;" +
                                $"CREATE TABLE IF NOT EXISTS `{guildsId}_votes` (" +
                                "`VoteTableID` INT NOT NULL AUTO_INCREMENT," +
                                "`VotingUserID` BIGINT NOT NULL," +
                                "`VotedUserID` BIGINT NOT NULL," +
                                "`GuildID` BIGINT NOT NULL," +
                                "`VoteRating` INT NOT NULL," +
                                "PRIMARY KEY (VoteTableID)" +
                                ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static List<DcSymSysRoleInfo> ReadAllRoleInfo(ulong guildId)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_roleinfo`";

            List<DcSymSysRoleInfo> dcSymSysRoleInfoList = new List<DcSymSysRoleInfo>();
            MySqlConnection mySqlConnection = DB_Connection.OpenDB();
            MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                DcSymSysRoleInfo dcSymSysRoleInfo = new DcSymSysRoleInfo();

                switch (mySqlDataReader.GetInt32("RatingValue"))
                {
                    case 1:
                        dcSymSysRoleInfo.RatingOne = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    case 2:
                        dcSymSysRoleInfo.RatingTwo = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    case 3:
                        dcSymSysRoleInfo.RatingThree = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    case 4:
                        dcSymSysRoleInfo.RatingFour = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    case 5:
                        dcSymSysRoleInfo.RatingFive = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    default:
                        break;
                }

                dcSymSysRoleInfoList.Add(dcSymSysRoleInfo);
            }

            DB_Connection.CloseDB(mySqlConnection);
            return dcSymSysRoleInfoList;
        }
        public static void AddRoleInfo(DcSympathieSystem dcSympathieSystem)
        {
            string sqlCommand = $"INSERT INTO `{dcSympathieSystem.GuildID}_roleinfo` (GuildRoleID, RatingValue, GuildID)";

            if (dcSympathieSystem.RoleInfo.RatingOne != 0)
                sqlCommand += $"VALUES({dcSympathieSystem.RoleInfo.RatingOne}, 1, {dcSympathieSystem.GuildID})";
            else if (dcSympathieSystem.RoleInfo.RatingTwo != 0)
                sqlCommand += $"VALUES({dcSympathieSystem.RoleInfo.RatingTwo}, 2, {dcSympathieSystem.GuildID})";
            else if (dcSympathieSystem.RoleInfo.RatingThree != 0)
                sqlCommand += $"VALUES({dcSympathieSystem.RoleInfo.RatingThree}, 3, {dcSympathieSystem.GuildID})";
            else if (dcSympathieSystem.RoleInfo.RatingFour != 0)
                sqlCommand += $"VALUES({dcSympathieSystem.RoleInfo.RatingFour}, 4, {dcSympathieSystem.GuildID})";
            else if (dcSympathieSystem.RoleInfo.RatingFive != 0)
                sqlCommand += $"VALUES({dcSympathieSystem.RoleInfo.RatingFive}, 5, {dcSympathieSystem.GuildID})";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void ChangeRoleInfo(DcSympathieSystem dcSympathieSystem)
        {
            string sqlCommand = $"UPDATE `{dcSympathieSystem.GuildID}_roleinfo` SET GuildRoleID=";

            if (dcSympathieSystem.RoleInfo.RatingOne != 0)
                sqlCommand += $"{dcSympathieSystem.RoleInfo.RatingOne} WHERE RatingValue=1";
            else if (dcSympathieSystem.RoleInfo.RatingTwo != 0)
                sqlCommand += $"{dcSympathieSystem.RoleInfo.RatingTwo} WHERE RatingValue=2";
            else if (dcSympathieSystem.RoleInfo.RatingThree != 0)
                sqlCommand += $"{dcSympathieSystem.RoleInfo.RatingThree} WHERE RatingValue=3";
            else if (dcSympathieSystem.RoleInfo.RatingFour != 0)
                sqlCommand += $"{dcSympathieSystem.RoleInfo.RatingFour} WHERE RatingValue=4";
            else if (dcSympathieSystem.RoleInfo.RatingFive != 0)
                sqlCommand += $"{dcSympathieSystem.RoleInfo.RatingFive} WHERE RatingValue=5";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static bool CheckRoleInfoExists(ulong guildId,int RatingValue)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_roleinfo` WHERE RatingValue=RatingValue";

            MySqlConnection mySqlConnection = DB_Connection.OpenDB();
            MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                if(mySqlDataReader.GetInt32("RatingValue") == RatingValue)
                    return true;
            }

            DB_Connection.CloseDB(mySqlConnection);

            return false;
        }
        public static void CreateTable_DcSympathieSystemRoleInfo(ulong guildsId)
        {
            Connections connetions = CSV_Connections.ReadAll();

            string database = WordCutter.RemoveUntilWord(connetions.MySqlConStr, "Database=", 9);
#if DEBUG
            database = WordCutter.RemoveUntilWord(connetions.MySqlConStrDebug, "Database=", 9);
#endif
            database = WordCutter.RemoveAfterWord(database, "; Uid", 0);

            string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" +
                                $"USE `{database}`;" +
                                $"CREATE TABLE IF NOT EXISTS `{guildsId}_roleinfo` (" +
                                "`RoleInfoID` INT NOT NULL AUTO_INCREMENT," +
                                "`GuildRoleID` BIGINT NOT NULL," +
                                "`RatingValue` INT NOT NULL," +
                                "`GuildID` BIGINT NOT NULL," +
                                "PRIMARY KEY (RoleInfoID)" +
                                ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static int GetUserRatings(ulong guildId, ulong votedUserID, int voteRating)
        {
            string sqlCommand = $"SELECT count(*)" +
                                $"FROM `{guildId}_votes` " +
                                $"WHERE VotedUserID={votedUserID} AND VoteRating={voteRating}";

            MySqlConnection mySqlConnection = DB_Connection.OpenDB();
            int returnnumber = DB_Connection.ExecuteScalarCount(sqlCommand, mySqlConnection);

            DB_Connection.CloseDB(mySqlConnection);

            return returnnumber;
        }
    }
}
