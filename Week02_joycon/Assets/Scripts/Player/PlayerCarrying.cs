using System.Collections.Generic;
using UnityEngine;

public class PlayerCarrying : MonoBehaviour
{
    private Transform holdPoint;
    private Vector2 dropPoint;
    private LayerMask carryableMask;
    private LayerMask obstacleMask;
    public float CarryableWeight;
    private float carryingTotalHeight;

    private Vector2 lastObjSize;
    private Controller2D controller2D;
    private bool showDropGizmo = false;
    private Vector2 pickUpPos;
    private Vector2 pickUpBox;
    private Vector2 lastDropPos;
    private BoxCollider2D boxCollider2D;

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

        carryingTotalHeight = 0.0f;
        controller2D = GetComponent<Controller2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        interactableFilter = new ContactFilter2D();
        interactableFilter.SetLayerMask(interactableMask);
        interactableFilter.useTriggers = true;
    }

    private void LateUpdate()
    {
        PlaceCarriedStack();
    }

    private void OnDrawGizmos()
    {
        if (showDropGizmo)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(lastDropPos, lastObjSize);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(pickUpPos, pickUpBox);
    }

    // ===== Public API =====
    public void TryInteract()
    {
        if (EnsureCooldown() == false) return;
        if (TryUseItemOnWorld()) return;
        TryPickUp();
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

    public void UpdateWeight()
    {
        CarryableWeight = 0;
        if (StackCount == 0) return;

        for (int i = 0; i < StackCount; i++)
        {
            var carryable = OwnedItems[i];
            if (carryable) CarryableWeight += carryable.GetWeight();
        }
    }

    public void ConsumeItem(int index)
    {
        if (index < 0 || index >= StackCount) return;
        var item = OwnedItems[index];
        if (!item)
        {
            // 이미 파괴됨 → 인벤토리에서만 제거
            InventoryManager.Instance.RemoveItem(item);
            return;
        }

        InventoryManager.Instance.RemoveItem(item);
        Destroy(item.gameObject);
        GameLogger.Instance.LogDebug(this, $"아이템 소모: {item.name}");
    }

    // ===== Pick Up =====
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
        if (closest.GetIsCarrying() == true) return false;

        InventoryManager.Instance.AddItem(closest);
        SetupRigidOnPickup(closest);
        closest.SetIsCarrying(true);

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

        PlaceAndRelease(obj, dropPos);

        InventoryManager.Instance.RemoveItem(carryable);
        UpdateWeight();
        return true;
    }

    public void DropAllForce()
    {
        foreach (var carryable in OwnedItems)
        {
            if (!carryable) continue;
            carryable.SetIsCarrying(false);
            SetupRigidOnPickup(carryable);
        }
    }

    // ===== World Interact (kept, but unified area calc) =====
    private bool TryUseItemOnWorld()
    {
        // 들고 있는 최상단 아이템 ID (정책: 0번을 최상단으로 사용)
        string heldItemId = null;
        if (StackCount > 0)
        {
            var top = OwnedItems[0];
            if (!top) return false;
            heldItemId = top.Id;
        }

        var area = BuildForwardBox();
        Collider2D[] hits = Physics2D.OverlapBoxAll(area.pos, area.size, 0f, interactableMask);
        if (hits.Length == 0) return false;

        GameObject closestObj = null;
        float minD = Mathf.Infinity;
        WorldInteractable wi = null;

        foreach (var hit in hits)
        {
            if (!hit) continue;
            if (hit.TryGetComponent<WorldInteractable>(out var temp))
            {
                float d = Vector2.Distance(transform.position, hit.transform.position);
                if (d < minD)
                {
                    minD = d;
                    closestObj = hit.gameObject;
                    wi = temp;
                }
            }
        }

        if (wi != null)
        {
            GameLogger.Instance.LogDebug(this, $"WorldInteractable 감지: {closestObj.name}");
            bool ok = wi.AttemptInteraction(heldItemId, this);
            if (ok)
            {
                GameLogger.Instance.LogDebug(this, "상호작용 성공함!!");
                return true;
            }
            GameLogger.Instance.LogDebug(this, "상호작용 실패함 (아이템 불일치 등). 줍기 시도.");
        }
        return false;
    }

    // 1) 공통 쿨타임
    private bool EnsureCooldown()
    {
        if (Time.time - lastInteractTime < PlayerConstant.InteractCoolTime) return false;

        lastInteractTime = Time.time;
        return true;
    }

    // 3) 드롭 포즈 계산(공통)
    private bool ComputeDropPose(GameObject obj, out Vector2 dropPos, out Vector2 dropSize)
    {
        dropPos = default;
        dropSize = default;
        var objSize = GetBoundsSize(obj);
        var playerSize = GetPlayerBoundsSize();
        int faceDir = GetFaceDir();

        // 드롭 오프셋
        dropPoint = new Vector2(
            (objSize.x + playerSize.x) * faceDir / 2f,
            (objSize.y - playerSize.y) / 2f + 0.05f
        );

        dropPos = (Vector2)transform.position + dropPoint;
        dropSize = objSize * 1.0f;

        // 디버그
        lastDropPos = dropPos;
        lastObjSize = dropSize;
        showDropGizmo = true;

        return true;
    }

    // 4) 배치 가능 여부(장애물 겹침)
    private bool CanPlace(Vector2 pos, Vector2 size)
    {
        int mask = obstacleMask.value;
        return Physics2D.OverlapBox(pos, size, 0f, mask) == null;
    }

    // 5) 실제 배치 + 해제
    private void PlaceAndRelease(GameObject obj, Vector2 pos)
    {
        if (obj.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.transform.SetParent(null, true);
            rb.position = pos;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            obj.transform.SetParent(null, true);
            obj.transform.position = pos;
        }

        if (obj.TryGetComponent<Carryable>(out var c))
            c.SetIsCarrying(false);
    }

    // 6) 전방 박스(픽업/인터랙트 공용)
    private (Vector2 pos, Vector2 size) BuildForwardBox()
    {
        int faceDir = GetFaceDir();
        pickUpPos = new Vector2(transform.position.x + (PlayerConstant.InteractableRange / 2f * faceDir),
                                transform.position.y);
        pickUpBox = new Vector2(PlayerConstant.InteractableRange, boxCollider2D.bounds.size.y * 1.1f);
        return (pickUpPos, pickUpBox);
    }

    // 7) 가장 가까운 Carryable 탐색
    private Carryable FindClosestCarryable(Collider2D[] hits)
    {
        Carryable closest = null;
        float minD = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit) continue;
            if (!hit.TryGetComponent<Carryable>(out var carryable)) continue;
            if (!carryable.enabled || carryable.GetIsCarrying() == true) continue;

            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minD) { minD = d; closest = carryable; }
            GameLogger.Instance.LogDebug(this, "집기 조작 " + hit);
        }
        return closest;
    }

    // 8) 픽업 시 Rigidbody 상태 통일
    private static void SetupRigidOnPickup(Carryable carryable)
    {
        if (!carryable.TryGetComponent<Rigidbody2D>(out var rb)) return;

        // 정방향/역방향으로 바로 세워 놓기
        var rot = carryable.transform.eulerAngles;
        rot.z = (rot.z < 90f || rot.z >= 270f) ? 0f : 180f;
        carryable.transform.eulerAngles = rot;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    // 9) 스택 배치 (Inventory 순서대로 쌓기)
    private void PlaceCarriedStack()
    {
        carryingTotalHeight = 0f;
        if (StackCount == 0) return;

        for (int i = 0; i < StackCount; i++)
        {
            var c = OwnedItems[i];
            if (!c) continue;

            var col = c.GetComponent<Collider2D>();
            if (!col) continue;

            float objH = col.bounds.size.y;
            c.transform.position = holdPoint.position + new Vector3(0, carryingTotalHeight + objH / 2f, 0);
            carryingTotalHeight += objH;
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

    private Vector2 GetPlayerBoundsSize()
    {
        if (boxCollider2D != null)
        {
            var s = boxCollider2D.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }
        if (TryGetComponent<Collider2D>(out var selfCol))
        {
            var s = selfCol.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            var s = sr.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }
        var ls = transform.lossyScale;
        return new Vector2(Mathf.Abs(ls.x), Mathf.Abs(ls.y));
    }

    // 11) 진행 방향
    private int GetFaceDir() => controller2D != null ? controller2D.collisions.faceDir : 1;
}