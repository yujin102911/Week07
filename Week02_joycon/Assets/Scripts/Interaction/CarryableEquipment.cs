public class CarryableEquipment : Carryable
{
    public virtual void GetItem()
    {
        InventoryManager.Instance.AddItem(itemName, gameObject);
    }

    public virtual bool UseItem(IInteractable interactable) { return false; }
}