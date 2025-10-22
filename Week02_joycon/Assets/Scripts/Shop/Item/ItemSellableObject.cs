using UnityEngine;

public class ItemSellableObject : MonoBehaviour
{
    [SerializeField] private ItemName itemName;
    private ItemSellable itemSellable;

    private void Start()
    {
        itemSellable = new(itemName);
    }

    public void SellItem()
    {
        ShopManager.Instance.SellItem(itemName);
    }
}