using UnityEngine;

public class ItemSellable : Item
{
    public ItemSellable(ItemName itemName) : base(itemName)
    {
        float randomPercent = Random.Range(-0.2f, 0.2f);
        itemData.price = (int)Mathf.Round(itemData.price * (1f + randomPercent));
    }

    public override void GetItem() { }
}