using UnityEngine;

public sealed class QuestRewardGiver : MonoBehaviour
{
    [SerializeField] private QuestSO quest;

    PauseMenu pauseMenu;
    private void Awake()
    {
        pauseMenu = GameObject.FindAnyObjectByType<PauseMenu>();


    }
    void OnEnable()
    {
        if (quest) quest.Completed += OnQuestCompleted; // ����

    }

    void OnDisable()
    {
        if (quest) quest.Completed -= OnQuestCompleted; // �� ����
    }

    private void OnQuestCompleted(QuestSO so)
    {

        pauseMenu.ShowEndingScreen();
        // ������ ����: ���� ����, �÷��� ����, UI �佺Ʈ ��
        // ��) PlayerInventory.AddCoins(100);
        Debug.Log($"[Quest] Completed: {so.name}");
    }
}
