using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HatUIElement : MonoBehaviour
{
    [SerializeField] private HatType hatType;
    [SerializeField] private PlayerHatController playerHatController;

    private Image uiImage;

    private void Start()
    {
        uiImage = GetComponent<Image>();

        if (playerHatController == null)
        {
            playerHatController = FindObjectOfType<PlayerHatController>();
        }

        if (playerHatController == null)
        {
            Debug.LogError("PlayerHatController를 씬에서 찾을 수 없습니다!");
            return;
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
            // 쓴 적이 있다면 알파값 255
            currentColor.a = 1.0f;
        }
        else
        {
            // 쓴 적이 없다면 알파값 100
            currentColor.a = 100f / 255f;
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
}