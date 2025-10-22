using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    private Dictionary<ItemName, bool> items;

    public bool IsItemSold(ItemName itemName) => items[itemName];
    public void SetItemSold(ItemName itemName, bool sold) => items[itemName] = sold;

    public void SellItem(ItemName itemName)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName] = true;
            Debug.Log($"Item '{itemName}' sold.");
        }
        else
        {
            Debug.LogWarning($"Item '{itemName}' not found in shop.");
        }
    }
}