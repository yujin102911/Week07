using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    private Inventory inventory;
    private void Start()
    {
        inventory = new();
        inventory.Initialize();
    }

    public bool HasItem(ItemName itemName) => inventory.HasItem(itemName);
    public bool HasItem(GameObject itemObject) => inventory.HasItem(itemObject);
    public void AddItem(ItemName itemName, GameObject itemObject) => inventory.AddItem(itemName, itemObject);
    public GameObject GetItemObject(ItemName itemName) => inventory.GetItemObject(itemName);
    public void RemoveItem(ItemName itemName) => inventory.RemoveItem(itemName);
    public void RemoveItem(GameObject itemObject) => inventory.RemoveItem(itemObject);
    public void RemoveAndDestroyItem(ItemName itemName)
    {
        var itemObject = GetItemObject(itemName);
        if (itemObject != null)
        {
            RemoveItem(itemName);
            Destroy(itemObject);
        }
    }
    public void RemoveAndDestroyItem(GameObject itemObject)
    {
        if (HasItem(itemObject) == false) return;

        RemoveItem(itemObject);
        Destroy(itemObject);
    }
    public void DropItem(GameObject itemObject)
    {
        if (HasItem(itemObject) == false) return;

        RemoveItem(itemObject);
        Instance.GetComponent<PlayerCarrying>().TryDrop(itemObject);
    }
}