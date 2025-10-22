public class Item
{
    protected ItemName itemName;
    protected ItemData itemData;

    public Item(ItemName itemName)
    {
        this.itemName = itemName;
        itemData = ItemDataLoader.GetItemData(itemName);
    }

    public ItemName GetItemName() => itemName;
    public ItemData GetItemData() => itemData;
    public virtual void GetItem() { }
    public virtual void UseItem() { }
}