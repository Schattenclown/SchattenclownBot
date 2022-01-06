using MySql.Data.MySqlClient;
using SchattenclownBot.Model.Objects;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SchattenclownBot.HelpClasses;

namespace SchattenclownBot.Model.Persistence.Connection
{
    class DB_Connection
    {
        private static string token = "";
        private static int virgin = 0;
        public static void SetDB()
        {
            Connections connections = Connections.GetConnections();
            token = connections.MySqlConStr;
#if DEBUG
            token = connections.MySqlConStrDebug;
#endif
        }
        public static MySqlConnection OpenDB()
        {
            if (virgin == 0)
                SetDB(); 
            virgin = 69;

            MySqlConnection connection = new MySqlConnection(token);
            do
            {
                try
                {
                    connection.Open();
                }
                catch
                {
                    /*Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
                    Center("DB IS DEAD");
                    Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
                    RestartProgram();
                    throw new Exception("DB DeaD");*/
                }
            } while (connection == null);

            return connection;
        }
        public static void CloseDB(MySqlConnection connection)
        {
            connection.Close();
        }
        public static void ExecuteNonQuery(string sql)
        {
            MySqlConnection connection = OpenDB();
            MySqlCommand sqlCommand = new MySqlCommand(sql, connection);
            int ret = sqlCommand.ExecuteNonQuery();
            if (ret != -1)
                Console.WriteLine("DEBUG: DB -1");
            CloseDB(connection);
        }

        public static MySqlDataReader ExecuteReader(String sql, MySqlConnection connection)
        {
            MySqlCommand sqlCommand = new MySqlCommand(sql, connection);
            try
            {
                MySqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                return sqlDataReader;
            }
            catch
            {
                /*Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
                Center("DB IS DEAD");
                Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
                RestartProgram();*/
                throw new Exception("DB DeaD");
            }
        }

        /// <summary>
        /// Centers the console.
        /// </summary>
        /// <param name="s">The text.</param>
        static void Center(string s)
        {
            try
            {
                Console.Write("██");
                Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
                Console.Write(s);
                Console.SetCursorPosition((Console.WindowWidth - 4), Console.CursorTop);
                Console.WriteLine("██");
            }
            catch (Exception)
            {
                s = "Console to smoll EXC";
                Console.SetCursorPosition((Console.WindowWidth - s.Length) / 2, Console.CursorTop);
                Console.Write(s);
                Console.SetCursorPosition((Console.WindowWidth - 4), Console.CursorTop);
                Console.WriteLine("██");
            }
        }

        /// <summary>
        /// Restarts the program.
        /// </summary>
        private static void RestartProgram()
        {

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
            Center(" ");
            Center(@"██████╗ ███████╗███████╗████████╗ █████╗ ██████╗ ████████╗██╗███╗   ██╗ ██████╗ ");
            Center(@"██╔══██╗██╔════╝██╔════╝╚══██╔══╝██╔══██╗██╔══██╗╚══██╔══╝██║████╗  ██║██╔════╝ ");
            Center(@"██████╔╝█████╗  ███████╗   ██║   ███████║██████╔╝   ██║   ██║██╔██╗ ██║██║  ███╗");
            Center(@"██╔══██╗██╔══╝  ╚════██║   ██║   ██╔══██║██╔══██╗   ██║   ██║██║╚██╗██║██║   ██║");
            Center(@"██║  ██║███████╗███████║   ██║   ██║  ██║██║  ██║   ██║   ██║██║ ╚████║╚██████╔╝");
            Center(@"╚═╝  ╚═╝╚══════╝╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝╚═╝  ╚═══╝ ╚═════╝ ");
            Center(" ");
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");
            Center("DB IS DEAD");
            Console.WriteLine($"{"".PadRight(Console.WindowWidth - 2, '█')}");

            // Get file path of current process 
            var filePath = Assembly.GetExecutingAssembly().Location;
            var newFilepath = "";
            //BotDLL.dll

            if (filePath.Contains("Debug"))
            {
                filePath = WordCutter.RemoveAfterWord(filePath, "Debug", 0);
                newFilepath = filePath + "Debug\\netcoreapp3.1\\SchattenclownBot.exe";
            }
            else if (filePath.Contains("Release"))
            {
                filePath = WordCutter.RemoveAfterWord(filePath, "Release", 0);
                newFilepath = filePath + "Release\\netcoreapp3.1\\SchattenclownBot.exe";
            }

            Console.WriteLine("Before 120 secound sleep");
            Thread.Sleep(1000 * 60);
            Console.WriteLine("After 120 secound sleep");
            // Start program
            Process.Start(newFilepath);

            // For all Windows application but typically for Console app.
            Environment.Exit(0);
        }
    }
}
