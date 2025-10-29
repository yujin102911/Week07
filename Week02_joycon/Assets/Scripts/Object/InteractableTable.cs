using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractableTable : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject soup;
    [SerializeField] private bool itemPlaced = false;

    public bool TryInteract()
    {
        if (itemPlaced) return false;
        if (InventoryManager.Instance.HasItem(ItemName.TomatoSoup) == false) return false;

        itemPlaced = true;
        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.TomatoSoup);
        soup.SetActive(true);

        QuestRuntime.Instance.SetFlag(FlagId.PreparingFood);
        GameLogger.Instance.LogDebug(this, "음식 퀘스트 완료");
        return true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (itemPlaced) return;
        if (collision.TryGetComponent(out Carryable carryable))
        {
            if (carryable.GetItemName() == ItemName.TomatoSoup)
            {
                itemPlaced = true;
                soup.SetActive(true);

                Destroy(collision.gameObject);

                QuestRuntime.Instance.SetFlag(FlagId.PreparingFood);
                GameLogger.Instance.LogDebug(this, "음식 퀘스트 완료");
            }
        }
    }
}