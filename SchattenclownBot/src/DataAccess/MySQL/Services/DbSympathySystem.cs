using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.DataAccess.CSV;
using SchattenclownBot.Models;
using SchattenclownBot.Utils;

namespace SchattenclownBot.DataAccess.MySQL.Services
{
    public static class DbSympathySystem
    {
        public static List<SympathySystem> ReadAll(ulong guildId)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_votes`";

            List<SympathySystem> sympathySystemList = new();
            MySqlConnection mySqlConnection = DbConnection.OpenDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                SympathySystem sympathySystemObj = new()
                {
                            VotingUserId = mySqlDataReader.GetUInt64("VotingUserID"),
                            VotedUserId = mySqlDataReader.GetUInt64("VotedUserID"),
                            GuildId = mySqlDataReader.GetUInt64("GuildID"),
                            VoteRating = mySqlDataReader.GetInt32("VoteRating")
                };

                sympathySystemList.Add(sympathySystemObj);
            }

            DbConnection.CloseDb(mySqlConnection);
            return sympathySystemList;
        }

        public static void Add(SympathySystem sympathySystem)
        {
            string sqlCommand = $"INSERT INTO `{sympathySystem.GuildId}_votes` (VotingUserID, VotedUserID, GuildID, VoteRating)" + $"VALUES({sympathySystem.VotingUserId}, {sympathySystem.VotedUserId}, {sympathySystem.GuildId}, {sympathySystem.VoteRating})";

            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        public static void Change(SympathySystem sympathySystem)
        {
            string sqlCommand = $"UPDATE `{sympathySystem.GuildId}_votes` SET VoteRating={sympathySystem.VoteRating} WHERE VotingUserID={sympathySystem.VotingUserId} AND VotedUserID={sympathySystem.VotedUserId}";
            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        public static void CreateTable_SympathySystem(ulong guildId)
        {
            Connections connections = CsvConnections.ReadAll();

#if DEBUG
            string database = StringCutter.RmUntil(connections.MySqlConStrDebug, "Database=", 9);
#else
            string database = StringCutter.RmUntil(connections.MySqlConStr, "Database=", 9);
#endif
            database = StringCutter.RmAfter(database, "; Uid", 0);

            string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + $"CREATE TABLE IF NOT EXISTS `{guildId}_votes` (" + "`VoteTableID` INT NOT NULL AUTO_INCREMENT," + "`VotingUserID` BIGINT NOT NULL," + "`VotedUserID` BIGINT NOT NULL," + "`GuildID` BIGINT NOT NULL," + "`VoteRating` INT NOT NULL," + "PRIMARY KEY (VoteTableID)" + ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        public static List<RoleInfoSympathySystem> ReadAllRoleInfo(ulong guildId)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_roleinfo`";

            List<RoleInfoSympathySystem> roleInfoSympathySystemList = new();
            MySqlConnection mySqlConnection = DbConnection.OpenDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                RoleInfoSympathySystem roleInfoSympathySystem = new();

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
                }

                roleInfoSympathySystemList.Add(roleInfoSympathySystem);
            }

            DbConnection.CloseDb(mySqlConnection);
            return roleInfoSympathySystemList;
        }

        public static void AddRoleInfo(SympathySystem sympathySystem)
        {
            string sqlCommand = $"INSERT INTO `{sympathySystem.GuildId}_roleinfo` (GuildRoleID, RatingValue, GuildID)";

            if (sympathySystem.RoleInfo.RatingOne != 0)
            {
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingOne}, 1, {sympathySystem.GuildId})";
            }
            else if (sympathySystem.RoleInfo.RatingTwo != 0)
            {
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingTwo}, 2, {sympathySystem.GuildId})";
            }
            else if (sympathySystem.RoleInfo.RatingThree != 0)
            {
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingThree}, 3, {sympathySystem.GuildId})";
            }
            else if (sympathySystem.RoleInfo.RatingFour != 0)
            {
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingFour}, 4, {sympathySystem.GuildId})";
            }
            else if (sympathySystem.RoleInfo.RatingFive != 0)
            {
                sqlCommand += $"VALUES({sympathySystem.RoleInfo.RatingFive}, 5, {sympathySystem.GuildId})";
            }

            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        public static void ChangeRoleInfo(SympathySystem sympathySystem)
        {
            string sqlCommand = $"UPDATE `{sympathySystem.GuildId}_roleinfo` SET GuildRoleID=";

            if (sympathySystem.RoleInfo.RatingOne != 0)
            {
                sqlCommand += $"{sympathySystem.RoleInfo.RatingOne} WHERE RatingValue=1";
            }
            else if (sympathySystem.RoleInfo.RatingTwo != 0)
            {
                sqlCommand += $"{sympathySystem.RoleInfo.RatingTwo} WHERE RatingValue=2";
            }
            else if (sympathySystem.RoleInfo.RatingThree != 0)
            {
                sqlCommand += $"{sympathySystem.RoleInfo.RatingThree} WHERE RatingValue=3";
            }
            else if (sympathySystem.RoleInfo.RatingFour != 0)
            {
                sqlCommand += $"{sympathySystem.RoleInfo.RatingFour} WHERE RatingValue=4";
            }
            else if (sympathySystem.RoleInfo.RatingFive != 0)
            {
                sqlCommand += $"{sympathySystem.RoleInfo.RatingFive} WHERE RatingValue=5";
            }

            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        public static bool CheckRoleInfoExists(ulong guildId, int ratingValue)
        {
            string sqlCommand = $"SELECT * FROM `{guildId}_roleinfo` WHERE RatingValue=RatingValue";

            MySqlConnection mySqlConnection = DbConnection.OpenDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sqlCommand, mySqlConnection);

            while (mySqlDataReader.Read())
            {
                if (mySqlDataReader.GetInt32("RatingValue") == ratingValue)
                {
                    return true;
                }
            }

            DbConnection.CloseDb(mySqlConnection);

            return false;
        }

        public static void CreateTable_RoleInfoSympathySystem(ulong guildsId)
        {
            Connections connections = CsvConnections.ReadAll();

#if DEBUG
            string database = StringCutter.RmUntil(connections.MySqlConStrDebug, "Database=", 9);
#else
            string database = StringCutter.RmUntil(connections.MySqlConStr, "Database=", 9);
#endif
            database = StringCutter.RmAfter(database, "; Uid", 0);

            string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + $"CREATE TABLE IF NOT EXISTS `{guildsId}_roleinfo` (" + "`RoleInfoID` INT NOT NULL AUTO_INCREMENT," + "`GuildRoleID` BIGINT NOT NULL," + "`RatingValue` INT NOT NULL," + "`GuildID` BIGINT NOT NULL," + "PRIMARY KEY (RoleInfoID)" + ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

            DbConnection.ExecuteNonQuery(sqlCommand);
        }

        public static int GetUserRatings(ulong guildId, ulong votedUserId, int voteRating)
        {
            string sqlCommand = "SELECT count(*)" + $"FROM `{guildId}_votes` " + $"WHERE VotedUserID={votedUserId} AND VoteRating={voteRating}";

            MySqlConnection mySqlConnection = DbConnection.OpenDb();
            int returnNumber = DbConnection.ExecuteScalarCount(sqlCommand, mySqlConnection);

            DbConnection.CloseDb(mySqlConnection);

            return returnNumber;
        }
    }
}