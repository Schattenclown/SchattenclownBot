using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.SystemConsole.Themes;

namespace SchattenclownBot.Utils
{
    public static class CustomLogger
    {
        private static readonly List<Logger> Loggers = new();

        public static void Create()
        {
            foreach (Enum consoleColor in Enum.GetValues(typeof(ConsoleColor)))
            {
                Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle> consoleThemeStyle = new()
                {
                            {
                                        ConsoleThemeStyle.Text, new SystemConsoleThemeStyle
                                        {
                                                    Foreground = consoleColor as ConsoleColor?
                                        }
                            },
                            {
                                        ConsoleThemeStyle.String, new SystemConsoleThemeStyle
                                        {
                                                    Foreground = ConsoleColor.White
                                        }
                            }
                };

                Loggers.Add(new LoggerConfiguration().WriteTo.Console(theme: new SystemConsoleTheme(consoleThemeStyle)).CreateLogger());
            }
        }

        public static void ToConsole(string message, ConsoleColor consoleColor, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RemoveAfter(callerNameSpace, "+", 0);
            string callerNameSpaceMethodName = $"[{callerNameSpace}.{methodName}]: ";
            Loggers[Convert.ToInt32(consoleColor)].Information(callerNameSpaceMethodName + "{message}", message);
        }

        public static void Red(Exception exception, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RemoveAfter(callerNameSpace, "+", 0);
            Loggers[5].Error($"[{callerNameSpace}.{methodName}]:\n" + "Exception Message:\n{Message}\nException StackTrace:\n{StackTrace}", exception.Message, exception.StackTrace);
        }

        public static void Red(string objectInfo, Exception exception, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RemoveAfter(callerNameSpace, "+", 0);
            Loggers[5].Error($"[{callerNameSpace}.{methodName}]:\n{objectInfo}\n" + "Exception Message:\n{Message}\nException StackTrace:\n{StackTrace}", exception.Message, exception.StackTrace);
        }
    }
}