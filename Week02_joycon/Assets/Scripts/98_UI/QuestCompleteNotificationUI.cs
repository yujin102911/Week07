using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UISlideToggleOnFire))]
public class QuestCompleteNotificationUI : MonoBehaviour
{
    #region Serialize Fields
    [Header("UI")]
    [SerializeField] private UISlideToggleOnFire slidePanel;
    [SerializeField] private ParticleSystem confettiEffect;

    [Header("Text")]
    [SerializeField] private TMP_Text objectiveCompleteText;

    [Header("Settings")]
    [SerializeField] private float showDuration; // 패널이 화면에 표시된 후 자동으로 숨겨질 때까지 걸리는 시간

    [Header("Manual Toggle Input")]
    [SerializeField] private InputActionReference manualToggleAction;
    #endregion

    #region Private Fields
    private readonly HashSet<string> _notifiedObjectiveIds = new HashSet<string>(); // 이미 알림을 표시한 퀘스트 ID를 저장
    private Coroutine _showHideCo;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (slidePanel == null) { TryGetComponent(out slidePanel); }
        if (objectiveCompleteText == null) { objectiveCompleteText = GetComponentInChildren<TMP_Text>(); }
    }

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated += OnQuestUpdated;

        if (manualToggleAction && manualToggleAction.action != null)
        {
            manualToggleAction.action.Enable();
            manualToggleAction.action.performed += OnManualToggle;
        }

        //if (slidePanel != null)
        //    slidePanel.OnSlideShown += PlayFanfare;
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -= OnQuestUpdated;

        if (manualToggleAction && manualToggleAction.action != null)
        {
            manualToggleAction.action.performed -= OnManualToggle;
            manualToggleAction.action.Disable();
        }

        //if (slidePanel != null)
        //    slidePanel.OnSlideShown -= PlayFanfare;
    }
    #endregion

    #region Private Methods
    private void OnManualToggle(InputAction.CallbackContext _)
    {
        if (_showHideCo != null)
        {
            StopCoroutine(_showHideCo);
            _showHideCo = null;
        }

        if (slidePanel.IsShown)
            slidePanel.Hide();
        else
            _showHideCo = StartCoroutine(ShowAndHidePanel("", false));
    }

    private void OnQuestUpdated(uint questId)
    {
        if (!QuestManager.Instance.TryGetSnapshot(questId, out var qs))
            return;

        foreach (var objective in qs.objectives)
        {
            if (objective.completed)
            {
                string objectiveKey = $"{questId}-{objective.def.displayName}";
                if (_notifiedObjectiveIds.Add(objectiveKey))
                {
                    if (_showHideCo != null)
                        StopCoroutine(_showHideCo);

                    _showHideCo = StartCoroutine(ShowAndHidePanel(objective.def.displayName, true));
                }
            }
        }
    }

    private IEnumerator ShowAndHidePanel(string objectiveName, bool playFanfare)
    {
        slidePanel.Show();

        if (playFanfare)
        {
            PlayFanfare(); // 퀘스트 완료 시에만 빵빠레 재생
        }

        float totalWaitTime = slidePanel.SlideDuration + showDuration;
        yield return new WaitForSecondsRealtime(totalWaitTime);

        slidePanel.Hide();
        _showHideCo = null;
    }

    private void PlayFanfare()
    {
        GameLogger.Instance.LogDebug(this, "빵빠레 터짐용");
        if (confettiEffect != null)
            confettiEffect.Play();
    }
    #endregion
}