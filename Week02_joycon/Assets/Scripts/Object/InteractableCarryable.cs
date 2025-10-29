using System.Collections.Generic;
using UnityEngine;

public class InteractableCarryable : Carryable, IInteractable
{
    [SerializeField] protected List<ItemName> interactableItems;

    public bool TryInteract()
    {
        if (carrying == true) return false;
        foreach (var item in interactableItems)
        {
            if (InventoryManager.Instance.HasItem(item) == false) continue;
            if (Interact(InventoryManager.Instance.GetItem(item))) return true;
        }
        return false;
    }

    protected virtual bool Interact(Carryable carryable)
    {
        if (interactableItems.Contains(carryable.GetItemName()) == false) return false;
        if (InteractMethod(carryable) == false) return false;

        InventoryManager.Instance.RemoveAndDestroyItem(carryable);
        return true;
    }

    protected virtual bool InteractMethod(Carryable carryable) => false;

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (carrying == true) return;
        if (collision.TryGetComponent(out Carryable carryable)) Interact(carryable);
    }
}