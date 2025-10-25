using UnityEngine;
using TMPro;
using System.Text;

public sealed class QuestTodoUI : MonoBehaviour
{
    [SerializeField] private QuestManager manager;
    [SerializeField] private uint questId;
    [SerializeField] private TMP_Text Titletext;
    [SerializeField] private TMP_Text ContentsText;

    [Header("Display Options")]
    [SerializeField] private bool strikeTitleOnlyWhenAllDone = true; // true: 전체 완료 시에만 제목 취소선
    [SerializeField] private bool strikeEachObjectiveWhenDone = true; // 완료된 목표는 취소선

    const string S_OPEN = "<s>";
    const string S_CLOSE = "</s>";

    void OnEnable()
    {
        if (manager != null) manager.OnQuestUpdated += OnQuestUpdated;
        Redraw();
    }
    void OnDisable()
    {
        if (manager != null) manager.OnQuestUpdated -= OnQuestUpdated;
    }
    public void SetQuest(uint newId) { questId = newId; Redraw(); }
    void OnQuestUpdated(uint changedId) { if (changedId == questId) Redraw(); }

    void Redraw()
    {
        if (!manager) return;

        if (!manager.TryGetSnapshot(questId, out var qs))
        {
            if (Titletext) Titletext.text = "";
            if (ContentsText) ContentsText.text = "";
            return;
        }

        // --- 제목 ---
        if (Titletext)
        {
            Titletext.richText = true;

            bool strikeTitle = qs.completed;
            if (!strikeTitle && !strikeTitleOnlyWhenAllDone)
            {
                // 목표 중 하나라도 완료되면 제목에 취소선(옵션)
                for (int i = 0; i < qs.objectives.Length; ++i)
                    if (qs.objectives[i].completed) { strikeTitle = true; break; }
            }

            Titletext.text = strikeTitle ? $"{S_OPEN}{qs.so.title}{S_CLOSE}" : qs.so.title;
        }

        // --- 본문: 목표 이름만, 체크기호/타깃명 없음 ---
        if (ContentsText)
        {
            var sb = new StringBuilder(256);
            for (int i = 0; i < qs.objectives.Length; ++i)
            {
                var os = qs.objectives[i];
                var name = os.def.displayName;

                if (strikeEachObjectiveWhenDone && os.completed)
                    sb.Append(" - ").Append(S_OPEN).Append(name).Append(S_CLOSE).AppendLine();
                else
                    sb.Append(" - ").Append(name).AppendLine();
            }

            ContentsText.richText = true;
            ContentsText.text = sb.ToString();
        }
    }
}