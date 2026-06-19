namespace StudentManagementApp.Application.Interfaces;

/// <summary>
/// Records every user-initiated action to a persistent prompt log.
/// </summary>
public interface IPromptLogger
{
    void Log(string prompt);
}
