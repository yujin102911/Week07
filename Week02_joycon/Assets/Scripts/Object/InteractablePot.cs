using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientObject
{
    public ItemName itemName;
    public GameObject ingredientObject;
}

public class InteractablePot : Carryable, IInteractable
{
    private Dictionary<ItemName, int> recipe = new()
    {
        { ItemName.Tomato, 3 },
        { ItemName.Onion, 1 },
    };

    [SerializeField] private List<IngredientObject> ingredients;

    [Header("State")]
    private InteractableStove currentStove = null;
    private bool isReadyToCook => recipe.Count == 0;
    private bool isCooked = false;

    [Header("Cooking")]
    [SerializeField] private GameObject cookedMealPrefab;
    [SerializeField] private Transform spawnPoint;

    public bool AddIngredient()
    {
        if (isCooked) return false;

        foreach (var item in recipe.Keys)
        {
            if (InventoryManager.Instance.HasItem(item))
            {
                recipe[item]--;
                if (recipe[item] <= 0) recipe.Remove(item);

                var ingredientObject = ingredients.Find(x => x.itemName == item);
                if (ingredientObject != null) ingredientObject.ingredientObject.SetActive(true);
                ingredients.Remove(ingredientObject);

                InventoryManager.Instance.RemoveAndDestroyItem(item);
                CheckCookingConditions();
                return true;
            }
        }
        return false;
    }

    public bool AddIngredient(Carryable carryable)
    {
        if (isCooked == true) return false;

        var itemName = carryable.GetItemName();
        if (recipe.ContainsKey(itemName) == false) return false;

        recipe[itemName]--;
        if (recipe[itemName] <= 0) recipe.Remove(itemName);
        Destroy(carryable.gameObject);

        var ingredientObject = ingredients.Find(x => x.itemName == itemName);
        if (ingredientObject != null) ingredientObject.ingredientObject.SetActive(true);
        ingredients.Remove(ingredientObject);

        return true;
    }

    public void SetCurrentStove(InteractableStove stove) => currentStove = stove;

    public void CheckCookingConditions()
    {
        if (isCooked == true) return;
        if (isReadyToCook == false) return;
        if (currentStove == null) return;
        if (currentStove.isFireOn == false) return;

        Cook();
    }

    private void Cook()
    {
        if (cookedMealPrefab != null) Instantiate(cookedMealPrefab, spawnPoint.position, Quaternion.identity);
        isCooked = true;
        currentStove.ResetStove();
    }

    public bool TryInteract()
    {
        if (isCarrying == true) return false;
        return AddIngredient();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isCarrying == true) return;
        if (collision.TryGetComponent<Carryable>(out var carryable) == false) return;
        if (carryable.GetIsCarrying() == true) return;

        AddIngredient(carryable);
    }
}