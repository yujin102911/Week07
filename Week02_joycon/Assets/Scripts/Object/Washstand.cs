using UnityEngine;

public class Washstand : MonoBehaviour, IInteractable
{
    public bool TryInteract()
    {
        return InventoryManager.Instance.HasItem(ItemName.Rag);
    }
}