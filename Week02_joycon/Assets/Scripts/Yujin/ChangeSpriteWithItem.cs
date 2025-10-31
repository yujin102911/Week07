using UnityEngine;

public class ChangeSpriteWithItem : InteractableWithItem
{
    [Header("Sprite Settings")]
    [SerializeField] private SpriteRenderer targetRenderer;
    [SerializeField] private Sprite newSprite;

    protected override bool InteractMethod(Carryable carryable)
    {
        if (targetRenderer == null) { GameLogger.Instance.LogError(this, $"{gameObject.name}의 Sprite Renderer가 할당되지 않았습니다."); return false; }
        if (newSprite == null) { GameLogger.Instance.LogError(this, $"{gameObject.name}을 바꿀 New Sprite가 할당되지 안핫삼"); return false; }
        targetRenderer.sprite = newSprite;
        return true;
    }

    protected override bool CanInteract(Carryable carryable)
    {
        // 부모 클래스의 기본 로직 (true 반환)을 그대로 사용
        bool baseCanInteract = base.CanInteract(carryable);
        if (!baseCanInteract) return false;

        // 만약 targetRenderer가 이미 newSprite로 변경된 상태라면,
        // 더 이상 상호작용할 필요가 없으므로 false를 반환.
        if (targetRenderer != null && targetRenderer.sprite == newSprite)
        {
            // 이미 변경된 상태이므로 상호작용 불필요
            return false;
        }

        return true;
    }

}
