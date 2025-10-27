using UnityEngine;
using UnityEngine.Events;

public class WorldInteractable : MonoBehaviour
{
    public string requiredItemId;
    public bool consumeItemOnSuccess = false;
    public UnityEvent OnInteractionSuccess;

    public bool AttemptInteraction(string heldItemId, PlayerCarrying player)
    {
        if (string.IsNullOrEmpty(requiredItemId))
        {
            GameLogger.Instance.LogDebug(this, $"{gameObject.name} 상호작용 성공");
            OnInteractionSuccess?.Invoke();

            return true;
        }

        if (!string.IsNullOrEmpty(heldItemId) && requiredItemId == heldItemId)
        {
            GameLogger.Instance.LogDebug(this, $"{gameObject.name} - {heldItemId} 상호작용 성공");
            OnInteractionSuccess?.Invoke();
            if (consumeItemOnSuccess)
                player.ConsumeItem(0);

            return true;
        }

        GameLogger.Instance.LogDebug(this, $"{gameObject.name}은(는) {requiredItemId}이 필요함");
        return false;
    }
}