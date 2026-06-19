using Microsoft.Extensions.Logging;

namespace StudentManagementApp.Infrastructure.Logging;

/// <summary>
/// Writes log entries to a text file. Thread-safe via a shared lock.
/// Format: [yyyy-MM-dd HH:mm:ss] [LogLevel] Message
/// </summary>
public sealed class FileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _filePath;

    // Shared across all FileLogger instances so concurrent writes don't interleave lines.
    private static readonly Lock _fileLock = new();

    public FileLogger(string categoryName, string filePath)
    {
        _categoryName = categoryName;
        _filePath = filePath;

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] {message}";

        if (exception is not null)
            entry += $"{Environment.NewLine}  Exception: {exception}";

        lock (_fileLock)
        {
            File.AppendAllText(_filePath, entry + Environment.NewLine);
        }
    }
}
