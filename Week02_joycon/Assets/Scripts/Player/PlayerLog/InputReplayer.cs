// InputReplayer.cs (업데이트 버전)
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class InputReplayer : MonoBehaviour
{
    [Range(1f, 10f)] public float playbackSpeed = 1f;
    public Rigidbody2D rb2D;         // 2D면 이걸로

    Player player;

    // 로그 로우 구조
    struct Row { public int tick; public float mx, my; public int jumpHeld, jumpDown, jumpUp; }
    List<Row> rows = new();
    int cursor, currentTick;
    float fixedDt;

    // 시작 Transform 저장
    Vector3 startPos;
    Quaternion startRot;

    void Awake()
    {
        player = GetComponent<Player>();
        if (rb2D == null) rb2D = GetComponent<Rigidbody2D>();

        // 현재 위치/회전을 "시작점"으로 저장
        startPos = transform.position;
        startRot = transform.rotation;
    }

    public void LoadAndPlay(string filePath)
    {
        // 1) 파일 읽기
        rows.Clear(); cursor = 0; currentTick = 0; fixedDt = 0f;

        var lines = File.ReadAllLines(filePath);
        foreach (var ln in lines)
        {
            if (ln.Contains("\"meta\""))
            {
                var i = ln.IndexOf("\"fixedDt\":");
                if (i >= 0)
                {
                    var s = ln.Substring(i + 10);
                    var j = s.IndexOfAny(new[] { ',', '}', '\n', '\r' });
                    if (j > 0 && float.TryParse(s.Substring(0, j), out var f)) fixedDt = f;
                }
                continue;
            }
            try { rows.Add(JsonUtility.FromJson<Row>(ln)); } catch { }
        }
        if (fixedDt <= 0f) fixedDt = Time.fixedDeltaTime;

        // 2) 시작 위치로 리셋(+ 속도 0)
        ResetToStartTransformAndVel();

        // 3) 배속 적용 & 재생 시작
        Time.timeScale = Mathf.Clamp(playbackSpeed, 1f, 10f);
        enabled = true;

        Debug.Log($"[Replayer] Loaded {rows.Count} rows, fixedDt={fixedDt}, x{Time.timeScale}");
    }

    public void Stop()
    {
        enabled = false;
        Time.timeScale = 1f;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }

    void FixedUpdate()
    {
        if (rows.Count == 0) return;

        // 현재 틱까지의 입력을 소비하며 최신 상태 유지
        while (cursor < rows.Count && rows[cursor].tick <= currentTick)
        {
            var r = rows[cursor++];
            player.SetDirectionalInput(new Vector2(r.mx, r.my));
            if (r.jumpDown == 1) player.OnJumpInputDown();
            if (r.jumpUp == 1) player.OnJumpInputUp();
        }

        currentTick++;
    }

    /// <summary>
    /// 시작 위치·회전으로 되돌리고 속도 0, 틱도 0으로 되감기
    /// (UI에서 "Rewind" 버튼으로도 호출 가능)
    /// </summary>
    public void ResetToStartAndRewind()
    {
        ResetToStartTransformAndVel();
        cursor = 0;
        currentTick = 0;
    }

    void ResetToStartTransformAndVel()
    {
        transform.SetPositionAndRotation(startPos, startRot);

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
    }

    // 최신 로그 자동 찾기(기존 그대로)
    public static string FindLatestInputLog()
    {
        if (!Directory.Exists(LogPathUtil.Root)) return null;
        string latest = null; System.DateTime lt = System.DateTime.MinValue;
        foreach (var file in Directory.GetFiles(LogPathUtil.Root, "input*.jsonl", SearchOption.AllDirectories))
        {
            var t = File.GetLastWriteTimeUtc(file);
            if (t > lt) { lt = t; latest = file; }
        }
        return latest;
    }
}