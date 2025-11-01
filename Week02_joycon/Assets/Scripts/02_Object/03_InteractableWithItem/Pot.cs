using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IngredientObject
{
    public ItemName itemName;
    public GameObject ingredientObject;
}

public class Pot : InteractableWithItem
{
    [SerializeField] private List<IngredientObject> ingredients;
    [SerializeField] private GameObject cookedMealPrefab;
    private Stove currentStove = null;
    private bool isCooked = false;

    public void SetCurrentStove(Stove stove) => currentStove = stove;

    protected override bool InteractMethod(Carryable carryable)
    {
        if (isCooked == true) return false;

        var ingredientObject = ingredients.Find(x => x.itemName == carryable.GetItemName());
        if (ingredientObject != null) ingredientObject.ingredientObject.SetActive(true);
        ingredients.Remove(ingredientObject);

        CheckCookingConditions();
        return true;
    }

    public void CheckCookingConditions()
    {
        if (isCooked == true) return;
        if (ingredients.Count > 0) return;
        if (currentStove == null || currentStove.isFireOn == false) return;

        Cook();
    }

    private void Cook()
    {
        if (cookedMealPrefab != null) Instantiate(cookedMealPrefab, transform.position, Quaternion.identity);
        isCooked = true;
    }
}