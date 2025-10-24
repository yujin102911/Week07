using System.Collections.Generic;

public enum ItemName
{
    Rag,
    Key,
}

public class Inventory
{
    private Dictionary<ItemName, int> OwnedItems;

    public void Initialize()
    {
        OwnedItems = new();
    }

    public bool HasItem(ItemName itemName) => OwnedItems.ContainsKey(itemName) && OwnedItems[itemName] > 0;
    public void AddItem(ItemName itemName, int count = 1)
    {
        if (HasItem(itemName)) OwnedItems[itemName] += count;
        else OwnedItems[itemName] = count;
    }
    public void RemoveItem(ItemName itemName, int count = 1)
    {
        if (HasItem(itemName))
        {
            OwnedItems[itemName] -= count;
            if (OwnedItems[itemName] <= 0) OwnedItems.Remove(itemName);
        }
    }

    public static bool HasItem(string itemId)
    {
        // ex) return SaveData.Items.TryGetValue(itemId, out var count) && count > 0;
        return false;
    }
}