using Microsoft.Extensions.Logging;

namespace StudentManagementApp.Infrastructure.Logging;

/// <summary>
/// Creates <see cref="FileLogger"/> instances for the Microsoft.Extensions.Logging pipeline.
/// </summary>
public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _filePath;

    public FileLoggerProvider(string filePath)
    {
        _filePath = filePath;
    }

    public ILogger CreateLogger(string categoryName) =>
        new FileLogger(categoryName, _filePath);

    public void Dispose() { }
}
