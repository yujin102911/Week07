using System;
using System.IO;
using UnityEngine;

#region LogLevel Enum
/// <summary>
/// 로그 레벨 정의
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

    [Header("Optional Custom File Name (접두사)")]
    [SerializeField] private string customFileName = "";

    //  1. 현재 기록할 로그 레벨 (필터)
    // 예: 설정한 단계 이상의 로그만 기록됨
    public LogLevel currentLogLevel = LogLevel.DEBUG; // 모든 로그 기록

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

        // 저장 위치
        string exeDir = Path.GetDirectoryName(Application.dataPath);
        string logDir = Path.Combine(exeDir, "log");
        Directory.CreateDirectory(logDir);

        string filePrefix = string.IsNullOrWhiteSpace(customFileName) ? "GameLog" : customFileName.Trim();
        string fileName = $"{filePrefix}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        logFilePath = Path.Combine(logDir, fileName);

        string header = "Timestamp, Level, Source, Message" + Environment.NewLine;
        File.AppendAllText(logFilePath, header);

        // 유니티 로그만 처리
        Application.logMessageReceived += HandleUnityLog;

        LogInfo(this, "=== Game Session Started ==="); //이 게임의 무조건 머리말
    }
    private void OnDestroy()
    {
        Application.logMessageReceived -= HandleUnityLog;
    }

    #endregion

    #region Initialization

    #endregion

    #region Public Methods - 디버그 레벨별 헬퍼 함수
    // 2. 레벨별 헬퍼(Helper) 함수들
    // 단계 맞춰서 이 함수들 호출하면 됨
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
    //  3. 모든 로그가 거쳐가는 핵심 함수 (Private)
    private void Log(LogLevel level, object source, string message)
    {
        // 3-1. 로그 레벨 필터링
        // 현재 설정된 레벨(currentLogLevel)보다 낮은 중요도의 로그는 기록하지 않음.
        if (level < currentLogLevel)
        {
            return;
        }

        // 3-2. CSV형식에 맞는 데이트 준비
        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string levelStr = level.ToString();
        string sourceName = (source != null) ? source.GetType().Name : "Unknown";

        string[] columns = {timestamp, levelStr, sourceName, message};

        // 3-3. 포맷 맞추기
        string formatted = string.Join(",", Array.ConvertAll(columns, EscapeCsvField)); //문서용 포맷
        string consoleFormatted = $"[{timestamp}] [{levelStr}] [{sourceName}] {message}"; //유니티 콘솔용 포맷

        // 3-4. 파일에 먼저 쓰기 (중요!)
        File.AppendAllText(logFilePath, formatted + Environment.NewLine);

        // 3-5. 유니티 콘솔에 레벨에 맞춰 출력하기
        // 이 코드가 HandleUnityLog 이벤트를 발생시키지만,
        // formatted 문자열이 "["로 시작하므로 HandleUnityLog가 무시
        // 따라서 기본 Debug.Log를 사용할 땐 "["로 시작하지 않도록 주의
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

    //  4. 유니티 기본 로그 처리
    private void HandleUnityLog(string logString, string stackTrace, LogType type)
    {
        // 4-1. 중복 방지
        // 3-5에서 직접 호출한 로그(Debug.LogWarning 등)는
        // 이미 파일에 썼고, "["로 시작하므로 여기서 걸러집니다.
        if (logString.StartsWith("["))
            return;

        string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        string levelStr;
        string sourceName = "Unity"; // 출처는 "Unity"로 고정
        string message = logString;

        // 4-2. GameLogger를 쓰지 않은 다른 스크립트의 Debug.Log, Error 등을 처리
        // (Unity의 LogType을 우리의 LogLevel로 매핑)
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
                message += $"\n{stackTrace}"; // 에러일 경우 스택트레이스를 메시지에 포함
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
    /// 메시지 안에 콤마나 큰따옴표가 있어도 엑셀이 깨지지않게
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private string EscapeCsvField(string field)
    {
        //1. 메시지 안의 큰따옴표(")를 두 개("")로
        string escapedField = field.Replace("\"", "\"\"");

        //2. 메시지에 콤마나 줄바꿈이 있다면 전체를 큰 따옴표로
        if (escapedField.Contains(",") || escapedField.Contains("\n"))
        {
            escapedField = $"\"{escapedField}\"";
        }
        return escapedField;
    }

    #endregion

}


/* 사용 예시
 * 
 * GameLogger.Instance.LogInfo(this, "게임매니저 시작됨");
 *  --> source를 this로 넘겨주면 알아서 GameLogger가 알아서 GameManager라는 이름 추출함
 * 
 * GameLogger.Instance.LogDebug(this, "플레이어 점프");
 * 
 * 디버그를 호출할 때는 로그 레벨에 맞는 디버그 함수를 호출해야함
 * Debug: 개발중에 사용되는 로그로, 상세한 정보와 디버깅을 위한 로그(플레이어 위치, 조작 등)
 * Info: 정상적인 작동, 게임의 진행을 확인할 수 있는 정보성 로그(게임 매니저 로드, 맵 로드 등)
 * Warning: 주의가 필요한 상황에서 발생하는 로그(리소스 누락이지만 게임 진행은 가능할 때)
 * Error: 게임 실행 중 명백한 오류를 나타내는 로그(데이터베이스 연결 오류, 특정 스크립트 로드 안됨)
 * Critical: 심각한 오류, 치명적인 오류를 나타내는 로그(핵심 시스템의 초기화 실패 등)
 * 
 * 대부분 Debug와 Info가 사용될 것으로 예상
 * 
 */