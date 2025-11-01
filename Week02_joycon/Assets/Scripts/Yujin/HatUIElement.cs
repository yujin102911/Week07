using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class HatUIElement : MonoBehaviour
{
    [SerializeField] private HatType hatType;
    [SerializeField] private PlayerHatController playerHatController;
    [SerializeField] private float notGetAlpha = 50f;
    [SerializeField] private float getAlpha = 255f;
    [SerializeField] private float panelDuration = 3f;

    [SerializeField] private UISlideToggleOnFire panelSlider;

    private Image uiImage;

    private static Coroutine _autoCloseCo;

    private void Start()
    {
        uiImage = GetComponent<Image>();

        if (playerHatController == null)
        {
            playerHatController = FindObjectOfType<PlayerHatController>();
        }

        if (playerHatController == null)
        {
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
        playerHatController.OnHatAcquired += HandleHatAcquired;

        // UI 알파 값 초기 업데이트
        UpdateAlpha();
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
            UpdateAlpha();
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
    /// 획득 여부에 따라 알파 값을 조절
    /// </summary>
    public void UpdateAlpha()
    {
        if (uiImage == null || playerHatController == null) return;

        bool isUsed = playerHatController.IsHatUsed(hatType);
        Color currentColor = uiImage.color;

        if (isUsed)
        {
            currentColor.a = getAlpha / 255f;
        }
        else
        {
            currentColor.a = notGetAlpha / 255f;
        }

        uiImage.color = currentColor;
    }


    private void OnDestroy()
    {
        if (playerHatController != null)
        {
            playerHatController.OnHatAcquired -= HandleHatAcquired;
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