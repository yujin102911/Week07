using System.Collections.Generic;
using UnityEngine;

public class ItemDataLoader : MonoBehaviour
{
    [SerializeField] private List<ItemData> itemDataList;
    private static Dictionary<ItemName, ItemData> itemDataDictionary;

    private void Awake()
    {
        itemDataDictionary = new();

        foreach (var itemData in itemDataList) itemDataDictionary[itemData.itemName] = itemData;

    }

    public static ItemData GetItemData(ItemName itemName)
    {
        if (itemDataDictionary.TryGetValue(itemName, out var itemData)) return itemData.Copy();

        Debug.LogWarning($"ItemData for '{itemName}' not found.");
        return null;
    }
}