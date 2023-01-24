using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace CosmosPeddler;

public static class Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public static readonly Action<ImmutableList<(string, LogLevel, DateTime)>>? OnLog;

    public static ImmutableList<(string, LogLevel, DateTime)> Logs => logs.ToImmutableList();
    private static readonly List<(string, LogLevel, DateTime)> logs = new();

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        logs.Add((message, level, DateTime.Now));
        OnLog?.Invoke(Logs);
        
        switch (level)
        {
            case LogLevel.Debug:
            case LogLevel.Info:
                GD.Print(message);
                break;
            
            case LogLevel.Warning:
                GD.PushWarning(message);
                break;

            case LogLevel.Error:
                GD.PushError(message);
                break;
        }
    }

    public static void Debug(string message) => Log(message, LogLevel.Debug);

    public static void Info(string message) => Log(message, LogLevel.Info);

    public static void Warning(string message) => Log(message, LogLevel.Warning);

    public static void Error(string message) => Log(message, LogLevel.Error);
}