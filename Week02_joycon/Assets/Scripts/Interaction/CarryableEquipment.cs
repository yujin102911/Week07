public class CarryableEquipment : Carryable
{
    protected ItemName itemName;
    public ItemName GetItemName() => itemName;

    public virtual void GetItem()
    {
        InventoryManager.Instance.AddItem(itemName);
    }

    public virtual bool UseItem(IInteractable interactable) { return false; }
}