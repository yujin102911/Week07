using UnityEngine;

public class InteractableClothesline : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject emptyLine;
    [SerializeField] private GameObject fullLine;

    public void Interact()
    {
        if (InventoryManager.Instance.HasItem(ItemName.Bedding) == false) return;

        emptyLine.SetActive(false);
        fullLine.SetActive(true);

        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.Bedding);

        QuestRuntime.Instance.SetFlag(FlagId.DryingRack);
        GameLogger.Instance.LogDebug(this, "이불 퀘스트 완료");
    }
}