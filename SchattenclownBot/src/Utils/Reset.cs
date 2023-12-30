using System;
using System.Diagnostics;
using System.Reflection;

namespace SchattenclownBot.Utils
{
    public class Reset
    {
        /// <summary>
        ///     Restarts the program.
        /// </summary>
        public void RestartProgram()
        {
            new CustomLogger().Information("SchattenclownBot rebooting!", ConsoleColor.DarkRed);
            // HandlerReader file path of current process 
            string filePath = Assembly.GetExecutingAssembly().Location;
            string newFilepath = "";
            //BotDLL.dll

            if (filePath.Contains("Debug"))
            {
                filePath = new StringCutter().RemoveAfter(filePath, "Debug", 0);
                newFilepath = filePath + @"Debug\net8.0\SchattenclownBot.exe";
            }
            else if (filePath.Contains("Release"))
            {
                filePath = new StringCutter().RemoveAfter(filePath, "Release", 0);
                newFilepath = filePath + @"Release\net8.0\SchattenclownBot.exe";
            }

            // Start program
            Process.Start(newFilepath);

            // For all Windows application but typically for Console app.
            Environment.Exit(0);
        }
    }
}