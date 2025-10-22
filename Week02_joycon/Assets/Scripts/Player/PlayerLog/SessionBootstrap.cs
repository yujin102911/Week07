using UnityEngine;

public class SessionBootstrap : MonoBehaviour
{
    public static string SessionId { get; private set; }
    void Awake()
    {
        if (string.IsNullOrEmpty(SessionId))
            SessionId = LogPathUtil.NewSessionId();
        Debug.Log($"[Session] {SessionId}");
    }
}