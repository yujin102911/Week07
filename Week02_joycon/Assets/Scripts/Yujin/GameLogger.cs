using System;
using System.IO;
using UnityEngine;

#region LogLevel Enum
public enum LogLevel
{
    DEBUG,
    INFO,
    WARNING,
    ERROR,
    CRITICAL
}
#endregion

public class GameLogger : Singleton<GameLogger>
{
    private string logFilePath;
    public string LogFilePath => logFilePath;

    [Header("Optional Custom File Name")]
    [SerializeField] private string customFileName = "";
    public LogLevel currentLogLevel = LogLevel.DEBUG;

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();

        string exeDir = Path.GetDirectoryName(Application.dataPath);
        string logDir = Path.Combine(exeDir, "log");
        Directory.CreateDirectory(logDir);

        string filePrefix = string.IsNullOrWhiteSpace(customFileName) ? "GameLog" : customFileName.Trim();
        string fileName = $"{filePrefix}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        logFilePath = Path.Combine(logDir, fileName);

        string header = "Timestamp, Level, Source, Message" + Environment.NewLine;
        File.AppendAllText(logFilePath, header);
        Application.logMessageReceived += HandleUnityLog;
        LogInfo(this, "=== Game Session Started ===");
    }
    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }
    #endregion

    #region Public Methods
    public void LogDebug(object source, string message) => Log(LogLevel.DEBUG, source, message);
    public void LogInfo(object source, string message) => Log(LogLevel.INFO, source, message);
    public void LogWarning(object source, string message) => Log(LogLevel.WARNING, source, message);
    public void LogCritical(object source, string message) => Log(LogLevel.CRITICAL, source, message);
    public void LogError(object source, string message, Exception e = null)
    {
        if (e != null) message += $"\nException: {e.Message}\n{e.StackTrace}";
        Log(LogLevel.ERROR, source, message);
    }
    #endregion

    #region Private Methods
    private void Log(LogLevel level, object source, string message)
    {
        if (level < currentLogLevel) return;

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string levelStr = level.ToString();
        string sourceName = (source != null) ? source.GetType().Name : "Unknown";

        string[] columns = { timestamp, levelStr, sourceName, message };

        string formatted = string.Join(",", Array.ConvertAll(columns, EscapeCsvField));
        string consoleFormatted = $"[{timestamp}] [{levelStr}] [{sourceName}] {message}";

        File.AppendAllText(logFilePath, formatted + Environment.NewLine);

        switch (level)
        {
            case LogLevel.DEBUG:
            case LogLevel.INFO:
                Debug.Log(consoleFormatted);
                break;
            case LogLevel.WARNING:
                Debug.LogWarning(consoleFormatted);
                break;
            case LogLevel.ERROR:
            case LogLevel.CRITICAL:
                Debug.LogError(consoleFormatted);
                break;
        }
    }

    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        if (logString.StartsWith("[")) return;

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string levelStr;
        string sourceName = "Unity";
        string message = logString;

        switch (type)
        {
            case LogType.Log:
                levelStr = LogLevel.INFO.ToString();
                if (LogLevel.INFO < currentLogLevel) return;
                break;
            case LogType.Warning:
                levelStr = LogLevel.WARNING.ToString();
                if (LogLevel.WARNING < currentLogLevel) return;
                break;
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                levelStr = LogLevel.ERROR.ToString();
                message += $"\n{stackTrace}";
                if (LogLevel.ERROR < currentLogLevel) return;
                break;
            default:
                return;
        }

        string[] columns = { timestamp, levelStr, sourceName, message };
        string formatted = string.Join(",", Array.ConvertAll(columns, EscapeCsvField));

        File.AppendAllText(logFilePath, formatted + Environment.NewLine);
    }

    private string EscapeCsvField(string field)
    {
        string escapedField = field.Replace("\"", "\"\"");
        if (escapedField.Contains(",") || escapedField.Contains("\n")) escapedField = $"\"{escapedField}\"";
        return escapedField;
    }
    #endregion
}