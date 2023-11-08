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
                if (consoleColor as ConsoleColor? != ConsoleColor.DarkRed)
                {
                    Loggers.Add(new LoggerConfiguration().WriteTo.Console(theme: new SystemConsoleTheme(consoleThemeStyle)).MinimumLevel.Verbose().CreateLogger());
                }
                else
                {
                    Loggers.Add(new LoggerConfiguration().WriteTo.File("log/Error.log", rollingInterval: RollingInterval.Day).WriteTo.Console(theme: new SystemConsoleTheme(consoleThemeStyle)).MinimumLevel.Verbose().CreateLogger());
                }
            }
        }

        public static void Information(string message, ConsoleColor consoleColor, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RemoveAfter(callerNameSpace, "+", 0);
            string callerNameSpaceMethodName = $"[{callerNameSpace}.{methodName}]: ";
            Loggers[Convert.ToInt32(consoleColor)].Information(callerNameSpaceMethodName + "{message}", message);
        }

        public static void Debug(string message, ConsoleColor consoleColor, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RemoveAfter(callerNameSpace, "+", 0);
            string callerNameSpaceMethodName = $"[{callerNameSpace}.{methodName}]: ";
            Loggers[Convert.ToInt32(consoleColor)].Debug(callerNameSpaceMethodName + "{message}", message);
        }

        public static void Error(Exception exception, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RemoveAfter(callerNameSpace, "+", 0);
            Loggers[4].Error($"[{callerNameSpace}.{methodName}]:\n" + "Exception Message:\n{Message}\nException StackTrace:\n{StackTrace}", exception.Message, exception.StackTrace);
        }

        public static void Error(string objectInfo, Exception exception, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RemoveAfter(callerNameSpace, "+", 0);
            Loggers[4].Error($"[{callerNameSpace}.{methodName}]:\n" + "{objectInfo}\nException Message:\n{Message}\nException StackTrace:\n{StackTrace}", objectInfo, exception.Message, exception.StackTrace);
        }
    }
}