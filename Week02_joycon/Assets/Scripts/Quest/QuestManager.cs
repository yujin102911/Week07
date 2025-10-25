using Game.Quests;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Minimal manager: InteractSet + TriggerFlags (enum-only public surface)</summary>
[DisallowMultipleComponent]
public sealed class QuestManager : MonoBehaviour
{
    [SerializeField] private QuestSO[] questDB;

    [Serializable]
    public struct SubTaskState
    {
        public InteractableId target; // enum 매칭
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

    readonly Dictionary<uint, QuestState> _states = new(16);
    public event Action<uint> OnQuestUpdated;

    void OnEnable()
    {
        QuestEvents.OnInteract += OnInteract;
        QuestEvents.OnFlagRaised += OnFlagChanged;
        QuestEvents.OnFlagCleared += OnFlagChanged;
    }
    void OnDisable()
    {
        QuestEvents.OnInteract -= OnInteract;
        QuestEvents.OnFlagRaised -= OnFlagChanged;
        QuestEvents.OnFlagCleared -= OnFlagChanged;
    }

    void Start()
    {
        StartQuest(1000);
    }

    // --- Public API ---
    public bool StartQuest(uint questId)
    {
        if (_states.TryGetValue(questId, out var qs))
        {
            if (qs.started) return false;
            qs.started = true;
            EvaluateImmediateObjectives(qs);
            OnQuestUpdated?.Invoke(questId);
            return true;
        }

        var so = FindQuestSO(questId);
        if (!so) return false;

        var newState = BuildState(so);
        newState.started = true;
        EvaluateImmediateObjectives(newState);
        _states[questId] = newState;
        OnQuestUpdated?.Invoke(questId);
        return true;
    }

    public bool TryGetSnapshot(uint questId, out QuestState qs)
        => _states.TryGetValue(questId, out qs);

    // --- Internals ---
    QuestSO FindQuestSO(uint id)
    {
        if (questDB == null) return null;
        for (int i = 0; i < questDB.Length; ++i)
            if (questDB[i] && questDB[i].id == id) return questDB[i];
        return null;
    }

    QuestState BuildState(QuestSO so)
    {
        var qs = new QuestState { so = so, started = false, completed = false };
        var objs = so.objectives ?? Array.Empty<ObjectiveDef>();
        qs.objectives = new ObjectiveState[objs.Length];

        for (int i = 0; i < objs.Length; ++i)
        {
            ref var def = ref objs[i];
            var os = new ObjectiveState { def = def, completed = false };

            // 대상 enum 집합 준비
            var targets = (def.targetEnums != null && def.targetEnums.Length > 0)
                ? def.targetEnums
                : new InteractableId[] { def.targetEnum };

            if (targets != null && targets.Length > 0)
            {
                os.subs = new SubTaskState[targets.Length];
                for (int s = 0; s < targets.Length; ++s)
                    os.subs[s] = new SubTaskState { target = targets[s], done = false };
            }
            else os.subs = Array.Empty<SubTaskState>();

            qs.objectives[i] = os;
        }
        return qs;
    }

    static int GetFirstIncompleteObjectiveIndex(QuestState qs)
    {
        for (int i = 0; i < qs.objectives.Length; ++i)
            if (!qs.objectives[i].completed) return i;
        return -1;
    }

    static bool AreMandatoryObjectivesCompleted(QuestState qs)
    {
        for (int i = 0; i < qs.objectives.Length; ++i)
        {
            var o = qs.objectives[i];
            if (o.def.optional) continue;
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
        if (!_states.TryGetValue(questId, out var s) || s == null) return;
        if (s.completed && !s.completionEventRaised)
        {
            s.so?.RaiseCompleted();
            s.completionEventRaised = true;
        }
    }

    // --- Event handlers ---
    void OnInteract(QuestEvents.InteractMsg msg)
    {
        _keysScratch.Clear();
        foreach (var id in _states.Keys) _keysScratch.Add(id);
        _changedIds.Clear();

        for (int k = 0; k < _keysScratch.Count; ++k)
        {
            var questId = _keysScratch[k];
            if (!_states.TryGetValue(questId, out var qs)) continue;
            if (!qs.started || qs.completed) continue;

            bool changed = false;

            if (qs.so.sequentialObjectives)
            {
                int idx = GetFirstIncompleteObjectiveIndex(qs);
                if (idx >= 0) changed |= TryProgressObjective_OnInteract(qs.objectives[idx], msg);
            }
            else
            {
                for (int i = 0; i < qs.objectives.Length; ++i)
                {
                    var o = qs.objectives[i];
                    if (!o.completed) changed |= TryProgressObjective_OnInteract(o, msg);
                }
            }

            if (changed)
            {
                qs.completed = AreMandatoryObjectivesCompleted(qs);
                _changedIds.Add(questId);
            }
        }

        for (int i = 0; i < _changedIds.Count; ++i)
        {
            var qid = _changedIds[i];
            OnQuestUpdated?.Invoke(qid);
            TryRaiseCompletedFor(qid);
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
                _changedIds.Add(questId);
            }
        }

        for (int i = 0; i < _changedIds.Count; ++i)
        {
            var qid = _changedIds[i];
            OnQuestUpdated?.Invoke(qid);
            TryRaiseCompletedFor(qid);
        }
    }

    // --- Progress logic (enum-based) ---
    static bool TryProgressObjective_OnInteract(ObjectiveState os, QuestEvents.InteractMsg msg)
    {
        if (os.completed) return false;
        if (os.def.type != ObjectiveType.InteractSet) return false;
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

        int need = os.def.requiredCount <= 0 ? os.subs.Length : Mathf.Min(os.def.requiredCount, os.subs.Length);
        int doneCnt = 0;
        for (int k = 0; k < os.subs.Length; ++k) if (os.subs[k].done) doneCnt++;
        os.completed = (doneCnt >= Mathf.Max(1, need));
        return true;
    }

    static bool TryProgressObjective_RecheckFlags(ObjectiveState os)
    {
        if (os.def.type != ObjectiveType.TriggerFlags) return false;

        // 플래그 enum 집합 준비
        var flags = (os.def.requiredFlagEnums != null && os.def.requiredFlagEnums.Length > 0)
            ? os.def.requiredFlagEnums
            : new FlagId[] { os.def.requiredFlagEnum };

        if (flags == null || flags.Length == 0) return false;

        bool before = os.completed;
        bool allOn = true;
        for (int i = 0; i < flags.Length; ++i)
            if (!QuestFlags.Has(flags[i])) { allOn = false; break; }

        os.completed = allOn;
        return os.completed != before;
    }

    readonly List<uint> _keysScratch = new(32);
    readonly List<uint> _changedIds = new(8);
}
