using System;
using System.IO;
using UnityEngine;

#region LogLevel Enum
/// <summary>
/// �α� ���� ����
/// </summary>
public enum LogLevel
{
    DEBUG,    // 0
    INFO,     // 1
    WARNING,  // 2
    ERROR,    // 3
    CRITICAL  // 4
}
#endregion


public class GameLogger : MonoBehaviour
{
    public static GameLogger Instance { get; private set; }

    private string logFilePath;
    public string LogFilePath => logFilePath;

    [Header("Optional Custom File Name (���λ�)")]
    [SerializeField] private string customFileName = "";

    //  1. ���� ����� �α� ���� (����)
    // ��: ������ �ܰ� �̻��� �α׸� ��ϵ�
    public LogLevel currentLogLevel = LogLevel.DEBUG; // ��� �α� ���

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // ���� ��ġ
        string exeDir = Path.GetDirectoryName(Application.dataPath);
        string logDir = Path.Combine(exeDir, "log");
        Directory.CreateDirectory(logDir);

        string filePrefix = string.IsNullOrWhiteSpace(customFileName) ? "GameLog" : customFileName.Trim();
        string fileName = $"{filePrefix}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        logFilePath = Path.Combine(logDir, fileName);

        string header = "Timestamp, Level, Source, Message" + Environment.NewLine;
        File.AppendAllText(logFilePath, header);

        // ����Ƽ �α׸� ó��
        Application.logMessageReceived += HandleUnityLog;

        LogInfo(this, "=== Game Session Started ==="); //�� ������ ������ �Ӹ���
    }
    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    #endregion

    #region Initialization

    #endregion

    #region Public Methods - ����� ������ ���� �Լ�
    // 2. ������ ����(Helper) �Լ���
    // �ܰ� ���缭 �� �Լ��� ȣ���ϸ� ��
    public void LogDebug(object source, string message)
    {
        Log(LogLevel.DEBUG, source, message);
    }

    public void LogInfo(object source, string message)
    {
        Log(LogLevel.INFO, source, message);
    }

    public void LogWarning(object source, string message)
    {
        Log(LogLevel.WARNING, source, message);
    }

    public void LogError(object source,string message, Exception e = null)
    {
        if (e != null)
        {
            message += $"\nException: {e.Message}\n{e.StackTrace}";
        }
        Log(LogLevel.ERROR, source, message);
    }

    public void LogCritical(object source ,string message)
    {
        Log(LogLevel.CRITICAL, source, message);
    }
    #endregion

    #region Private Methods
    //  3. ��� �αװ� ���İ��� �ٽ� �Լ� (Private)
    private void Log(LogLevel level, object source, string message)
    {
        // 3-1. �α� ���� ���͸�
        // ���� ������ ����(currentLogLevel)���� ���� �߿䵵�� �α״� ������� ����.
        if (level < currentLogLevel)
        {
            return;
        }

        // 3-2. CSV���Ŀ� �´� ����Ʈ �غ�
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string levelStr = level.ToString();
        string sourceName = (source != null) ? source.GetType().Name : "Unknown";

        string[] columns = {timestamp, levelStr, sourceName, message};

        // 3-3. ���� ���߱�
        string formatted = string.Join(",", Array.ConvertAll(columns, EscapeCsvField)); //������ ����
        string consoleFormatted = $"[{timestamp}] [{levelStr}] [{sourceName}] {message}"; //����Ƽ �ֿܼ� ����

        // 3-4. ���Ͽ� ���� ���� (�߿�!)
        File.AppendAllText(logFilePath, formatted + Environment.NewLine);

        // 3-5. ����Ƽ �ֿܼ� ������ ���� ����ϱ�
        // �� �ڵ尡 HandleUnityLog �̺�Ʈ�� �߻���Ű����,
        // formatted ���ڿ��� "["�� �����ϹǷ� HandleUnityLog�� ����
        // ���� �⺻ Debug.Log�� ����� �� "["�� �������� �ʵ��� ����
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

    //  4. ����Ƽ �⺻ �α� ó��
    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        // 4-1. �ߺ� ����
        // 3-5���� ���� ȣ���� �α�(Debug.LogWarning ��)��
        // �̹� ���Ͽ� ���, "["�� �����ϹǷ� ���⼭ �ɷ����ϴ�.
        if (logString.StartsWith("["))
            return;

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string levelStr;
        string sourceName = "Unity"; // ��ó�� "Unity"�� ����
        string message = logString;

        // 4-2. GameLogger�� ���� ���� �ٸ� ��ũ��Ʈ�� Debug.Log, Error ���� ó��
        // (Unity�� LogType�� �츮�� LogLevel�� ����)
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
                message += $"\n{stackTrace}"; // ������ ��� ����Ʈ���̽��� �޽����� ����
                if (LogLevel.ERROR < currentLogLevel) return;
                break;
            default:
                return;
        }

        string[] columns = { timestamp, levelStr, sourceName, message };
        string formatted = string.Join(",", Array.ConvertAll(columns, EscapeCsvField));

        File.AppendAllText(logFilePath, formatted + Environment.NewLine);
    }

    /// <summary>
    /// �޽��� �ȿ� �޸��� ū����ǥ�� �־ ������ �������ʰ�
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private string EscapeCsvField(string field)
    {
        //1. �޽��� ���� ū����ǥ(")�� �� ��("")��
        string escapedField = field.Replace("\"", "\"\"");

        //2. �޽����� �޸��� �ٹٲ��� �ִٸ� ��ü�� ū ����ǥ��
        if (escapedField.Contains(",") || escapedField.Contains("\n"))
        {
            escapedField = $"\"{escapedField}\"";
        }
        return escapedField;
    }

    #endregion

}


/* ��� ����
 * 
 * GameLogger.Instance.LogInfo(this, "���ӸŴ��� ���۵�");
 *  --> source�� this�� �Ѱ��ָ� �˾Ƽ� GameLogger�� �˾Ƽ� GameManager��� �̸� ������
 * 
 * GameLogger.Instance.LogDebug(this, "�÷��̾� ����");
 * 
 * ����׸� ȣ���� ���� �α� ������ �´� ����� �Լ��� ȣ���ؾ���
 * Debug: �����߿� ���Ǵ� �α׷�, ���� ������ ������� ���� �α�(�÷��̾� ��ġ, ���� ��)
 * Info: �������� �۵�, ������ ������ Ȯ���� �� �ִ� ������ �α�(���� �Ŵ��� �ε�, �� �ε� ��)
 * Warning: ���ǰ� �ʿ��� ��Ȳ���� �߻��ϴ� �α�(���ҽ� ���������� ���� ������ ������ ��)
 * Error: ���� ���� �� ����� ������ ��Ÿ���� �α�(�����ͺ��̽� ���� ����, Ư�� ��ũ��Ʈ �ε� �ȵ�)
 * Critical: �ɰ��� ����, ġ������ ������ ��Ÿ���� �α�(�ٽ� �ý����� �ʱ�ȭ ���� ��)
 * 
 * ��κ� Debug�� Info�� ���� ������ ����
 * 
 */