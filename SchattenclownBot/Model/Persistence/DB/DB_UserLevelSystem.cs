using System.Collections.Generic;
using MySql.Data.MySqlClient;
using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence.DB;

public static class DB_UserLevelSystem
{
   public static List<UserLevelSystem> Read(ulong guildId)
   {
      string sqlCommand = $"SELECT * FROM `{guildId}_levelSystem`";
      List<UserLevelSystem> userLevelSystemList = new();
      MySqlConnection mySqlConnection = DB_Connection.OpenDB();
      MySqlDataReader mySqlDataReader = DB_Connection.ExecuteReader(sqlCommand, mySqlConnection);

      while (mySqlDataReader.Read())
      {
         UserLevelSystem userLevelSystemObj = new()
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
      string sqlCommand = $"INSERT INTO `{guildId}_levelSystem` (MemberId, OnlineTicks, OnlineTime) " + $"VALUES ({userLevelSystem.MemberId}, {userLevelSystem.OnlineTicks}, '{userLevelSystem.OnlineTime}')";
      DB_Connection.ExecuteNonQuery(sqlCommand);
   }

   public static void Change(ulong guildId, UserLevelSystem userLevelSystem)
   {
      string sqlCommand = $"UPDATE `{guildId}_levelSystem` SET OnlineTicks={userLevelSystem.OnlineTicks} WHERE MemberId={userLevelSystem.MemberId}";
      DB_Connection.ExecuteNonQuery(sqlCommand);
   }

   public static void CreateTable_UserLevelSystem(ulong guildId)
   {
      Connections connections = CSV_Connections.ReadAll();

#if DEBUG
      string database = StringCutter.RmUntil(connections.MySqlConStrDebug, "Database=", 9);
#else
      string database = StringCutter.RmUntil(connections.MySqlConStr, "Database=", 9);
#endif
      database = StringCutter.RmAfter(database, "; Uid", 0);

      string sqlCommand = $"CREATE DATABASE IF NOT EXISTS `{database}`;" + $"USE `{database}`;" + $"CREATE TABLE IF NOT EXISTS `{guildId}_levelSystem` (" + "`MemberId` BIGINT NOT NULL," + "`OnlineTicks` INT NOT NULL," + "`OnlineTime` varchar(69) NOT NULL," + "PRIMARY KEY (MemberId)" + ") ENGINE=InnoDB DEFAULT CHARSET=latin1;";

      DB_Connection.ExecuteNonQuery(sqlCommand);
   }
}