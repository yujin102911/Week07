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

        string fileName = $"GameLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        logFilePath = Path.Combine(logDir, fileName);

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
        // 3-2. ��ó ��������
        string sourceName = (source != null) ? source.GetType().Name : "Unknown";

        // 3-3. ���� ���߱�
        string formatted = $"[{DateTime.Now:HH:mm:ss}] [{level}] [{sourceName}] {message}";

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
                Debug.Log(formatted);
                break;
            case LogLevel.WARNING:
                Debug.LogWarning(formatted);
                break;
            case LogLevel.ERROR:
            case LogLevel.CRITICAL:
                Debug.LogError(formatted);
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

        // 4-2. GameLogger�� ���� ���� �ٸ� ��ũ��Ʈ�� Debug.Log, Error ���� ó��
        // (Unity�� LogType�� �츮�� LogLevel�� ����)
        string formatted;
        string sourceName = "[Unity]"; //GameLogger�Ƚ����Ƿ�
        switch (type)
        {
            case LogType.Log:
                // GameLogger.LogInfo()�� �� �� �׳� Debug.Log()�� [INFO]�� ó��
                if (LogLevel.INFO < currentLogLevel) return; // ���͸�
                formatted = $"[{DateTime.Now:HH:mm:ss}] [INFO] {sourceName} {logString}"; break;
            case LogType.Warning:
                if (LogLevel.WARNING < currentLogLevel) return; // ���͸�
                formatted = $"[{DateTime.Now:HH:mm:ss}] [WARNING] {sourceName} {logString}"; break;
            case LogType.Error:
            case LogType.Exception:
            case LogType.Assert:
                if (LogLevel.ERROR < currentLogLevel) return; // ���͸�
                formatted = $"[{DateTime.Now:HH:mm:ss}] [ERROR] {sourceName} {logString}\n{stackTrace}"; break;
            default:
                return; // ó�� �� ��
        }

        File.AppendAllText(logFilePath, formatted + Environment.NewLine);
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