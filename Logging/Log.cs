namespace FantasAIFootball.Logging;

public enum LogLevel
{
    Debug,
    Info,
    Warn,
    Error
}

public static class Log
{
    public static LogLevel MinLevel { get; set; } = LogLevel.Info;

    public static void Debug(string message) => Write(LogLevel.Debug, message);

    public static void Info(string message) => Write(LogLevel.Info, message);

    public static void Warn(string message) => Write(LogLevel.Warn, message);

    public static void Error(string message) => Write(LogLevel.Error, message);

    public static void Error(string message, Exception exception)
        => Write(LogLevel.Error, message + Environment.NewLine + exception);

    private static void Write(LogLevel level, string message)
    {
        if (level < MinLevel)
        {
            return;
        }

        var color = level switch
        {
            LogLevel.Debug => ConsoleColor.DarkGray,
            LogLevel.Warn => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.Gray
        };

        var prefix = level switch
        {
            LogLevel.Warn => "WARN - ",
            LogLevel.Error => "ERROR - ",
            _ => ""
        };

        Console.ForegroundColor = color;
        Console.WriteLine(prefix + message);
        Console.ResetColor();
    }
}
