public class InventoryManager : Singleton<InventoryManager>
{
    private Inventory inventory;
    private void Start()
    {
        inventory = new();
        inventory.Initialize();
    }

    public void AddItem(ItemName itemName, int count = 1) => inventory.AddItem(itemName, count);
    public void RemoveItem(ItemName itemName, int count = 1) => inventory.RemoveItem(itemName, count);
    public bool HasItem(ItemName itemName) => inventory.HasItem(itemName);
}