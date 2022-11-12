using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using SchattenclownBot.Model.Persistence.Connection;
using System.Collections.Generic;

namespace SchattenclownBot.Model.Persistence
{
    internal class DB_API
    {
        internal static List<API> GET()
        {
            string sql = "SELECT * FROM CommandRequests";

            List<API> aPIGETs = new();
            MySqlConnection mySqlConnection = DbConnection.OpenAPIDb();
            MySqlDataReader mySqlDataReader = DbConnection.ExecuteReader(sql, mySqlConnection);

            if (mySqlDataReader != null)
            {
                while (mySqlDataReader.Read())
                {
                    API aPI = new()
                    {
                        PUTiD = mySqlDataReader.GetInt32("idPUT"),
                        Command = mySqlDataReader.GetString("command"),
                        RequestTimeStamp = mySqlDataReader.GetDateTime("RequestTimeStamp"),
                        RequestSecret = mySqlDataReader.GetUInt64("RequestSecret")
                    };

                    aPIGETs.Add(aPI);
                }
            }

            DbConnection.CloseDb(mySqlConnection);
            return aPIGETs;
        }
        internal static void DELETE(int pUTiD)
        {
            string sql = $"DELETE FROM `db_SelfApi`.`CommandRequests` WHERE (`idPUT` = '{pUTiD}') and (`Command` = 'Test')";
            DbConnection.ExecuteNonQueryAPI(sql);
        }
    }
}

