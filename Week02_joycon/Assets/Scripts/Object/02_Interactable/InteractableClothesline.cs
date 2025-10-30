using UnityEngine;

public class InteractableClothesline : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject emptyLine;
    [SerializeField] private GameObject fullLine;

    public bool TryInteract()
    {
        if (InventoryManager.Instance.HasItem(ItemName.Bedding) == false) return false;

        emptyLine.SetActive(false);
        fullLine.SetActive(true);

        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.Bedding);

        QuestRuntime.Instance.SetFlag(FlagId.DryingRack);

        return true;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.name.Contains("Bedding"))
        {
            Debug.Log("Clothesline Collision with Bedding");
            var carryable = collision.gameObject.GetComponent<Carryable>();
            if (carryable == null) return; // carryable 없으면 종료
            if (carryable.GetIsCarried() == true) return; // 플레이어가 들고 있으면 무시
            emptyLine.SetActive(false);
            fullLine.SetActive(true);
            Destroy(collision.gameObject);

            QuestRuntime.Instance.SetFlag(FlagId.DryingRack);
        }
    }
}