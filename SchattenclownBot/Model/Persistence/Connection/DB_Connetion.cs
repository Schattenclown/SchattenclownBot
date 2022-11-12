// Copyright (c) Schattenclown

using System;
using System.Reflection;

using MySql.Data.MySqlClient;

using SchattenclownBot.Model.Discord.Main;
using SchattenclownBot.Model.HelpClasses;

namespace SchattenclownBot.Model.Persistence.Connection;

class DbConnection
{
	private static string s_token = "";
	public static MySqlConnection OpenDb()
	{
		s_token = Bot.Connections.MySqlConStr;
#if DEBUG
		s_token = Bot.Connections.MySqlConStrDebug;
#endif

		MySqlConnection connection = new(s_token);

		try
		{
			connection.Open();
		}
		catch (Exception ex)
		{
			CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
			Reset.RestartProgram();
			throw;
		}

		return connection;
	}
	public static MySqlConnection OpenAPIDb()
	{
		s_token = Bot.Connections.MySqlAPIConStr;
#if DEBUG
		s_token = Bot.Connections.MySqlAPIConStr;
#endif

		MySqlConnection connection = new(s_token);

		try
		{
			connection.Open();
		}
		catch (Exception ex)
		{
			CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
			Reset.RestartProgram();
			throw;
		}

		return connection;
	}
	public static void CloseDb(MySqlConnection connection) => connection.Close();
	public static void ExecuteNonQuery(string sql)
	{
		var connection = OpenDb();
		MySqlCommand sqlCommand = new(sql, connection);
		var ret = sqlCommand.ExecuteNonQuery();
		if (ret != -1)
		{
			CWLogger.Write($"{sqlCommand.CommandText}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		}
		CloseDb(connection);
	}
	public static void ExecuteNonQueryAPI(string sql)
	{
		var connection = OpenAPIDb();
		MySqlCommand sqlCommand = new(sql, connection);
		var ret = sqlCommand.ExecuteNonQuery();
		if (ret != -1)
		{
			CWLogger.Write($"{sqlCommand.CommandText}", MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Magenta);
		}
		CloseDb(connection);
	}
	public static MySqlDataReader ExecuteReader(string sql, MySqlConnection connection)
	{
		MySqlCommand sqlCommand = new(sql, connection);
		try
		{
			var sqlDataReader = sqlCommand.ExecuteReader();
			return sqlDataReader;
		}
		catch (Exception ex)
		{
			CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
			Reset.RestartProgram();
			throw;
		}
	}
	public static int ExecuteScalarCount(string sql, MySqlConnection connection)
	{
		MySqlCommand sqlCommand = new(sql, connection);
		try
		{
			var count = Convert.ToInt32(sqlCommand.ExecuteScalar());
			return count;
		}
		catch (Exception ex)
		{
			CWLogger.Write(ex, MethodBase.GetCurrentMethod()?.DeclaringType?.Name, ConsoleColor.Red);
			Reset.RestartProgram();
			throw;
		}
	}
}
