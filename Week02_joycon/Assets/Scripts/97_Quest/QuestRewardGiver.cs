using UnityEngine;

public sealed class QuestRewardGiver : MonoBehaviour
{
    [SerializeField] private QuestSO quest;
    PauseMenu pauseMenu;

    void Awake() => pauseMenu = GameObject.FindAnyObjectByType<PauseMenu>();

    void OnEnable() { if (quest) quest.Completed += OnQuestCompleted; }
    void OnDisable() { if (quest) quest.Completed -= OnQuestCompleted; }

    void OnQuestCompleted(QuestSO so)
    {
        if (pauseMenu) pauseMenu.ShowEndingScreen();
        Debug.Log($"[Quest] Completed: {so.name}");
    }
}