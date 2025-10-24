using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Global quest runtime: flags only (kept simple).</summary>
[DefaultExecutionOrder(-500)]
public sealed class QuestRuntime : MonoBehaviour
{
    public static QuestRuntime Instance
    {
        get
        {
            if (_inst == null)
            {
#if UNITY_2023_1_OR_NEWER
                _inst = FindFirstObjectByType<QuestRuntime>(FindObjectsInactive.Include);
#else
                _inst = FindObjectOfType<QuestRuntime>();
#endif
                if (_inst == null)
                {
                    var go = new GameObject(nameof(QuestRuntime));
                    _inst = go.AddComponent<QuestRuntime>();
                    DontDestroyOnLoad(go);
                }
            }
            return _inst;
        }
    }
    static QuestRuntime _inst;

    [Header("Initial Flags (debug)")]
    [SerializeField] private string[] initialFlags;

    readonly HashSet<int> _flags = new(128);

    public event Action<int> OnFlagRaised;
    public event Action<int> OnFlagCleared;

    void Awake()
    {
        if (_inst != this && _inst != null) { Destroy(gameObject); return; }
        _inst = this;
        DontDestroyOnLoad(gameObject);

        if (initialFlags != null)
            for (int i = 0; i < initialFlags.Length; ++i)
                _flags.Add(Animator.StringToHash(initialFlags[i] ?? string.Empty));
    }

    // ---- Flags ----
    public bool HasFlag(int flagHash) => _flags.Contains(flagHash);
    public bool HasFlag(string flagId) => _flags.Contains(Animator.StringToHash(flagId ?? string.Empty));

    public bool SetFlag(string flagId)
    {
        int h = Animator.StringToHash(flagId ?? string.Empty);
        if (_flags.Add(h))
        {
            OnFlagRaised?.Invoke(h);
            QuestEvents.RaiseFlag(flagId);
            return true;
        }
        return false;
    }

    public bool ClearFlag(string flagId)
    {
        int h = Animator.StringToHash(flagId ?? string.Empty);
        if (_flags.Remove(h))
        {
            OnFlagCleared?.Invoke(h);
            QuestEvents.RaiseFlagCleared(flagId);
            return true;
        }
        return false;
    }

    // ---- (Optional) Tiny save/load for flags ----
    [Serializable] struct SaveBlob { public int[] flags; }

    public string ToJson()
    {
        var arr = new int[_flags.Count];
        int i = 0; foreach (var h in _flags) arr[i++] = h;
        return JsonUtility.ToJson(new SaveBlob { flags = arr }, false);
    }

    public void FromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return;
        var b = JsonUtility.FromJson<SaveBlob>(json);
        _flags.Clear();
        if (b.flags != null) for (int i = 0; i < b.flags.Length; ++i) _flags.Add(b.flags[i]);
    }
}