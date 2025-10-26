using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractableTable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject soup;
    [SerializeField] private bool itemPlaced = false;

    public bool Interact()
    {
        if (itemPlaced) return false;
        if (InventoryManager.Instance.HasItem(ItemName.TomatoSoup) == false) return false;

        itemPlaced = true;
        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.TomatoSoup);
        soup.SetActive(true);

        QuestRuntime.Instance.SetFlag(FlagId.Table_Used);
        GameLogger.Instance.LogDebug(this, "음식 퀘스트 완료");
        return true;
    }
}