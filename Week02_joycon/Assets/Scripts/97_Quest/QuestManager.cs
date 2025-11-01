using Game.Quests;
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class QuestManager : Singleton<QuestManager>
{
    [SerializeField] private QuestSO[] questDB;
    [Serializable]
    public struct SubTaskState
    {
        public InteractableId target; // enum match key
        public bool done;
    }

    [Serializable]
    public class ObjectiveState
    {
        public ObjectiveDef def;
        public SubTaskState[] subs;
        public bool completed;
    }

    [Serializable]
    public class QuestState
    {
        public QuestSO so;
        public bool started;
        public bool completed;
        public bool completionEventRaised;
        public ObjectiveState[] objectives;
    }

    private readonly Dictionary<uint, QuestState> _states = new(16);
    public event Action<uint> OnQuestUpdated;
    public static event Action OnAllQuestsCompleted;
    private bool _allQuestsRaised;

    void OnEnable()
    {
        QuestEvents.OnFlagRaised += OnFlagChanged;
        QuestEvents.OnFlagCleared += OnFlagChanged;
    }

    void OnDisable()
    {
        QuestEvents.OnFlagRaised -= OnFlagChanged;
        QuestEvents.OnFlagCleared -= OnFlagChanged;
    }

    private void Start()
    {
        TryAutoStartFirstNotStarted();
    }

    // --- Public API ---
    public bool StartQuest(uint questId)
    {
        if (_states.TryGetValue(questId, out var qs))
        {
            if (qs.started) return false;
            qs.started = true;
            EvaluateImmediateObjectives(qs);
            _states[questId] = qs;
            OnQuestUpdated?.Invoke(questId);
            TryRaiseCompletedFor(questId);
            MaybeRaiseAllCompleted();
            return true;
        }

        var so = FindQuestSO(questId);
        if (!so) return false;

        var newState = BuildState(so);
        newState.started = true;
        EvaluateImmediateObjectives(newState);
        _states[questId] = newState;
        OnQuestUpdated?.Invoke(questId);
        TryRaiseCompletedFor(questId);
        MaybeRaiseAllCompleted();
        return true;
    }

    public bool TryGetSnapshot(uint questId, out QuestState qs) => _states.TryGetValue(questId, out qs);

    QuestSO FindQuestSO(uint id)
    {
        if (questDB == null) return null;
        for (int i = 0; i < questDB.Length; ++i)
            if (questDB[i] && questDB[i].id == id) return questDB[i];
        return null;
    }

    int GetIndexById(uint id)
    {
        if (questDB == null) return -1;
        for (int i = 0; i < questDB.Length; ++i)
            if (questDB[i] && questDB[i].id == id) return i;
        return -1;
    }

    bool TryGetState(uint id, out QuestState s) => _states.TryGetValue(id, out s);

    QuestState BuildState(QuestSO so)
    {
        var qs = new QuestState { so = so, started = false, completed = false };
        var objs = so.objectives ?? Array.Empty<ObjectiveDef>();
        qs.objectives = new ObjectiveState[objs.Length];

        for (int i = 0; i < objs.Length; ++i)
        {
            ref var def = ref objs[i];
            var os = new ObjectiveState { def = def, completed = false };
            qs.objectives[i] = os;
        }
        return qs;
    }

    public bool TryGetFirstQuestTitleByFlag(FlagId flag, out string title)
    {
        title = null;
        if (questDB == null || questDB.Length == 0) return false;

        foreach (var so in questDB)
        {
            if (so == null || so.objectives == null) continue;

            foreach (var obj in so.objectives)
            {
                if (obj.requiredFlagEnum.Equals(flag))
                {
                    title = obj.displayName;
                    return true;
                }
            }
        }
        return false;
    }

    static bool AreMandatoryObjectivesCompleted(QuestState qs)
    {
        for (int i = 0; i < qs.objectives.Length; ++i)
        {
            var o = qs.objectives[i];
            if (!o.completed) return false;
        }
        return true;
    }

    void EvaluateImmediateObjectives(QuestState qs)
    {
        for (int i = 0; i < qs.objectives.Length; ++i)
            TryProgressObjective_RecheckFlags(qs.objectives[i]);
        qs.completed = AreMandatoryObjectivesCompleted(qs);
    }

    void TryRaiseCompletedFor(uint questId)
    {
        if (!TryGetState(questId, out var s) || s == null) return;

        if (s.completed && !s.completionEventRaised)
        {
            s.so?.RaiseCompleted();
            s.completionEventRaised = true;
            _states[questId] = s;

            TryStartNextChain(questId);
        }
    }

    // === Sequence helpers ===
    void TryAutoStartFirstNotStarted()
    {
        if (questDB == null || questDB.Length == 0) return;

        // 이미 시작된 퀘가 있으면 패스 (수동 진행 중인 시나리오 고려)
        foreach (var kv in _states)
            if (kv.Value != null && kv.Value.started) return;

        // DB의 첫 퀘를 시작
        var first = questDB[0];
        if (first) StartQuest(first.id);
    }

    void TryStartNextChain(uint justCompletedId)
    {
        if (questDB == null || questDB.Length == 0) return;

        int idx = GetIndexById(justCompletedId);
        if (idx < 0) return;

        // 다음 인덱스부터 순차적으로 시작.
        // 즉시 완료되는 퀘스트가 연속으로 있으면 연쇄적으로 넘어간다.
        for (int i = idx + 1; i < questDB.Length; ++i)
        {
            var so = questDB[i];
            if (!so) continue;

            // 이미 시작/완료 여부 확인
            if (TryGetState(so.id, out var st) && st.started)
            {
                if (!st.completed) break;  // 진행 중이면 더 이상 자동 진행 안 함
                else continue;             // 이미 완료면 다음으로 넘어감(연쇄)
            }

            // 시작
            StartQuest(so.id);

            // 방금 시작한 퀘가 즉시 완료되었는지 확인 (즉시완료면 다음으로 계속)
            if (TryGetState(so.id, out var ns) && ns.completed) continue;

            // 완료되지 않았다 → 여기서 대기 (다음 완료 때 다시 호출되어 이어서 진행)
            break;
        }

        // 모든 퀘가 이미 완료상태였다면 여기서 전체 완료 판정도 갱신
        MaybeRaiseAllCompleted();
    }

    // === All-quests helpers ===
    public bool AreAllStartedQuestsCompleted()
    {
        foreach (var kv in _states)
        {
            var s = kv.Value;
            if (s == null) continue;
            if (!s.started) continue;
            if (!s.completed) return false;
        }
        return true;
    }

    void MaybeRaiseAllCompleted()
    {
        if (_allQuestsRaised) return;
        if (AreAllStartedQuestsCompleted())
        {
            _allQuestsRaised = true;
            OnAllQuestsCompleted?.Invoke();
        }
    }

    void OnFlagChanged(FlagId _flag) => RecheckAllFlagsAndNotify();

    void RecheckAllFlagsAndNotify()
    {
        _keysScratch.Clear();
        foreach (var id in _states.Keys) _keysScratch.Add(id);
        _changedIds.Clear();

        for (int k = 0; k < _keysScratch.Count; ++k)
        {
            var questId = _keysScratch[k];
            if (!_states.TryGetValue(questId, out var qs)) continue;
            if (!qs.started) continue;

            bool changed = false;
            for (int i = 0; i < qs.objectives.Length; ++i)
                changed |= TryProgressObjective_RecheckFlags(qs.objectives[i]);

            if (changed)
            {
                qs.completed = AreMandatoryObjectivesCompleted(qs);
                _states[questId] = qs;
                _changedIds.Add(questId);
            }
        }

        for (int i = 0; i < _changedIds.Count; ++i)
        {
            var qid = _changedIds[i];
            OnQuestUpdated?.Invoke(qid);
            TryRaiseCompletedFor(qid);
            MaybeRaiseAllCompleted();
        }
    }

    // --- Progress logic (enum-based) ---
    static bool TryProgressObjective_OnInteract(ObjectiveState os, QuestEvents.InteractMsg msg)
    {
        if (os.completed) return false;
        if (os.subs.Length == 0) return false;

        bool touched = false;
        for (int s = 0; s < os.subs.Length; ++s)
        {
            if (!os.subs[s].done && os.subs[s].target.Equals(msg.id))
            {
                os.subs[s].done = true;
                touched = true;
            }
        }
        if (!touched) return false;

        int need = os.subs.Length;
        int doneCnt = 0;
        for (int k = 0; k < os.subs.Length; ++k) if (os.subs[k].done) doneCnt++;
        os.completed = doneCnt >= Mathf.Max(1, need);
        return true;
    }

    static bool TryProgressObjective_RecheckFlags(ObjectiveState os)
    {
        var flags = (os.def.requiredFlagEnums != null && os.def.requiredFlagEnums.Length > 0)
            ? os.def.requiredFlagEnums
            : (os.def.requiredFlagEnum.Equals(default(FlagId)) ? null : new FlagId[] { os.def.requiredFlagEnum });

        if (flags == null || flags.Length == 0) return false;

        bool before = os.completed;
        bool allOn = true;
        for (int i = 0; i < flags.Length; ++i)
            if (!QuestFlags.Has(flags[i])) { allOn = false; break; }

        os.completed = allOn;
        return os.completed != before;
    }

    private readonly List<uint> _keysScratch = new(32);
    private readonly List<uint> _changedIds = new(8);
}
