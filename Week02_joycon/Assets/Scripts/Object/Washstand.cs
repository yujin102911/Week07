using UnityEngine;

public class Washstand : MonoBehaviour, IInteractable
{
    public bool Interact()
    {
        return InventoryManager.Instance.HasItem(ItemName.Rag);
    }
}