using System.Collections.Generic;
using UnityEngine;

public enum ItemName
{
    None,
    Rag,
    Coin,
    Bedding,
    Mimic,
    Key,
}

public class Inventory
{
    private List<(ItemName itemName, GameObject itemObject)> OwnedItems;

    public void Initialize()
    {
        OwnedItems = new();
    }

    public bool HasItem(ItemName itemName) => OwnedItems.Exists(item => item.itemName == itemName);
    public void AddItem(ItemName itemName, GameObject itemObject) => OwnedItems.Add((itemName, itemObject));
    public GameObject GetItemObject(ItemName itemName)
    {
        var item = OwnedItems.Find(i => i.itemName == itemName);
        return item != default ? item.itemObject : null;
    }
    public void RemoveItem(ItemName itemName)
    {
        var item = OwnedItems.Find(i => i.itemName == itemName);
        if (item != default) OwnedItems.Remove(item);
    }
    public void RemoveAndDestroyItem(ItemName itemName)
    {
        var item = OwnedItems.Find(i => i.itemName == itemName);
        if (item != default)
        {
            OwnedItems.Remove(item);
            GameObject.Destroy(item.itemObject);
        }
    }

    public static bool HasItem(string itemId)
    {
        // ex) return SaveData.Items.TryGetValue(itemId, out var count) && count > 0;
        return false;
    }
}