using UnityEngine;
using TMPro;
using System.Linq;

public sealed class QuestUI : MonoBehaviour
{
    private QuestManager questManager => QuestManager.Instance;
    [SerializeField] private uint questId;
    [SerializeField] private TMP_Text Titletext;

    [Header("Prefab & Container")]
    [SerializeField] private Transform contentsPanel;
    [SerializeField] private QuestContentUI contentPrefab;

    [Header("Display Options")]
    [SerializeField] private bool strikeTitleOnlyWhenAllDone = true;  // 전체 완료 시에만 제목 취소선
    [SerializeField] private bool strikeEachObjectiveWhenDone = true; // 목표 완료 시 해당 라인에 취소선(또는 완료선)

    const string S_OPEN = "<s>";
    const string S_CLOSE = "</s>";

    void OnEnable()
    {
        if (questManager != null) questManager.OnQuestUpdated += OnQuestUpdated;
        Redraw();
    }

    void OnDisable()
    {
        if (questManager != null) questManager.OnQuestUpdated -= OnQuestUpdated;
    }
    public void SetQuest(uint newId) { questId = newId; Redraw(); }
    void OnQuestUpdated(uint changedId)
    {
        if (changedId != questId) SetQuest(changedId);
        else Redraw();
    }

    void Redraw()
    {
        if (!questManager) return;

        // 스냅샷 없으면 UI 비움
        if (!questManager.TryGetSnapshot(questId, out var qs))
        {
            if (Titletext) Titletext.text = "";
            ClearContents();
            return;
        }

        // ----- 제목 갱신 -----
        if (Titletext)
        {
            Titletext.richText = true;

            bool strikeTitle = qs.completed;
            if (!strikeTitle && !strikeTitleOnlyWhenAllDone)
            {
                // 하나라도 완료되면 제목에 취소선(옵션)
                strikeTitle = qs.objectives.Any(o => o.completed);
            }
            Titletext.text = strikeTitle ? $"{S_OPEN}{qs.so.title}{S_CLOSE}" : qs.so.title;
        }

        // ----- 본문(목표들) 갱신: 프리팹 생성 후 자식으로 붙이기 -----
        ClearContents();

        if (!contentPrefab || !contentsPanel)
        {
            Debug.LogWarning("[QuestUI] contentPrefab 또는 contentsPanel이 지정되지 않았습니다.");
            return;
        }

        for (int i = 0; i < qs.objectives.Length; ++i)
        {
            var os = qs.objectives[i];
            var displayName = os.def.displayName;

            var entry = Instantiate(contentPrefab, contentsPanel);
            // 완료 표시 여부: 옵션에 따라 완료선/취소선 표시
            bool markCompleted = strikeEachObjectiveWhenDone && os.completed;

            // 각 아이템 UI에 내용/완료상태 주입
            entry.SetContent($"- {displayName}", markCompleted);
        }
    }

    private void ClearContents()
    {
        if (!contentsPanel) return;
        for (int i = contentsPanel.childCount - 1; i >= 0; --i)
            Destroy(contentsPanel.GetChild(i).gameObject);
    }
}