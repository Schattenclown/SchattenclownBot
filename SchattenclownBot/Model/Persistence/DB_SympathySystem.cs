using System;
using System.Collections.Generic;
using System.Text;

using MySql.Data.MySqlClient;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence
{
    public static class DB_SympathySystem
    {
        public static List<SympathySystem> ReadAll(ulong guildId)
        {
            var sqlCommand = $"SELECT * FROM `{guildId}_votes`";

            var sympathySystemList = new List<SympathySystem>();
            var mySqlConnection = DB_Connection.OpenDB();
            var mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                var sympathySystemObj = new SympathySystem()
                {
                    VotingUserID = mySqlDataReader.GetUInt64("VotingUserID"),
                    VotedUserID = mySqlDataReader.GetUInt64("VotedUserID"),
                    GuildID = mySqlDataReader.GetUInt64("GuildID"),
                    VoteRating = mySqlDataReader.GetInt32("VoteRating")
                };

                sympathySystemList.Add(sympathySystemObj);
            }

            DB_Connection.CloseDB(mySqlConnection);
            return sympathySystemList;
        }
        public static void Add(SympathySystem sympathySystem)
        {
            var sqlCommand = $"INSERT INTO `{sympathySystem.GuildID}_votes` (VotingUserID, VotedUserID, GuildID, VoteRating)" +
                             $"VALUES({sympathySystem.VotingUserID}, {sympathySystem.VotedUserID}, {sympathySystem.GuildID}, {sympathySystem.VoteRating})";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void Change(SympathySystem sympathySystem)
        {
            var sqlCommand = $"UPDATE `{sympathySystem.GuildID}_votes` SET VoteRating={sympathySystem.VoteRating} WHERE VotingUserID={sympathySystem.VotingUserID} AND VotedUserID={sympathySystem.VotedUserID}";
            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void CreateTable_SympathySystem(ulong guildId)
        {
            var connetions = CSV_Connections.ReadAll();

            var database = StringCutter.RemoveUntilWord(connetions.MySqlConStr, "Database=", 9);
#if DEBUG
            database = StringCutter.RemoveUntilWord(connetions.MySqlConStrDebug, "Database=", 9);
#endif
            database = StringCutter.RemoveAfterWord(database, "; Uid", 0);

            var sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" +
                             $"USE `{database}`;" +
                             $"CREATE TABLE IF NOT EXISTS `{guildId}_votes` (" +
                             "`VoteTableID` INT NOT NULL AUTO_INCREMENT," +
                             "`VotingUserID` BIGINT NOT NULL," +
                             "`VotedUserID` BIGINT NOT NULL," +
                             "`GuildID` BIGINT NOT NULL," +
                             "`VoteRating` INT NOT NULL," +
                             "PRIMARY KEY (VoteTableID)" +
                             ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static List<RoleInfoSympathySystem> ReadAllRoleInfo(ulong guildId)
        {
            var sqlCommand = $"SELECT * FROM `{guildId}_roleinfo`";

            var roleInfoSympathySystemList = new List<RoleInfoSympathySystem>();
            var mySqlConnection = DB_Connection.OpenDB();
            var mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                var roleInfoSympathySystem = new RoleInfoSympathySystem();

                switch (mySqlDataReader.GetInt32("RatingValue"))
                {
                    case 1:
                        roleInfoSympathySystem.RatingOne = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    case 2:
                        roleInfoSympathySystem.RatingTwo = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    case 3:
                        roleInfoSympathySystem.RatingThree = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    case 4:
                        roleInfoSympathySystem.RatingFour = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    case 5:
                        roleInfoSympathySystem.RatingFive = mySqlDataReader.GetUInt64("GuildRoleID");
                        break;
                    default:
                        break;
                }

                roleInfoSympathySystemList.Add(roleInfoSympathySystem);
            }

            DB_Connection.CloseDB(mySqlConnection);
            return roleInfoSympathySystemList;
        }
        public static void AddRoleInfo(SympathySystem sympathySystem)
        {
            var sqlCommand = $"INSERT INTO `{sympathySystem.GuildID}_roleinfo` (GuildRoleID, RatingValue, GuildID)";

            if (sympathySystem.RoleInfo.RatingOne != 0)
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingOne}, 1, {sympathySystem.GuildID})";
            else if (sympathySystem.RoleInfo.RatingTwo != 0)
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingTwo}, 2, {sympathySystem.GuildID})";
            else if (sympathySystem.RoleInfo.RatingThree != 0)
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingThree}, 3, {sympathySystem.GuildID})";
            else if (sympathySystem.RoleInfo.RatingFour != 0)
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingFour}, 4, {sympathySystem.GuildID})";
            else if (sympathySystem.RoleInfo.RatingFive != 0)
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingFive}, 5, {sympathySystem.GuildID})";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static void ChangeRoleInfo(SympathySystem sympathySystem)
        {
            var sqlCommand = $"UPDATE `{sympathySystem.GuildID}_roleinfo` SET GuildRoleID=";

            if (sympathySystem.RoleInfo.RatingOne != 0)
                sqlCommand += $"{sympathySystem.RoleInfo.RatingOne} WHERE RatingValue=1";
            else if (sympathySystem.RoleInfo.RatingTwo != 0)
                sqlCommand += $"{sympathySystem.RoleInfo.RatingTwo} WHERE RatingValue=2";
            else if (sympathySystem.RoleInfo.RatingThree != 0)
                sqlCommand += $"{sympathySystem.RoleInfo.RatingThree} WHERE RatingValue=3";
            else if (sympathySystem.RoleInfo.RatingFour != 0)
                sqlCommand += $"{sympathySystem.RoleInfo.RatingFour} WHERE RatingValue=4";
            else if (sympathySystem.RoleInfo.RatingFive != 0)
                sqlCommand += $"{sympathySystem.RoleInfo.RatingFive} WHERE RatingValue=5";

            DB_Connection.ExecuteNonQuery(sqlCommand);
        }
        public static bool CheckRoleInfoExists(ulong guildId,int RatingValue)
        {
            var sqlCommand = $"SELECT * FROM `{guildId}_roleinfo` WHERE RatingValue=RatingValue";

            var mySqlConnection = DB_Connection.OpenDB();
            var mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                if(mySqlDataReader.GetInt32("RatingValue") == RatingValue)
                    return true;
            }

            DB_Connection.CloseDB(mySqlConnection);

            return false;
        }
        public static void CreateTable_RoleInfoSympathySystem(ulong guildsId)
        {
            var connetions = CSV_Connections.ReadAll();

            var database = StringCutter.RemoveUntilWord(connetions.MySqlConStr, "Database=", 9);
#if DEBUG
            database = StringCutter.RemoveUntilWord(connetions.MySqlConStrDebug, "Database=", 9);
#endif
            database = StringCutter.RemoveAfterWord(database, "; Uid", 0);

            var sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" +
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
            var sqlCommand = $"SELECT count(*)" +
                             $"FROM `{guildId}_votes` " +
                             $"WHERE VotedUserID={votedUserID} AND VoteRating={voteRating}";

            var mySqlConnection = DB_Connection.OpenDB();
            var returnnumber = DB_Connection.ExecuteScalarCount(sqlCommand, mySqlConnection);

            DB_Connection.CloseDB(mySqlConnection);

            return returnnumber;
        }
    }
}
