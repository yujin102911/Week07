// InputReplayer.cs  (Pause/Resume 전용, 현재 구조 기반 보강)
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class InputReplayer : MonoBehaviour
{
    private float playbackSpeed = 1f;
    private Rigidbody2D rb;

    private Player player;
    private PlayerCarrying playerCarrying;

    private struct Row
    {
        public int tick;
        public float mx;
        public float my;
        public int jumpHeld;
        public int jumpDown;
        public int jumpUp;
        public int interact;
        public int drop;
    }

    private readonly List<Row> rows = new();
    private int cursor;
    private int currentTick;
    private float fixedDt;

    private Vector3 startPos;
    private Quaternion startRot;

    private bool isPaused = false;

    void Awake()
    {
        player = GetComponent<Player>();
        playerCarrying = GetComponent<PlayerCarrying>();
        rb = GetComponent<Rigidbody2D>();

        startPos = transform.position;
        startRot = transform.rotation;

        playbackSpeed = 1f;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void LoadAndPlay(string filePath)
    {
        rows.Clear(); cursor = 0; currentTick = 0; fixedDt = 0f;

        var lines = File.ReadAllLines(filePath);
        foreach (var ln in lines)
        {
            if (string.IsNullOrWhiteSpace(ln)) continue;

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

        ResetToStartTransformAndVel();

        isPaused = false;
        Time.timeScale = Mathf.Clamp(playbackSpeed, 1f, 10f);
        enabled = true;

        Debug.Log($"[Replayer] Loaded {rows.Count} rows, fixedDt={fixedDt}, x{Time.timeScale}");
    }

    public void Stop()
    {
        enabled = false;
        isPaused = false;
        Time.timeScale = 1f;
    }

    // --- 일시정지 컨트롤 ---
    public void Pause()
    {
        if (isPaused) return;
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!isPaused) return;
        isPaused = false;
        Time.timeScale = Mathf.Clamp(playbackSpeed, 1f, 10f);
    }

    public void TogglePause()
    {
        if (isPaused) Resume(); else Pause();
    }

    public void SetPlaybackSpeed(float speed01to10)
    {
        playbackSpeed = Mathf.Clamp(speed01to10, 1f, 10f);
        if (!isPaused) Time.timeScale = playbackSpeed;
    }

    void FixedUpdate()
    {
        if (rows.Count == 0 || isPaused) return;

        // 현재 틱까지 입력 소비
        while (cursor < rows.Count && rows[cursor].tick <= currentTick)
        {
            var r = rows[cursor++];
            player.SetDirectionalInput(new Vector2(r.mx, r.my));
            if (r.jumpDown == 1) player.OnJumpInputDown();
            if (r.jumpUp == 1) player.OnJumpInputUp();
            if (r.interact == 1) playerCarrying.TryInteract();
            if (r.drop == 1) playerCarrying.TryDrop();
        }

        currentTick++;
    }

    void ResetToStartTransformAndVel()
    {
        transform.SetPositionAndRotation(startPos, startRot);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

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