using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class IngredientInfo
{
    public ItemName itemName;
    public GameObject ingredientObject;
    public Transform snapPoint;
    public bool isPlaced = false;
}

public class InteractableBath : MonoBehaviour, IInteractable
{
    [SerializeField] private float processingTime = 1.0f;
    [SerializeField] private List<IngredientInfo> requiredIngredients;
    private bool isComplete = false;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isComplete == true) return;
        if (!other.TryGetComponent(out Carryable carryable)) return;

        IngredientInfo ingredient = requiredIngredients.Find(ing => ing.itemName == carryable.GetItemName());

        if (carryable.GetIsCarried() == false && ingredient != null && ingredient.isPlaced == false) PlaceItem(carryable);
    }

    private void PlaceItem(Carryable item)
    {
        IngredientInfo ingredient = requiredIngredients.Find(ing => ing.itemName == item.GetItemName());
        if (ingredient == null) return;

        // 배치 순서대로 스냅 포인트 선택
        Transform snapPoint = ingredient.snapPoint;

        // 위치/회전 고정
        item.transform.position = snapPoint.position;
        item.transform.localScale = Vector3.one * 1.2f;
        item.transform.rotation = Quaternion.identity;

        // 물리 중지(흔들림 방지)
        if (item.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 더 이상 상호작용되지 않게 비활성화
        item.enabled = false;
        ingredient.ingredientObject = item.gameObject;
        ingredient.isPlaced = true;

        GameLogger.Instance.LogDebug(this, $"재료 배치 완료: {item.GetItemName()}");

        // 모두 모였는지 검사
        CheckForCompletion();
    }

    private void CheckForCompletion()
    {
        foreach (var ingredient in requiredIngredients)
            if (ingredient.isPlaced == false) return;


        // 여기까지 왔으면 완성
        isComplete = true;
        GameLogger.Instance.LogDebug(this, "조합 완료! 결과 생성 절차를 시작합니다...");
        ProcessCombination();
    }

    private void ProcessCombination()
    {
        foreach (GameObject itemObject in requiredIngredients.ConvertAll(ing => ing.ingredientObject))
        {
            if (itemObject.TryGetComponent(out CarryableMimic carryableMimic))
            {
                carryableMimic.bubbles.SetActive(true);
                carryableMimic.enabled = true;
            }
            else Destroy(itemObject);
        }
    }

    public bool TryInteract()
    {
        if (isComplete == true) return false;
        foreach (var ingredient in requiredIngredients)
        {
            if (InventoryManager.Instance.HasItem(ingredient.itemName))
            {
                var item = InventoryManager.Instance.GetItem(ingredient.itemName);
                Player.TryDrop(item);
                PlaceItem(item);
                CheckForCompletion();
                return true;
            }
        }
        return false;
    }
}