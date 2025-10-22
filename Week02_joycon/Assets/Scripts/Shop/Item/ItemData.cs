using UnityEngine;

public enum ItemName
{
    Sword,
}

[System.Serializable]
public class ItemData
{
    public ItemName itemName;
    public string itemNameString;
    public string description;
    public int price;
    public Sprite sprite;

    private ItemData(ItemName itemName, string itemNameString, string description, int price, Sprite sprite)
    {
        this.itemName = itemName;
        this.itemNameString = itemNameString;
        this.description = description;
        this.price = price;
        this.sprite = sprite;
    }

    public ItemData Copy() => new(itemName, itemNameString, description, price, sprite);
}