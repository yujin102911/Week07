using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Carryable), typeof(Collider2D))]
public class Pot : MonoBehaviour, IInteractable
{
    private Dictionary<ItemName, int> recipe = new()
    {
        { ItemName.Tomato, 3 },
        { ItemName.Onion, 1 },
    };

    [Header("State")]
    private Stove currentStove = null;
    private bool isReadyToCook => recipe.Count == 0;
    private bool isCooked = false;

    [Header("Cooking")]
    [SerializeField] private GameObject cookedMealPrefab;
    [SerializeField] private Transform spawnPoint;

    private Carryable selfCarryable;
    private IngredientReceiver ingredientReceiver;
    private Collider2D ingredientCollider;

    private void Start()
    {
        selfCarryable = GetComponent<Carryable>();
        ingredientReceiver = GetComponentInChildren<IngredientReceiver>(true);
        ingredientCollider = ingredientReceiver.GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (selfCarryable != null && ingredientCollider != null) ingredientCollider.isTrigger = !selfCarryable.carrying;
    }

    public bool AddIngredient()
    {
        if (isCooked) return false;

        foreach (var item in recipe.Keys)
        {
            if (InventoryManager.Instance.HasItem(item))
            {
                recipe[item]--;
                if (recipe[item] <= 0) recipe.Remove(item);

                InventoryManager.Instance.RemoveAndDestroyItem(item);
                CheckCookingConditions();
                return true;
            }
        }
        return false;
    }

    public void SetCurrentStove(Stove stove) => currentStove = stove;

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

    public bool Interact()
    {
        if (selfCarryable.carrying == true) return false;
        return AddIngredient();
    }
}