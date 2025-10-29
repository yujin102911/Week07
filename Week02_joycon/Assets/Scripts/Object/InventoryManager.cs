public class InventoryManager : Singleton<InventoryManager>
{
    private Inventory inventory;
    private void Start()
    {
        inventory = new();
        inventory.Initialize();
    }

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
        if (HasItem(item) == false) return;

        RemoveItem(item);
        Destroy(item.gameObject);
    }
}