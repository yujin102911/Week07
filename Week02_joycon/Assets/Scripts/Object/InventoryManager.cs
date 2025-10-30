using System.Collections.Generic;

public class InventoryManager : Singleton<InventoryManager>
{
    private Inventory inventory;
    private void Start()
    {
        inventory = new();
        inventory.Initialize();
    }

    private void Update()
    {
        RemoveInvalidOwnedItems();
    }

    public List<Carryable> GetOwnedItems() => inventory.GetOwnedItems();
    public bool HasItem(ItemName itemName) => inventory.HasItem(itemName);
    public bool HasItem(Carryable item) => inventory.HasItem(item);
    public void AddItem(Carryable item) => inventory.AddItem(item);
    public Carryable GetItem(ItemName itemName) => inventory.GetItem(itemName);
    public void RemoveItem(ItemName itemName) => inventory.RemoveItem(itemName);
    public void RemoveItem(Carryable item) => inventory.RemoveItem(item);
    public void RemoveAndDestroyItem(ItemName itemName)
    {
        var item = GetItem(itemName);
        if (item == null) return;

        RemoveItem(item);
        Destroy(item.gameObject);
    }
    public void RemoveAndDestroyItem(Carryable item)
    {
        if (HasItem(item) == true) RemoveItem(item);
        Destroy(item.gameObject);
    }

    public void RemoveInvalidOwnedItems()
    {
        var items = GetOwnedItems();
        if (items == null) return;

        for (int i = items.Count - 1; i >= 0; --i)
        {
            var item = items[i];
            if (item == null || item.GetIsCarried() == false) items.RemoveAt(i);
        }

        Player.UpdateWeight();
    }
}