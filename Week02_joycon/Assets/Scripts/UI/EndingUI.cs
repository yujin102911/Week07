using UnityEngine;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private GameObject endingPanel;

    private void Start()
    {
        endingPanel.SetActive(false);
        QuestManager.OnAllQuestsCompleted += SetEndingUIActive;
    }

    private void SetEndingUIActive()
    {
        endingPanel.SetActive(true);
    }
}