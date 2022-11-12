// Copyright (c) Schattenclown

using System.Collections.Generic;

using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;

namespace SchattenclownBot.Model.Persistence;

internal class DB_API
{
	internal static List<API> GET()
	{
		var sql = "SELECT * FROM `db_SelfApi`.`CommandRequests`";

		List<API> aPIGETs = new();
		var mySqlConnection = DbConnection.OpenAPIDb();
		var mySqlDataReader = DbConnection.ExecuteReader(sql, mySqlConnection);

		if (mySqlDataReader != null)
		{
			while (mySqlDataReader.Read())
			{
				API aPI = new()
				{
					CommandRequestID = mySqlDataReader.GetInt32("CommandRequestID"),
					RequestDiscordUserId = mySqlDataReader.GetUInt64("RequestDiscordUserId"),
					RequestSecretKey = mySqlDataReader.GetUInt64("RequestSecretKey"),
					RequestTimeStamp = mySqlDataReader.GetDateTime("RequestTimeStamp"),
					RequesterIP = mySqlDataReader.GetString("RequesterIP"),
					Command = mySqlDataReader.GetString("Command")
				};

				aPIGETs.Add(aPI);
			}
		}

		DbConnection.CloseDb(mySqlConnection);
		return aPIGETs;
	}
	internal static void DELETE(int commandRequestID)
	{
		var sql = $"DELETE FROM `db_SelfApi`.`CommandRequests` WHERE (`CommandRequests`.`CommandRequestID` = '{commandRequestID}') AND (`CommandRequests`.`RequestSecretKey` = 42069)";
		DbConnection.ExecuteNonQueryAPI(sql);
	}
	public static void PUT(API aPI)
	{
		var sql = "INSERT INTO `db_SelfApi`.`CommandRequests` (`RequestDiscordUserId`, `RequestSecretKey`, `requesterIP`, `Command`, `Data`) " +
					 $"VALUES ({aPI.RequestDiscordUserId}, {aPI.RequestSecretKey}, '{aPI.RequesterIP}', '{aPI.Command}', '{aPI.Data}')";

		DbConnection.ExecuteNonQueryAPI(sql);
	}
}

