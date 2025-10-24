using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private Inventory inventory;
    private void Start()
    {
        inventory = new();
        inventory.Initialize();
    }

    public void AddItem(ItemName itemName, GameObject itemObject) => inventory.AddItem(itemName, itemObject);
    public GameObject GetItemObject(ItemName itemName) => inventory.GetItemObject(itemName);
    public void RemoveItem(ItemName itemName) => inventory.RemoveItem(itemName);
    public void RemoveAndDestroyItem(ItemName itemName) => inventory.RemoveAndDestroyItem(itemName);
    public bool HasItem(ItemName itemName) => inventory.HasItem(itemName);
}