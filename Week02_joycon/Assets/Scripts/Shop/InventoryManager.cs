public class InventoryManager : Singleton<InventoryManager>
{
    private Inventory inventory;

    protected override void Awake()
    {
        base.Awake();

        inventory = new();
        inventory.Initialize();
    }

    public bool HasItem(ItemName itemName) => inventory.HasItem(itemName);
    public void AddItem(ItemName itemName)
    {
        inventory.AddItem(itemName);
    }
    public int GetGold() => inventory.GetGold();
    public int AddGold(int amount) => inventory.AddGold(amount);
}