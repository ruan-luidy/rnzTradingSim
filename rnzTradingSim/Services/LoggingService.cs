using System;
using System.IO;

namespace rnzTradingSim.Services
{
  public enum LogLevel
  {
    Debug,
    Info,
    Warning,
    Error
  }

  public static class LoggingService
  {
    private static readonly string _logDirectory;
    private static readonly string _logFilePath;
    private static readonly object _lockObject = new();

    static LoggingService()
    {
      _logDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RnzTradingSim", "Logs");

      Directory.CreateDirectory(_logDirectory);

      _logFilePath = Path.Combine(_logDirectory, $"app_{DateTime.Now:yyyy-MM-dd}.log");
    }

    public static void Log(LogLevel level, string message, Exception? exception = null)
    {
      try
      {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var logEntry = $"[{timestamp}] [{level}] {message}";

        if (exception != null)
        {
          logEntry += $"\nException: {exception.Message}\nStackTrace: {exception.StackTrace}";
        }

        lock (_lockObject)
        {
          File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }

        // Also output to debug console
        System.Diagnostics.Debug.WriteLine(logEntry);
      }
      catch
      {
        // Falha silenciosa para evitar loops infinitos
      }
    }

    public static void Debug(string message) => Log(LogLevel.Debug, message);
    public static void Info(string message) => Log(LogLevel.Info, message);
    public static void Warning(string message) => Log(LogLevel.Warning, message);
    public static void Error(string message, Exception? exception = null) => Log(LogLevel.Error, message, exception);

    public static void CleanupOldLogs(int daysToKeep = 7)
    {
      try
      {
        var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
        var logFiles = Directory.GetFiles(_logDirectory, "app_*.log");

        foreach (var file in logFiles)
        {
          var fileInfo = new FileInfo(file);
          if (fileInfo.CreationTime < cutoffDate)
          {
            File.Delete(file);
            Debug($"Deleted old log file: {file}");
          }
        }
      }
      catch (Exception ex)
      {
        Error("Failed to cleanup old log files", ex);
      }
    }
  }
}