using System.Collections.Generic;
using UnityEngine;

public enum ItemName
{
    None,
    Rag,
    Coin,
    Bedding,
    Key,
}

public class Inventory
{
    private List<(ItemName, GameObject)> OwnedItems;

    public void Initialize()
    {
        OwnedItems = new();
    }

    public bool HasItem(ItemName itemName) => OwnedItems.Exists(item => item.Item1 == itemName);
    public void AddItem(ItemName itemName, GameObject itemObject) => OwnedItems.Add((itemName, itemObject));
    public void RemoveItem(ItemName itemName)
    {
        var item = OwnedItems.Find(i => i.Item1 == itemName);
        if (item != default) OwnedItems.Remove(item);
    }
    public void RemoveAndDestroyItem(ItemName itemName)
    {
        var item = OwnedItems.Find(i => i.Item1 == itemName);
        if (item != default)
        {
            OwnedItems.Remove(item);
            GameObject.Destroy(item.Item2);
        }
    }

    public static bool HasItem(string itemId)
    {
        // ex) return SaveData.Items.TryGetValue(itemId, out var count) && count > 0;
        return false;
    }
}