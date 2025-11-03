using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class HatUIElement : MonoBehaviour
{
    [SerializeField] private HatType hatType;
    [SerializeField] private PlayerHatController playerHatController;
    //[SerializeField] private float notGetAlpha = 50f;
    //[SerializeField] private float getAlpha = 255f;
    [SerializeField] private float panelDuration = 3f;
    [SerializeField] private UISlideToggleOnFire panelSlider;

    private Button uiButton;

    private static Coroutine _autoCloseCo;

    private void Start()
    {
        uiButton = GetComponent<Button>();

        if (playerHatController == null)
        {
            playerHatController = FindObjectOfType<PlayerHatController>();
        }
        if (playerHatController == null)
        {
            GameLogger.Instance.LogError(this, "PlayerHatController를 찾을 수 없음");
            return;
        }
        if (panelSlider == null)
        {
            panelSlider = GetComponentInParent<UISlideToggleOnFire>();
        }
        if (panelSlider == null)
        {
            GameLogger.Instance.LogError(this, "UISlideToggleOnFire를 찾을 수 없음");
        }
        uiButton.onClick.AddListener(OnHatButtonClick);
        playerHatController.OnHatAcquired += HandleHatAcquired;
        UpdateUIState();

    }

    /// <summary>
    /// PlayerHatController에서 이벤트 발행 시 호출
    /// </summary>
    private void HandleHatAcquired(HatType acquiredHat)
    { 
        // 획득한 모자가 이 UI와 관련된 모자인지 확인
        if (acquiredHat == this.hatType)
        {
            // 즉시 알파 값을 업데이트
            UpdateUIState();
            if (panelSlider != null)
            {
                panelSlider.Show();
                if (_autoCloseCo != null)
                {
                    StopCoroutine(_autoCloseCo);
                }
                _autoCloseCo = StartCoroutine(AutoClosePanelAfterDelay(panelDuration));
            }
        }
    }

    /// <summary>
    /// 획득 여부에 따라 버튼 활성화
    /// </summary>
    public void UpdateUIState()
    {
        if (uiButton == null || playerHatController == null) return;

        // 모자를 획득한 적이 있는지 확인
        bool isUsed = playerHatController.IsHatUsed(hatType);

        uiButton.interactable = isUsed;
    }

    /// <summary>
    /// 버튼 눌리면 호출될 메서드
    /// </summary>
    private void OnHatButtonClick()
    {
        if (playerHatController != null)
        {
            playerHatController.ChangeHat(hatType, false);
        }
    }

    private void OnDestroy()
    {
        if (playerHatController != null)
        {
            playerHatController.OnHatAcquired -= HandleHatAcquired;
        }
        if (uiButton != null)
        {
            uiButton.onClick.RemoveListener(OnHatButtonClick);
        }
    }

    private IEnumerator AutoClosePanelAfterDelay(float delay)
    {
        // 3초 대기
        yield return new WaitForSecondsRealtime(delay);

        // 3초가 지난 후 패널을 닫음
        if (panelSlider != null)
        {
            panelSlider.Hide();
        }

        // 코루틴 완료
        _autoCloseCo = null;
    }

}