using UnityEngine;

public class InteractableClothesline : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject emptyLine;
    [SerializeField] private GameObject fullLine;

    public void Interact()
    {
        Debug.Log("Clothesline Interacted");
        if (InventoryManager.Instance.HasItem(ItemName.Bedding) == false) return;

        emptyLine.SetActive(false);
        fullLine.SetActive(true);

        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.Bedding);
    }
}