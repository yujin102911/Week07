using UnityEngine;
using TMPro;
using System.Text;

public sealed class QuestTodoUI : MonoBehaviour
{
    [SerializeField] private QuestManager manager;
    [SerializeField] private uint questId;
    [SerializeField] private TMP_Text Titletext;
    [SerializeField] private TMP_Text ContentsText;

    const string S_OPEN = "<s>";
    const string S_CLOSE = "</s>";

    void OnEnable()
    {
        if (manager != null)
            manager.OnQuestUpdated += OnQuestUpdated;
        Redraw();
    }

    void OnDisable()
    {
        if (manager != null)
            manager.OnQuestUpdated -= OnQuestUpdated;
    }

    public void SetQuest(uint newId)
    {
        questId = newId;
        Redraw();
    }

    void OnQuestUpdated(uint changedId)
    {
        if (changedId == questId) Redraw();
    }

    void Redraw()
    {
        if (!manager || !ContentsText) return;
        if (!manager.TryGetSnapshot(questId, out var qs))
        {
            ContentsText.text = "";
            return;
        }

        var sb = new StringBuilder(256); // body
        var tb = new StringBuilder(128); // title

        // Title with strike when whole quest completed
        AppendLineWithStrike(tb, qs.so.title, qs.completed);

        for (int i = 0; i < qs.objectives.Length; ++i)
        {
            var o = qs.objectives[i];

            sb.Append("- ");
            if (o.completed)
            {
                sb.Append(S_OPEN).Append(o.def.displayName);
                if (o.def.optional) sb.Append(" (선택)");
                sb.Append(S_CLOSE);
            }
            else
            {
                sb.Append(o.def.displayName);
                if (o.def.optional) sb.Append(" (선택)");
            }
            sb.AppendLine();

            // Subtasks (InteractSet only): render enum labels aligned with subs
            var subs = o.subs;
            if (subs != null && subs.Length > 0 && o.def.type == ObjectiveType.InteractSet)
            {
                // Prepare labels from enums (no strings in runtime)
                var enumLabels = (o.def.targetEnums != null && o.def.targetEnums.Length > 0)
                    ? o.def.targetEnums
                    : new InteractableId[] { o.def.targetEnum };

                // If counts mismatch, we still try to render up to the smaller length
                int count = Mathf.Min(subs.Length, enumLabels.Length);
                if (count > 0)
                {
                    sb.Append("   · ( ");
                    for (int s = 0; s < count; ++s)
                    {
                        if (s > 0) sb.Append(", ");
                        var label = enumLabels[s].ToString();
                        if (subs[s].done)
                            sb.Append(S_OPEN).Append(label).Append(S_CLOSE);
                        else
                            sb.Append(label);
                    }
                    sb.Append(" )");
                }
            }
        }

        if (Titletext) { Titletext.richText = true; Titletext.text = tb.ToString(); }
        ContentsText.richText = true;
        ContentsText.text = sb.ToString();
    }

    static void AppendLineWithStrike(StringBuilder sb, string str, bool strike)
    {
        if (strike)
        {
            sb.Append(S_OPEN).Append(str).Append(S_CLOSE);
            sb.Append("              >> 완료!").AppendLine();
        }
        else
        {
            sb.AppendLine(str);
        }
    }
}