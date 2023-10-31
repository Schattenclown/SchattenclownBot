using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SchattenclownBot.Integrations.Discord.Main;
using Serilog;

namespace SchattenclownBot.Utils
{
    public static class ConsoleLogger
    {
        public static void WriteLine(string message, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RmAfter(callerNameSpace, "+", 0);
            Log.Information($"[{callerNameSpace}.{methodName}]: " + "{message}", $"{message}");
        }

        public static void WriteLine(string message, bool error, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RmAfter(callerNameSpace, "+", 0);
            DiscordBot.ErrorLogger.Information($"[{callerNameSpace}.{methodName}]: " + "{message}", $"{message}");
        }

        public static void WriteLine(Exception exception, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RmAfter(callerNameSpace, "+", 0);
            Log.Fatal($"[{callerNameSpace}.{methodName}] {exception.Message}\n{exception.StackTrace}");
        }

        public static void WriteLine(string objectInfo, Exception exception, [CallerMemberName] string methodName = "")
        {
            string callerNameSpace = new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.UnderlyingSystemType.ReflectedType?.FullName ?? new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.FullName ?? "Unknown";
            callerNameSpace = StringCutter.RmAfter(callerNameSpace, "+", 0);
            DiscordBot.ErrorLogger.Fatal($"[{callerNameSpace}.{methodName}] {objectInfo}\n{exception.Message}\n{exception.StackTrace}");
        }
    }
}