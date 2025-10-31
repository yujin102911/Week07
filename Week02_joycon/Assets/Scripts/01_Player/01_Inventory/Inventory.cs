using System.Collections.Generic;

public class Inventory
{
    private List<Carryable> OwnedItems;

    public void Initialize()
    {
        OwnedItems = new();
    }

    public List<Carryable> GetOwnedItems() => OwnedItems;
    public bool HasItem(ItemName itemName) => OwnedItems.Exists(item => item.NameIs(itemName));
    public bool HasItem(Carryable itemObject) => OwnedItems.Exists(item => item == itemObject);
    public void AddItem(Carryable itemObject) => OwnedItems.Add(itemObject);
    public Carryable GetItem(ItemName itemName)
    {
        return OwnedItems.Find(i => i.NameIs(itemName));
    }
    public void RemoveItem(ItemName itemName)
    {
        var item = OwnedItems.Find(i => i.NameIs(itemName));
        if (item != default) OwnedItems.Remove(item);
    }
    public void RemoveItem(Carryable itemObject)
    {
        var item = OwnedItems.Find(i => i == itemObject);
        if (item != default) OwnedItems.Remove(item);
    }
}