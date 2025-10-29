using Game.Quests;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarrying : MonoBehaviour
{
    [Header("Carry Settings")]
    public Transform holdPoint;//물채를 집어서 머뤼 위에 놓을 위치(집는 물건마다 갱신)
    public float carryingTop;//들고있는 물건들의 높이 합
    public Vector2 dropOffset;//내려둘 위치
    public LayerMask carryableMask;
    public LayerMask maskObstacle;
    public float CarryAbleWeight;

    private Vector2 lastObjSize;
    public float pickUpRange = 1.5f;
    public int maxCarryCount = 3;
    public float stackOffsetY = 0.5f;
    Controller2D controller2D;
    private bool showDropGizmo = false;
    Vector2 pickUpPos;
    Vector2 pickUpBox;
    Vector2 lastDropPos;
    Vector2 dropPos;
    public int collideCarrying = 0;//충돌한 짐 넘버 (현재 들고있는 것보다 높게 유지해야 안떨어짐)닿은거 이상 다 떨어질거야
    BoxCollider2D boxCollider2D;

    public List<GameObject> carriedObjects = new List<GameObject>();
    public List<Carryable> carryable = new List<Carryable>();

    [Header("Interaction Cooldown")]
    public float interactCooldown = 0.5f;
    private float lastInteractTime = 0;

    [Header("World Interaction")]
    [SerializeField] private LayerMask interactableMask;
    private ContactFilter2D interactableFilter;

    [Header("General Interaction")]
    private Player playerScript;

    private void Start()
    {
        playerScript = GetComponent<Player>();
        if (playerScript == null)
            GameLogger.Instance.LogError(this, "Player.cs 스크립트를 찾을 수 없습니다!");

        var hp = new GameObject("HoldPoint");
        hp.transform.parent = transform;
        hp.transform.localPosition = new Vector2(0, 0.5f);
        holdPoint = hp.transform;

        carryingTop = 0f;
        controller2D = GetComponent<Controller2D>();

        interactableFilter = new ContactFilter2D();
        interactableFilter.SetLayerMask(interactableMask);
        interactableFilter.useTriggers = true;

        if (boxCollider2D == null)
            boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        bool removedNull = false;
        for (int i = carriedObjects.Count - 1; i >= 0; --i)
        {
            if (carriedObjects[i] == null)
            {
                carriedObjects.RemoveAt(i);
                removedNull = true;
            }
        }
        if (removedNull)
        {
            collideCarrying = carriedObjects.Count;
            UpdateWeight();
        }

        if (collideCarrying < carriedObjects.Count)
            CarryingDrop();
    }

    private void LateUpdate()
    {
        carryingTop = 0f;
        for (int i = 0; i < carriedObjects.Count; i++)
        {
            var go = carriedObjects[i];
            if (!go) continue;

            var col = go.GetComponent<Collider2D>();
            if (!col) continue;

            float objHeight = col.bounds.size.y;
            go.transform.position = holdPoint.position + new Vector3(0, carryingTop + objHeight / 2f, 0);
            carryingTop += objHeight;
        }
    }

    public void TryInteract()
    {
        if (Time.time - lastInteractTime < interactCooldown) return;
        lastInteractTime = Time.time;

        if (TryUseItemOnWorld() == true) return;
        TryPickUp();
    }

    void TryPickUp()
    {
        if (carriedObjects.Count >= maxCarryCount)
        {
            Debug.Log("Cannot pick up: Max carry count reached");
            return;
        }
        pickUpPos = new Vector2(transform.position.x + (pickUpRange / 2 * controller2D.collisions.faceDir), transform.position.y);//내 위치의 절반만큼 앞으로
        pickUpBox = new Vector2(pickUpRange, boxCollider2D.bounds.size.y * 1.1f);//내 높이*1.1f 와 픽업 범위만큼 체크
        // 주변 오브젝트 배열 가져오기
        Collider2D[] hits = Physics2D.OverlapBoxAll(pickUpPos, pickUpBox, 0f, carryableMask);
        GameObject closestObj = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider2D hit in hits)
        {
            GameLogger.Instance.LogDebug(this, "집기 조작" + hit);
            Carryable c = hit.GetComponent<Carryable>();
            if (c.enabled == false) continue;
            if (c != null && c.carrying) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObj = hit.gameObject;
            }
        }

        if (closestObj != null)
        {
            Rigidbody2D rb = closestObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                var rot = closestObj.transform.eulerAngles;
                rot.z = (rot.z < 90f || rot.z >= 270f) ? 0f : 180f;
                closestObj.transform.eulerAngles = rot;

                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = true;
            }

            carriedObjects.Add(closestObj);
            collideCarrying++;
            Carryable cy = closestObj.GetComponent<Carryable>();
            if (cy != null) cy.carrying = true;
            if (cy != null && cy.GetItemName() != ItemName.None)
                InventoryManager.Instance.AddItem(cy.GetItemName(), closestObj);

            // ---- Quest 연동 (ENUM ONLY) ----
            var it = closestObj.GetComponent<Interactable2D>();
            if (it != null)
            {
                if (!it.HasRequiredFlags(QuestFlags.Has)) return;
                QuestEvents.RaiseInteract(it.IdEnum, it.transform.position, InteractionKind.Press);
            }
            else
            {
                Debug.Log("_focus is null");
            }
            // ---------------------------------

            UpdateWeight();
        }
    }

    public void TryDrop()
    {
        if (Time.time - lastInteractTime < interactCooldown) return;
        lastInteractTime = Time.time;

        if (carriedObjects.Count <= 0)
        {
            UpdateWeight();
            return;
        }

        // 스택의 최상단(마지막) 아이템
        GameObject obj = carriedObjects[carriedObjects.Count - 1];

        // 파괴되어 null이면 즉시 정리
        if (obj == null)
        {
            carriedObjects.RemoveAt(carriedObjects.Count - 1);
            collideCarrying = carriedObjects.Count; // 필요 시 유지되는 필드
            UpdateWeight();
            return;
        }

        // 필요한 컴포넌트들
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        var objSize = GetBoundsSize(obj);        // 드롭할 오브젝트의 월드 크기
        var playerSize = GetPlayerBoundsSize();     // 플레이어의 월드 크기

        // faceDir: -1(왼쪽)/+1(오른쪽) 가정
        int faceDir = controller2D != null ? controller2D.collisions.faceDir : 1;

        // 드롭 오프셋 계산 (기존 수식 일반화)
        dropOffset = new Vector2(
            (objSize.x + playerSize.x) * faceDir / 2f,
            (objSize.y - playerSize.y) / 2f + 0.05f
        );

        Vector2 dropPos = (Vector2)transform.position + dropOffset;

        // 기즈모/디버그용 기록
        lastDropPos = dropPos;
        lastObjSize = objSize * 1.0f;     // 필요하면 0.95f 같은 여유값 적용 가능
        showDropGizmo = true;

        // 겹침(막힘) 체크: 장애물 레이어와 충돌?
        // maskObstacle은 LayerMask 필드(예: "Obstacle" 포함)라고 가정
        int mask = maskObstacle.value;
        Collider2D hit = Physics2D.OverlapBox(dropPos, lastObjSize, 0f, mask);

        if (hit != null) return;

        obj.transform.SetParent(null, true);
        obj.transform.position = dropPos;

        if (rb != null)
        {
            rb.transform.SetParent(null, true);
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Carryable 상태 갱신
        if (obj.TryGetComponent<Carryable>(out var carryable))
        {
            carryable.carrying = false;
            if (carryable.GetItemName() != ItemName.None)
            {
                InventoryManager.Instance.RemoveItem(carryable.GetItemName());
            }
        }

        // 스택에서 제거 및 부가 상태 갱신
        carriedObjects.RemoveAt(carriedObjects.Count - 1);
        collideCarrying = carriedObjects.Count; // 유지되는 카운터라면 업데이트
        UpdateWeight();
    }

    private bool DropAtIndex(int index)
    {
        if (index < 0 || index >= carriedObjects.Count) return false;

        GameObject obj = carriedObjects[index];

        // 파괴되어 null이면 즉시 정리만
        if (obj == null)
        {
            carriedObjects.RemoveAt(index);
            collideCarrying = carriedObjects.Count;
            UpdateWeight();
            return false;
        }

        // 필요한 컴포넌트/크기 계산
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        var objSize = GetBoundsSize(obj);
        var playerSize = GetPlayerBoundsSize();
        int faceDir = controller2D != null ? controller2D.collisions.faceDir : 1;

        // 기존 TryDrop과 동일한 드롭 오프셋 계산
        dropOffset = new Vector2(
            (objSize.x + playerSize.x) * faceDir / 2f,
            (objSize.y - playerSize.y) / 2f + 0.05f
        );

        Vector2 dropPos = (Vector2)transform.position + dropOffset;

        // 디버그용 기록
        lastDropPos = dropPos;
        lastObjSize = objSize * 1.0f;
        showDropGizmo = true;

        // 장애물 겹침 체크
        int mask = maskObstacle.value;
        Collider2D hit = Physics2D.OverlapBox(dropPos, lastObjSize, 0f, mask);
        if (hit != null)
        {
            // 막혀 있으면 드롭 실패 (자리를 못 잡음)
            return false;
        }

        // 실제 드롭
        if (rb != null)
        {
            rb.transform.SetParent(null, true);
            rb.position = dropPos;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            obj.transform.SetParent(null, true);
            obj.transform.position = dropPos;
        }

        // Carryable/인벤토리 갱신
        if (obj.TryGetComponent<Carryable>(out var cy))
        {
            cy.carrying = false;
            if (cy.GetItemName() != ItemName.None)
                InventoryManager.Instance.RemoveItem(cy.GetItemName());
        }

        carriedObjects.RemoveAt(index);
        collideCarrying = carriedObjects.Count;
        UpdateWeight();
        return true;
    }

    // 특정 ItemName을 가진 아이템을 찾아(상단부터 검색) 드롭
    public bool TryDrop(ItemName target)
    {
        // 인터랙션 쿨다운 체크 (기존 TryDrop과 동일 정책)
        if (Time.time - lastInteractTime < interactCooldown) return false;
        lastInteractTime = Time.time;

        if (carriedObjects.Count == 0) { UpdateWeight(); return false; }

        // 스택 상단(마지막 인덱스)부터 내려가며 해당 아이템 탐색
        for (int i = carriedObjects.Count - 1; i >= 0; --i)
        {
            var go = carriedObjects[i];
            if (!go) { carriedObjects.RemoveAt(i); continue; }

            if (go.TryGetComponent<Carryable>(out var c))
            {
                if (c.GetItemName() == target)
                {
                    // 찾으면 해당 인덱스 드롭 시도
                    bool dropped = DropAtIndex(i);
                    return dropped;
                }
            }
        }

        // 못 찾음
        return false;
    }

    public void TryDrop(GameObject obj)
    {
        if (Time.time - lastInteractTime < interactCooldown) return;
        lastInteractTime = Time.time;

        if (carriedObjects.Count <= 0)
        {
            UpdateWeight();
            return;
        }

        int index = carriedObjects.IndexOf(obj);
        if (index == -1)
        {
            Debug.Log("해당 오브젝트를 들고 있지 않습니다.");
            return;
        }

        // 해당 인덱스 드롭 시도
        DropAtIndex(index);
    }


    // 클래스 내부 어딘가에 같이 추가 (재사용 헬퍼)
    private static Vector2 GetBoundsSize(GameObject go)
    {
        if (go == null) return Vector2.zero;

        // 가장 신뢰도 높은: Collider2D
        if (go.TryGetComponent<Collider2D>(out var col))
        {
            var s = col.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }

        // 그다음: SpriteRenderer
        if (go.TryGetComponent<SpriteRenderer>(out var sr))
        {
            var s = sr.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }

        // 최후 폴백: 월드 스케일(1유닛 = 1m 가정)
        var ls = go.transform.lossyScale;
        return new Vector2(Mathf.Abs(ls.x), Mathf.Abs(ls.y));
    }

    private Vector2 GetPlayerBoundsSize()
    {
        // 플레이어 콜라이더가 따로 있으면 우선 사용
        if (boxCollider2D != null)
        {
            var s = boxCollider2D.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }

        // 없으면 자기 자신 임의 콜라이더
        if (TryGetComponent<Collider2D>(out var selfCol))
        {
            var s = selfCol.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }

        // 폴백: 렌더러 → 스케일
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            var s = sr.bounds.size;
            if (s != Vector3.zero) return (Vector2)s;
        }

        var ls = transform.lossyScale;
        return new Vector2(Mathf.Abs(ls.x), Mathf.Abs(ls.y));
    }


    public void CarryingDrop()
    {
        int count = carriedObjects.Count;
        if (count == 0) { collideCarrying = 0; return; }

        int startIndex = Mathf.Clamp(collideCarrying, 0, count);//짐 떨어트릴 시작 값
        for (int i = count - 1; i >= startIndex; --i)
        {
            var go = carriedObjects[i];
            if (go == null) { carriedObjects.RemoveAt(i); continue; }

            if (go.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.bodyType = RigidbodyType2D.Dynamic;//떨어뜨릴때 원상복구
                rb.freezeRotation = false;
            }

            if (go.TryGetComponent<Carryable>(out var car))
                car.carrying = false;

            carriedObjects.RemoveAt(i);
        }

        collideCarrying = carriedObjects.Count;
        UpdateWeight();
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

    public void UpdateWeight()
    {
        CarryAbleWeight = 0;

        if (carriedObjects.Count <= 0) return;

        for (int i = 0; i < carriedObjects.Count; i++)
        {
            var c = carriedObjects[i]?.GetComponent<Carryable>();
            if (c) CarryAbleWeight += c.weight;
        }
    }

    /// <summary>
    /// (배달/아이템 기반 상호작용 비활성화) — 현 퀘스트 시스템은 enum-only이므로 이 경로는 사용 안 함.
    /// </summary>
    private bool TryUseItemOnWorld()
    {
        // 1. 들고 있는 아이템 ID 가져오기 (맨손이면 null)
        string heldItemId = null;
        if (carriedObjects.Count > 0)
        {
            var go0 = carriedObjects[0];
            if (go0 == null)
            {
                // 외부 파괴 → 정리 후 실패 처리
                carriedObjects.RemoveAt(0);
                collideCarrying = carriedObjects.Count;
                UpdateWeight();
                return false;
            }

            Carryable topItem = go0.GetComponent<Carryable>();
            if (topItem == null) { GameLogger.Instance.LogError(this, "0번 인덱스에 있는 아이템이 carryable이 아님"); return false; }
            heldItemId = topItem.Id;
        }

        // 2. [수정] TryPickUp과 동일한 사각형 범위로 'interactableMask' 레이어 감지
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            new Vector2(transform.position.x + (pickUpRange / 2 * controller2D.collisions.faceDir), transform.position.y),
            new Vector2(pickUpRange, boxCollider2D.bounds.size.y), 0f, interactableMask); // *<- interactableMask 사용*

        if (hits.Length == 0) return false; // 상호작용할 오브젝트 없음

        // 3. [추가] 감지된 것들 중 가장 가까운 WorldInteractable 오브젝트 찾기
        GameObject closestObj = null;
        float minDistance = Mathf.Infinity;
        WorldInteractable interactable = null; // 가장 가까운 오브젝트의 스크립트

        foreach (Collider2D hit in hits)
        {
            // WorldInteractable 스크립트가 있는지 확인
            if (hit.TryGetComponent<WorldInteractable>(out WorldInteractable tempInteractable))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestObj = hit.gameObject;
                    interactable = tempInteractable; // 가장 가까운 오브젝트 정보 저장
                }
            }
        }

        // 4. [수정] 가장 가까운 스크립트를 찾았으면 상호작용 시도
        if (interactable != null)
        {
            GameLogger.Instance.LogDebug(this, $"WorldInteractable 감지: {closestObj.name}");
            bool success = interactable.AttemptInteraction(heldItemId, this); // 상호작용 시도

            if (success)
            {
                GameLogger.Instance.LogDebug(this, "상호작용 성공함!!");
                return true; // 성공했으므로 true 반환 (TryPickUp 실행 안 됨)
            }
            else
            {
                GameLogger.Instance.LogDebug(this, "상호작용 실패함 (아이템 불일치 등). 줍기 시도.");
                return false; // 실패했으므로 false 반환 (TryPickUp 실행됨)
            }
        }

        return false; // 감지된 콜라이더에 WorldInteractable 스크립트가 없었음
    }

    public void ConsumeItem(int index)
    {
        if (index < 0 || index >= carriedObjects.Count) return;
        GameObject itemToConsume = carriedObjects[index];
        if (itemToConsume == null)
        {
            carriedObjects.RemoveAt(index);
            collideCarrying = carriedObjects.Count;
            UpdateWeight();
            return;
        }

        carriedObjects.RemoveAt(index);
        collideCarrying = carriedObjects.Count;
        UpdateWeight();

        Destroy(itemToConsume);
        GameLogger.Instance.LogDebug(this, $"아이템 소모: {itemToConsume.name}");
    }
}
