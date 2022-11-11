using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using System;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Persistence
{
    internal class DB_API
    {
        internal static List<API_Object> GET()
        {
            string sql = "SELECT * FROM PUT";

            List<API_Object> aPI_ObjectList = new();
            MySqlConnection mySqlConnection = DbConnection.OpenAPIDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sql, mySqlConnection);

            if (mySqlDataReader != null)
            {
                while (mySqlDataReader.Read())
                {
                    API_Object aPI_Object = new()
                    {
                        idPUT = mySqlDataReader.GetInt32("idPUT"),
                        command = mySqlDataReader.GetString("command"),
                        RequestTimeStamp = mySqlDataReader.GetDateTime("RequestTimeStamp"),
                        RequestSecret = mySqlDataReader.GetUInt64("RequestSecret")
                    };

                    aPI_ObjectList.Add(aPI_Object);
                }
            }

            DbConnection.CloseDb(mySqlConnection);
            return aPI_ObjectList;

        }
        internal static void DELETE(int idPUT)
        {
            string sql = $"DELETE FROM `db_SelfApi`.`PUT` WHERE (`idPUT` = '{idPUT}') and (`Command` = 'Test')";
            DbConnection.ExecuteNonQueryAPI(sql);
        }
    }
}

