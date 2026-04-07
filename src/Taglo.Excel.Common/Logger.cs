using System.Diagnostics;

namespace Taglo.Excel.Common;

/// <summary>
///     Simple file logger for crash diagnostics. Writes to %LOCALAPPDATA%\{appName}\logs\{appName}.log.
///     All I/O failures are silently ignored — the logger must never itself cause a crash.
/// </summary>
public static class Logger
{
    private const long MaxFileSize = 1_048_576; // 1 MB

    private static readonly object Lock = new();
    private static string? _logFilePath;

    /// <summary>
    ///     Initializes the logger. Truncates the log file if it exceeds 1 MB.
    ///     Call once during AddIn.AutoOpen.
    /// </summary>
    /// <param name="appName">
    ///     Application name used for the log directory and filename.
    ///     e.g. "FormulaBoss" → %LOCALAPPDATA%\FormulaBoss\logs\FormulaBoss.log
    /// </param>
    public static void Initialize(string appName)
    {
        try
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName,
                "logs");
            Directory.CreateDirectory(dir);
            _logFilePath = Path.Combine(dir, $"{appName}.log");

            // Truncate if over 1 MB
            if (File.Exists(_logFilePath) && new FileInfo(_logFilePath).Length > MaxFileSize)
            {
                File.WriteAllText(_logFilePath, "");
            }
        }
        catch
        {
            // Silently ignore — logging is best-effort
        }
    }

    public static void Info(string message)
    {
        Write("INFO", message);
    }

    public static void Error(string source, Exception ex)
    {
        Write("ERROR", $"{source}: {ex}");
    }

    /// <summary>
    ///     Resets the logger state. Intended for testing only.
    /// </summary>
    internal static void Reset()
    {
        lock (Lock)
        {
            _logFilePath = null;
        }
    }

    /// <summary>
    ///     Gets the current log file path, or null if not initialized.
    ///     Intended for testing only.
    /// </summary>
    internal static string? LogFilePath => _logFilePath;

    private static void Write(string level, string message)
    {
        // Always write to Debug output
        Debug.WriteLine($"[{level}] {message}");

        if (_logFilePath == null)
        {
            return;
        }

        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
            lock (Lock)
            {
                File.AppendAllText(_logFilePath, line);
            }
        }
        catch
        {
            // Silently ignore — logging is best-effort
        }
    }
}
