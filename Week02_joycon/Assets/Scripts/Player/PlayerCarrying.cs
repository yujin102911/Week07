using Game.Quests;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
    float lastObjRadius = 0.25f;
    public int collideCarrying = 0;//충돌한 짐 넘버 (현재 들고있는 것보다 높게 유지해야 안떨어짐)닿은거 이상 다 떨어질거야
    BoxCollider2D boxCollider2D;

    public List<GameObject> carriedObjects = new List<GameObject>();
    public List<Carryable> carryable = new List<Carryable>();

    [Header("Interaction Cooldown")]
    public float interactCooldown = 0.5f;
    private float lastInteractTime = 0;

    [Header("World Interaction")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private LayerMask interactableMask;
    private Collider2D[] interactableHits = new Collider2D[3];
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
            WeightUpdate();
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

        // (배달/아이템 상호작용 시스템 비활성) → 바로 픽업 시도
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

            WeightUpdate();
        }
    }

    public void TryDrop()
    {
        if (Time.time - lastInteractTime < interactCooldown) return;
        lastInteractTime = Time.time;

        if (carriedObjects.Count > 0)
        {
            GameObject obj = carriedObjects[carriedObjects.Count - 1];
            if (obj == null)
            {
                carriedObjects.RemoveAt(carriedObjects.Count - 1);
                collideCarrying = carriedObjects.Count;
                WeightUpdate();
                return;
            }

            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            SpriteRenderer box = obj.GetComponent<SpriteRenderer>();
            Vector2 checkSize;
            if (box != null)
                checkSize = box.size; // 실제 콜라이더 크기 사용
            else
                checkSize = obj.transform.localScale;

            // 플레이어가 바라보는 방향에 드롭 위치 계산
            dropOffset = new Vector2((box.bounds.size.x + boxCollider2D.bounds.size.x) * controller2D.collisions.faceDir / 2, (box.bounds.size.y - boxCollider2D.bounds.size.y) / 2 + 0.05f);//들고있는 것/2 +플레이어 크기
            Vector2 dropPos = (Vector2)transform.position + dropOffset;

            lastDropPos = dropPos;
            lastObjSize = box ? box.bounds.size * 1f : Vector2.one * 0.5f;//사이즈
            showDropGizmo = true;

            Collider2D hit = Physics2D.OverlapBox(dropPos, lastObjSize, 0, LayerMask.GetMask("Obstacle"));
            if (hit != null)
            {
                Debug.Log("막혔어");
                return;
            }

            if (rb != null)
            {
                rb.transform.position = dropPos;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = false;
            }

            Carryable carryable = obj.GetComponent<Carryable>();
            if (carryable != null) carryable.carrying = false;

            carriedObjects.RemoveAt(carriedObjects.Count - 1);
        }

        WeightUpdate();
    }

    public void CarryingDrop()
    {
        int count = carriedObjects.Count;
        if (count == 0) { collideCarrying = 0; return; }

        int startIndex = Mathf.Clamp(collideCarrying, 0, count);//짐 떨어트릴 시작 값
        for (int i = count - 1; i >= startIndex; --i)
        {
            var go = carriedObjects[i];
            if (go==null) { carriedObjects.RemoveAt(i); continue; }

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
        WeightUpdate();
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

    public void WeightUpdate()
    {
        CarryAbleWeight = 0;
        Debug.Log("Weight Update =" + CarryAbleWeight);

        if (carriedObjects.Count <= 0) return;

        for (int i = 0; i < carriedObjects.Count; i++)
        {
            var c = carriedObjects[i]?.GetComponent<Carryable>();
            if (c) CarryAbleWeight += c.weight;
        }
        Debug.Log("Weight Update2 =" + CarryAbleWeight);
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
                WeightUpdate();
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
            WeightUpdate();
            return;
        }

        carriedObjects.RemoveAt(index);
        collideCarrying = carriedObjects.Count;
        WeightUpdate();

        Destroy(itemToConsume);
        GameLogger.Instance.LogDebug(this, $"아이템 소모: {itemToConsume.name}");
    }
}
