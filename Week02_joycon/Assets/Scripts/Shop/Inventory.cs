using System.Collections.Generic;

public class Inventory
{
    private int gold;
    private List<ItemName> ownedItems;

    public void Initialize()
    {
        ownedItems = new();
        gold = 0;
    }

    public bool HasItem(ItemName itemName) => ownedItems.Contains(itemName);
    public void AddItem(ItemName itemName)
    {
        if (!ownedItems.Contains(itemName)) ownedItems.Add(itemName);
    }

    public int GetGold() => gold;
    public int AddGold(int amount) => gold += amount;
}