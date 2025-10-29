using System.Collections.Generic;
using UnityEngine;

public class PlayerCarrying : MonoBehaviour
{
    private Transform holdPoint;
    private Vector2 dropPoint;
    private LayerMask carryableMask;
    private LayerMask obstacleMask;
    private float totalHeight;
    private float totalWeight;
    public float GetTotalWeight() => totalWeight;

    private Controller2D controller2D;
    private Vector2 playerSize;

    private float lastInteractTime = 0;

    [SerializeField] private LayerMask interactableMask;
    private ContactFilter2D interactableFilter;

    private List<Carryable> OwnedItems => InventoryManager.Instance.GetOwnedItems();
    private int StackCount => OwnedItems?.Count ?? 0;

    // ===== Unity =====
    private void Start()
    {
        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.parent = transform;
        holdPoint.localPosition = PlayerConstant.HoldPointOffset;

        carryableMask = LayerMask.GetMask(PlayerConstant.CarryableMask);
        obstacleMask = LayerMask.GetMask(PlayerConstant.ObstacleMask);

        totalHeight = 0.0f;
        controller2D = GetComponent<Controller2D>();
        playerSize = GetComponent<BoxCollider2D>().bounds.size;

        interactableFilter = new ContactFilter2D();
        interactableFilter.SetLayerMask(interactableMask);
        interactableFilter.useTriggers = true;
    }

    private void LateUpdate()
    {
        PlaceCarriedStack();
    }

    public bool TryInteract()
    {
        if (EnsureCooldown() == false) return false;
        return TryPickUp();
    }

    public bool TryDrop()
    {
        if (IsReadyToDrop() == false) return false;
        return DropAtIndex(StackCount - 1);
    }

    public bool TryDrop(ItemName itemName)
    {
        if (IsReadyToDrop() == false) return false;

        for (int i = StackCount - 1; i >= 0; --i)
        {
            var carryable = OwnedItems[i];
            if (carryable && carryable.GetItemName() == itemName) return DropAtIndex(i);
        }
        return false;
    }

    public bool TryDrop(Carryable carryable)
    {
        if (IsReadyToDrop() == false || !carryable) return false;

        int index = OwnedItems.IndexOf(carryable);
        if (index < 0) return false;
        return DropAtIndex(index);
    }

    private bool IsReadyToDrop()
    {
        if (EnsureCooldown() == false) return false;
        if (StackCount == 0) return false;

        return true;
    }

    private void UpdateWeight()
    {
        totalWeight = 0;
        if (StackCount == 0) return;

        for (int i = 0; i < StackCount; i++)
        {
            var carryable = OwnedItems[i];
            if (carryable) totalWeight += carryable.GetWeight();
        }
    }

    private bool TryPickUp()
    {
        if (StackCount >= PlayerConstant.CarryableMaxCount)
        {
            Debug.Log("Cannot pick up: Max carry count reached");
            return false;
        }

        var area = BuildForwardBox();
        Collider2D[] hits = Physics2D.OverlapBoxAll(area.pos, area.size, 0f, carryableMask);

        Carryable closest = FindClosestCarryable(hits);
        if (closest == null) return false;
        if (closest.GetIsCarried() == true) return false;

        InventoryManager.Instance.AddItem(closest);
        closest.SetIsCarried(true);

        UpdateWeight();
        return true;
    }

    // ===== Drop (Single Path) =====
    private bool DropAtIndex(int index)
    {
        if (index < 0 || index >= StackCount) return false;

        var carryable = OwnedItems[index];
        if (!carryable)
        {
            InventoryManager.Instance.RemoveItem(carryable);
            return false;
        }

        GameObject obj = carryable.gameObject;

        if (!ComputeDropPose(obj, out Vector2 dropPos, out Vector2 dropSize)) return false;
        if (!CanPlace(dropPos, dropSize)) return false;

        PlaceAndRelease(carryable, dropPos);

        InventoryManager.Instance.RemoveItem(carryable);
        UpdateWeight();
        return true;
    }

    public void DropAllForce()
    {
        foreach (var carryable in OwnedItems)
        {
            if (carryable == false) continue;
            carryable.SetIsCarried(false);
        }
    }

    private bool EnsureCooldown()
    {
        if (Time.time - lastInteractTime < PlayerConstant.InteractCoolTime) return false;

        lastInteractTime = Time.time;
        return true;
    }

    // 드롭 포즈 계산
    private bool ComputeDropPose(GameObject obj, out Vector2 dropPos, out Vector2 dropSize)
    {
        var objSize = GetBoundsSize(obj);
        int faceDir = GetFaceDir();

        dropPoint = new Vector2(
            (objSize.x + playerSize.x) * faceDir / 2f,
            (objSize.y - playerSize.y) / 2f + 0.05f
        );

        dropPos = (Vector2)transform.position + dropPoint;
        dropSize = objSize * 1.0f;

        return true;
    }

    // 배치 가능 여부(장애물 겹침)
    private bool CanPlace(Vector2 pos, Vector2 size)
    {
        int mask = obstacleMask.value;
        return Physics2D.OverlapBox(pos, size, 0f, mask) == null;
    }

    // 실제 배치 + 해제
    private void PlaceAndRelease(Carryable carryable, Vector2 pos)
    {
        carryable.transform.position = pos;
        carryable.SetIsCarried(false);
    }

    // 전방 박스
    private (Vector2 pos, Vector2 size) BuildForwardBox()
    {
        int faceDir = GetFaceDir();
        var pickUpPos = new Vector2(transform.position.x + (PlayerConstant.InteractableRange / 2f * faceDir), transform.position.y);
        var pickUpBox = new Vector2(PlayerConstant.InteractableRange, playerSize.y * 1.1f);
        return (pickUpPos, pickUpBox);
    }

    // 가장 가까운 Carryable 탐색
    private Carryable FindClosestCarryable(Collider2D[] hits)
    {
        Carryable closest = null;
        float minD = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit) continue;
            if (!hit.TryGetComponent<Carryable>(out var carryable)) continue;
            if (!carryable.enabled || carryable.GetIsCarried() == true) continue;

            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minD) { minD = d; closest = carryable; }
            GameLogger.Instance.LogDebug(this, "집기 조작 " + hit);
        }
        return closest;
    }

    // 9) 스택 배치 (Inventory 순서대로 쌓기)
    private void PlaceCarriedStack()
    {
        totalHeight = 0f;
        if (StackCount == 0) return;

        for (int i = 0; i < StackCount; i++)
        {
            var c = OwnedItems[i];
            if (!c) continue;

            var col = c.GetComponent<Collider2D>();
            if (!col) continue;

            float objH = col.bounds.size.y;
            c.transform.position = holdPoint.position + new Vector3(0, totalHeight + objH / 2f, 0);
            totalHeight += objH;
        }
    }

    // 10) 크기 계산 (오브젝트/플레이어)
    private static Vector2 GetBoundsSize(GameObject go)
    {
        if (!go) return Vector2.zero;
        if (go.TryGetComponent<Collider2D>(out var col))
        {
            var s = col.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }
        if (go.TryGetComponent<SpriteRenderer>(out var sr))
        {
            var s = sr.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }
        var ls = go.transform.lossyScale;
        return new Vector2(Mathf.Abs(ls.x), Mathf.Abs(ls.y));
    }

    // 11) 진행 방향
    private int GetFaceDir() => controller2D != null ? controller2D.collisions.faceDir : 1;
}