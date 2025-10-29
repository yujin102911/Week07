using Game.Quests;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarrying : MonoBehaviour
{
    // ===== Fields =====
    private Transform holdPoint;
    private Vector2 dropPoint;
    private LayerMask carryableMask;
    private LayerMask obstacleMask;
    public float CarryableWeight;
    private float carryingTotalHeight;

    private Vector2 lastObjSize;
    Controller2D controller2D;
    private bool showDropGizmo = false;
    Vector2 pickUpPos;
    Vector2 pickUpBox;
    Vector2 lastDropPos;
    public int collideCarrying = 0;
    private BoxCollider2D boxCollider2D;

    public List<GameObject> carriedObjects = new();
    private float lastInteractTime = 0;

    [Header("World Interaction")]
    [SerializeField] private LayerMask interactableMask;
    private ContactFilter2D interactableFilter;

    // ===== Unity =====
    private void Start()
    {
        carryableMask = LayerMask.GetMask(PlayerConstant.CarryableMask);
        obstacleMask = LayerMask.GetMask(PlayerConstant.ObstacleMask);

        holdPoint = new GameObject("HoldPoint").transform;
        holdPoint.parent = transform;
        holdPoint.localPosition = PlayerConstant.HoldPointOffset;

        carryingTotalHeight = 0.0f;
        controller2D = GetComponent<Controller2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        interactableFilter = new ContactFilter2D();
        interactableFilter.SetLayerMask(interactableMask);
        interactableFilter.useTriggers = true;
    }

    private void Update()
    {
        if (PruneCarriedNulls() == true)
        {
            collideCarrying = carriedObjects.Count;
            UpdateWeight();
        }

        if (collideCarrying < carriedObjects.Count) CarryingDrop();
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
        if (TryUseItemOnWorld() == true) return;

        TryPickUp();
    }

    public bool TryDrop()
    {
        if (IsReadyToDrop() == false) return false;

        DropAtIndex(carriedObjects.Count - 1);
        return true;
    }

    public bool TryDrop(ItemName target)
    {
        if (IsReadyToDrop() == false) return false;

        for (int i = carriedObjects.Count - 1; i >= 0; --i)
        {
            var go = carriedObjects[i];
            if (!go) { carriedObjects.RemoveAt(i); continue; }
            if (go.TryGetComponent<Carryable>(out var c) && c.GetItemName() == target)
                return DropAtIndex(i);
        }

        return false;
    }

    public bool TryDrop(Carryable obj)
    {
        if (IsReadyToDrop() == false) return false;

        int index = carriedObjects.IndexOf(obj ? obj.gameObject : null);
        if (index < 0) return false;
        DropAtIndex(index);
        return true;
    }

    private bool IsReadyToDrop()
    {
        if (EnsureCooldown() == false) return false;
        if (carriedObjects.Count == 0) { UpdateWeight(); return false; }
        return true;
    }

    public void UpdateWeight()
    {
        CarryableWeight = 0;
        if (carriedObjects.Count <= 0) return;

        for (int i = 0; i < carriedObjects.Count; i++)
        {
            var carryable = carriedObjects[i]?.GetComponent<Carryable>();
            if (carryable != null) CarryableWeight += carryable.weight;
        }
    }

    public void CarryingDrop()
    {
        int count = carriedObjects.Count;
        if (count == 0) { collideCarrying = 0; return; }

        int startIndex = Mathf.Clamp(collideCarrying, 0, count);
        for (int i = count - 1; i >= startIndex; --i)
        {
            var go = carriedObjects[i];
            if (go == null) { carriedObjects.RemoveAt(i); continue; }

            if (go.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = false;
            }
            if (go.TryGetComponent<Carryable>(out var car))
                car.carrying = false;

            carriedObjects.RemoveAt(i);
        }
        collideCarrying = carriedObjects.Count;
        UpdateWeight();
    }

    public void ConsumeItem(int index)
    {
        if (index < 0 || index >= carriedObjects.Count) return;
        var item = carriedObjects[index];
        if (!item) { carriedObjects.RemoveAt(index); collideCarrying = carriedObjects.Count; UpdateWeight(); return; }

        carriedObjects.RemoveAt(index);
        collideCarrying = carriedObjects.Count;
        UpdateWeight();

        Destroy(item);
        GameLogger.Instance.LogDebug(this, $"아이템 소모: {item.name}");
    }

    // ===== Pick Up =====
    void TryPickUp()
    {
        if (carriedObjects.Count >= PlayerConstant.CarryableMaxCount)
        {
            Debug.Log("Cannot pick up: Max carry count reached");
            return;
        }

        var area = BuildForwardBox();
        Collider2D[] hits = Physics2D.OverlapBoxAll(area.pos, area.size, 0f, carryableMask);

        GameObject closest = FindClosestCarryable(hits);
        if (closest == null) return;

        SetupRigidOnPickup(closest);

        carriedObjects.Add(closest);
        collideCarrying++;

        if (closest.TryGetComponent<Carryable>(out var carryable))
        {
            carryable.carrying = true;
            InventoryManager.Instance.AddItem(carryable);
        }

        UpdateWeight();
    }

    // ===== Drop (Single Path) =====
    private bool DropAtIndex(int index)
    {
        if (index < 0 || index >= carriedObjects.Count) return false;

        GameObject obj = carriedObjects[index];

        // 파괴되어 null이면 즉시 정리
        if (!obj)
        {
            RemoveFromStack(index);
            return false;
        }

        if (!ComputeDropPose(obj, out Vector2 dropPos, out Vector2 dropSize)) return false;
        if (!CanPlace(dropPos, dropSize)) return false;

        PlaceAndRelease(obj, dropPos);
        UpdateInventoryOnDrop(obj);
        RemoveFromStack(index);
        return true;
    }

    // ===== World Interact (kept, but unified area calc) =====
    private bool TryUseItemOnWorld()
    {
        // 들고 있는 최상단 아이템 ID
        string heldItemId = null;
        if (carriedObjects.Count > 0)
        {
            var go0 = carriedObjects[0];
            if (!go0) { carriedObjects.RemoveAt(0); collideCarrying = carriedObjects.Count; UpdateWeight(); return false; }
            var top = go0.GetComponent<Carryable>();
            if (!top) { GameLogger.Instance.LogError(this, "0번 인덱스 아이템이 Carryable 아님"); return false; }
            heldItemId = top.Id;
        }

        var area = BuildForwardBox();
        Collider2D[] hits = Physics2D.OverlapBoxAll(area.pos, area.size, 0f, interactableMask);
        if (hits.Length == 0) return false;

        GameObject closestObj = null;
        float minD = Mathf.Infinity;
        WorldInteractable wi = null;

        foreach (var h in hits)
        {
            if (!h) continue;
            if (h.TryGetComponent<WorldInteractable>(out var temp))
            {
                float d = Vector2.Distance(transform.position, h.transform.position);
                if (d < minD)
                {
                    minD = d;
                    closestObj = h.gameObject;
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

    // ===== Helpers (Shared) =====

    // 1) 공통 쿨타임
    private bool EnsureCooldown()
    {
        if (Time.time - lastInteractTime < PlayerConstant.InteractCoolTime) return false;
        lastInteractTime = Time.time;
        return true;
    }

    // 2) 널 제거
    private bool PruneCarriedNulls()
    {
        bool removed = false;
        for (int i = carriedObjects.Count - 1; i >= 0; --i)
        {
            if (!carriedObjects[i])
            {
                carriedObjects.RemoveAt(i);
                removed = true;
            }
        }
        return removed;
    }

    // 3) 스택 제거 + 무게 갱신
    private void RemoveFromStack(int index)
    {
        carriedObjects.RemoveAt(index);
        collideCarrying = carriedObjects.Count;
        UpdateWeight();
    }

    // 4) 드롭 포즈 계산(공통)
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

    // 5) 배치 가능 여부(장애물 겹침)
    private bool CanPlace(Vector2 pos, Vector2 size)
    {
        int mask = obstacleMask.value;
        return Physics2D.OverlapBox(pos, size, 0f, mask) == null;
    }

    // 6) 실제 배치 + 해제
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
            c.carrying = false;
    }

    // 7) 인벤토리 드롭 반영
    private void UpdateInventoryOnDrop(GameObject obj)
    {
        if (obj.TryGetComponent<Carryable>(out var cy))
        {
            if (cy.GetItemName() != ItemName.None)
                InventoryManager.Instance.RemoveItem(cy.GetItemName());
        }
    }

    // 8) 전방 박스(픽업/인터랙트 공용)
    private (Vector2 pos, Vector2 size) BuildForwardBox()
    {
        int faceDir = GetFaceDir();
        pickUpPos = new Vector2(transform.position.x + (PlayerConstant.InteractableRange / 2f * faceDir),
                                transform.position.y);
        pickUpBox = new Vector2(PlayerConstant.InteractableRange, boxCollider2D.bounds.size.y * 1.1f);
        return (pickUpPos, pickUpBox);
    }

    // 9) 가장 가까운 Carryable 탐색
    private GameObject FindClosestCarryable(Collider2D[] hits)
    {
        GameObject closest = null;
        float minD = Mathf.Infinity;

        foreach (var h in hits)
        {
            if (!h) continue;
            if (!h.TryGetComponent<Carryable>(out var c)) continue;
            if (!c.enabled || c.carrying) continue;

            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < minD) { minD = d; closest = h.gameObject; }
            GameLogger.Instance.LogDebug(this, "집기 조작 " + h);
        }
        return closest;
    }

    // 10) 픽업 시 Rigidbody 상태 통일
    private static void SetupRigidOnPickup(GameObject go)
    {
        if (!go.TryGetComponent<Rigidbody2D>(out var rb)) return;

        var rot = go.transform.eulerAngles;
        rot.z = (rot.z < 90f || rot.z >= 270f) ? 0f : 180f;
        go.transform.eulerAngles = rot;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;
    }

    // 11) 스택 배치
    private void PlaceCarriedStack()
    {
        carryingTotalHeight = 0f;
        for (int i = 0; i < carriedObjects.Count; i++)
        {
            var go = carriedObjects[i];
            if (!go) continue;

            var col = go.GetComponent<Collider2D>();
            if (!col) continue;

            float objH = col.bounds.size.y;
            go.transform.position = holdPoint.position + new Vector3(0, carryingTotalHeight + objH / 2f, 0);
            carryingTotalHeight += objH;
        }
    }

    // 12) 크기 계산 (오브젝트/플레이어)
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

    // 13) 진행 방향
    private int GetFaceDir() => controller2D != null ? controller2D.collisions.faceDir : 1;
}
