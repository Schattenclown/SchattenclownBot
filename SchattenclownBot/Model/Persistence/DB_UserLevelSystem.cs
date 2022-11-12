// Copyright (c) Schattenclown

using System.Collections.Generic;

using SchattenclownBot.Model.HelpClasses;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence;

public static class DbUserLevelSystem
{
	public static List<UserLevelSystem> Read(ulong guildId)
	{
		var sqlCommand = $"SELECT * FROM `{guildId}_levelSystem`";
		List<UserLevelSystem> userLevelSystemList = new();
		var mySqlConnection = DbConnection.OpenDb();
		var mySqlDataReader = DbConnection.ExecuteReader(sqlCommand, mySqlConnection);

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
		var sqlCommand = $"INSERT INTO `{guildId}_levelSystem` (MemberId, OnlineTicks, OnlineTime) " +
						 $"VALUES ({userLevelSystem.MemberId}, {userLevelSystem.OnlineTicks}, '{userLevelSystem.OnlineTime}')";
		DbConnection.ExecuteNonQuery(sqlCommand);
	}
	public static void Change(ulong guildId, UserLevelSystem userLevelSystem)
	{
		var sqlCommand = $"UPDATE `{guildId}_levelSystem` SET OnlineTicks={userLevelSystem.OnlineTicks} WHERE MemberId={userLevelSystem.MemberId}";
		DbConnection.ExecuteNonQuery(sqlCommand);
	}
	public static void CreateTable_UserLevelSystem(ulong guildId)
	{
		var connections = CsvConnections.ReadAll();

#if DEBUG
		var database = StringCutter.RemoveUntilWord(connections.MySqlConStrDebug, "Database=", 9);
#else
            string database = StringCutter.RemoveUntilWord(connections.MySqlConStr, "Database=", 9);
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

		DbConnection.ExecuteNonQuery(sqlCommand);
	}
}
