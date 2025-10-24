using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Quests;

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

    [Header("초기 플래그 (enum)")]
    [SerializeField] private FlagId[] initialFlagEnums;

    // 내부 상태는 해시로만 관리 (외부로 노출 안 함)
    private readonly HashSet<int> _flags = new(128);

    // 퍼블릭 이벤트는 enum만 노출
    public event Action<FlagId> OnFlagRaised;
    public event Action<FlagId> OnFlagCleared;

    void Awake()
    {
        if (_inst != this && _inst != null) { Destroy(gameObject); return; }
        _inst = this;
        DontDestroyOnLoad(gameObject);

        if (initialFlagEnums != null)
        {
            foreach (var f in initialFlagEnums)
                _flags.Add(Hash(f));
        }
    }

    static int Hash(FlagId id) => Animator.StringToHash(id.ToString().Replace('_', '.'));

    // ---- 조회/설정 (enum만) ----
    public bool HasFlag(FlagId flag) => _flags.Contains(Hash(flag));

    public bool SetFlag(FlagId flag)
    {
        int h = Hash(flag);
        if (_flags.Add(h))
        {
            OnFlagRaised?.Invoke(flag);
            QuestEvents.RaiseFlag(flag);
            return true;
        }
        return false;
    }

    public bool ClearFlag(FlagId flag)
    {
        int h = Hash(flag);
        if (_flags.Remove(h))
        {
            OnFlagCleared?.Invoke(flag);
            QuestEvents.RaiseFlagCleared(flag);
            return true;
        }
        return false;
    }

    // 선택: 저장/불러오기(내부는 해시로 저장)
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
        if (b.flags != null) foreach (var h in b.flags) _flags.Add(h);
    }
}