using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class QuestUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Transform contentsPanel;
    [SerializeField] private QuestContentUI contentPrefab;

    private QuestManager questManager => QuestManager.Instance;
    private readonly List<QuestContentUI> _entries = new();
    public static uint questId;
    private uint _builtForQuestId;

    void OnEnable() { if (questManager) questManager.OnQuestUpdated += OnQuestUpdated; Redraw(); }
    void OnDisable() { if (questManager) questManager.OnQuestUpdated -= OnQuestUpdated; }

    public void SetQuest(uint newId)
    {
        if (questId == newId) { Redraw(); return; }
        questId = newId;
        RebuildForQuest();
        Redraw();
    }

    void OnQuestUpdated(uint changedId)
    {
        if (changedId != questId) SetQuest(changedId);
        Redraw();
    }

    void RebuildForQuest()
    {
        ClearEntries();

        if (!questManager || !questManager.TryGetSnapshot(questId, out var qs) || qs.so == null) return;

        int n = qs.objectives.Length;
        for (int i = 0; i < n; i++) _entries.Add(Instantiate(contentPrefab, contentsPanel));

        _builtForQuestId = questId;
    }

    void Redraw()
    {
        if (!questManager || !questManager.TryGetSnapshot(questId, out var qs) || qs.so == null)
        {
            if (titleText) titleText.text = "";
            return;
        }

        if (_builtForQuestId != questId)
        {
            RebuildForQuest();
            if (_builtForQuestId != questId) return;
        }

        if (titleText) titleText.text = qs.so.title;

        int cnt = Mathf.Min(_entries.Count, qs.objectives.Length);
        for (int i = 0; i < cnt; i++)
        {
            var obj = qs.objectives[i];
            _entries[i].SetContent($"- {obj.def.displayName}", obj.completed);
        }

        for (int i = cnt; i < _entries.Count; i++) _entries[i].gameObject.SetActive(false);
    }

    void ClearEntries()
    {
        for (int i = 0; i < _entries.Count; i++)
            if (_entries[i]) Destroy(_entries[i].gameObject);
        _entries.Clear();
    }
}