using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractableItems
{
    public ItemName itemName;
    public int interactableCount;
    public bool destroyItem;
}

public class InteractableWithItem : MonoBehaviour, IInteractable
{
    protected const int InteractableAlways = -1;

    [SerializeField] protected List<InteractableItems> interactableItems;
    protected Carryable carryable;
    protected bool _isInteracting;

    protected virtual void Start()
    {
        carryable = GetComponent<Carryable>();
        _isInteracting = false;
    }

    public virtual bool TryInteract()
    {
        if (_isInteracting == true) return false;
        if (carryable != null && carryable.GetIsCarried() == true) return false;

        foreach (var item in interactableItems)
        {
            if (item.interactableCount == 0) continue;
            if (InventoryManager.Instance.HasItem(item.itemName) == false) continue;

            var target = InventoryManager.Instance.GetItem(item.itemName);
            if (target == null) continue;

            if (Interact(target) == true) return true;
        }
        return false;
    }

    public virtual bool Interact(Carryable target)
    {
        if (_isInteracting == true) return false;
        _isInteracting = true;

        try
        {
            if (target == null) return false;
            if (CanInteract(target) == false) return false;

            int idx = GetIdxOfInteractableItem(target.GetItemName());
            if (idx < 0) return false;

            var item = interactableItems[idx];
            if (item.interactableCount == 0) return false;

            if (item.destroyItem == true) InventoryManager.Instance.RemoveAndDestroyItem(target);
            else Player.TryDrop(target);

            if (InteractMethod(target) == false) return false;
            if (item.interactableCount != InteractableAlways)
            {
                item.interactableCount--;
                if (item.interactableCount == 0) interactableItems.RemoveAt(idx);
            }

            return true;
        }
        finally
        {
            _isInteracting = false;
        }
    }

    protected int GetIdxOfInteractableItem(ItemName itemName) => interactableItems.FindIndex(x => x.itemName == itemName);
    protected virtual bool CanInteract(Carryable carryable) => true;
    protected virtual bool InteractMethod(Carryable carryable) => false;

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (carryable != null && carryable.GetIsCarried() == true) return;
        if (collision.TryGetComponent(out Carryable target) == false) return;
        if (target.GetIsCarried() == true) return;

        Interact(target);
    }
}