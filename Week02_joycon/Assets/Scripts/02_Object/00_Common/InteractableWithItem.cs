using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractableItems
{
    public ItemName itemName;
    public int itemCount;
    public bool destroyItem;
}

public class InteractableWithItem : MonoBehaviour, IInteractable
{
    [SerializeField] protected List<InteractableItems> interactableItems;
    private Carryable carryable;
    private bool _isInteracting;

    protected virtual void Start()
    {
        carryable = GetComponent<Carryable>();
        _isInteracting = false;
    }

    public bool TryInteract()
    {
        if (_isInteracting == true) return false;
        if (carryable != null && carryable.GetIsCarried() == true) return false;

        foreach (var item in interactableItems)
        {
            if (item.itemCount <= 0) continue;
            if (!InventoryManager.Instance.HasItem(item.itemName)) continue;

            var target = InventoryManager.Instance.GetItem(item.itemName);
            if (target == null) continue;

            if (Interact(target)) return true;
        }
        return false;
    }

    protected virtual bool Interact(Carryable target)
    {
        if (_isInteracting == true) return false;
        _isInteracting = true;

        try
        {
            if (target == null) return false;
            if (CanInteract(target) == false) return false;

            int idx = GetIdxOfInteractableItem(target.GetItemName());
            var item = interactableItems[idx];
            if (idx < 0) return false;
            if (item.itemCount <= 0) return false;

            if (item.destroyItem) InventoryManager.Instance.RemoveAndDestroyItem(target);
            else InventoryManager.Instance.RemoveItem(target);

            InteractMethod(target);

            item.itemCount--;
            if (item.itemCount <= 0) interactableItems.RemoveAt(idx);

            return true;
        }
        finally
        {
            _isInteracting = false;
        }
    }

    protected int GetIdxOfInteractableItem(ItemName itemName) => interactableItems.FindIndex(x => x.itemName == itemName);
    protected virtual bool CanInteract(Carryable carryable) => false;
    protected virtual bool InteractMethod(Carryable carryable) => false;

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (this.carryable != null && this.carryable.GetIsCarried() == true) return;
        if (collision.TryGetComponent(out Carryable carryable) == false) return;
        if (carryable.GetIsCarried() == true) return;

        Interact(carryable);
    }
}