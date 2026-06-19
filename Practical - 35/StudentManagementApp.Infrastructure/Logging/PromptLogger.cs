using StudentManagementApp.Application.Interfaces;

namespace StudentManagementApp.Infrastructure.Logging;

/// <summary>
/// Appends every user-initiated action to Logs/prompts.txt.
/// Format: a "---" separator, timestamp, then the prompt text.
/// </summary>
public sealed class PromptLogger : IPromptLogger
{
    private readonly string _filePath;
    private static readonly Lock _fileLock = new();

    public PromptLogger(string filePath)
    {
        _filePath = filePath;

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);
    }

    public void Log(string prompt)
    {
        var entry =
            $"---{Environment.NewLine}" +
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]{Environment.NewLine}" +
            $"{prompt}{Environment.NewLine}";

        lock (_fileLock)
        {
            File.AppendAllText(_filePath, entry);
        }
    }
}
