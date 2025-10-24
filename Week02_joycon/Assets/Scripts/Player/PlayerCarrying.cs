using Game.Quests;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCarrying : MonoBehaviour
{
    [Header("Carry Settings")]
    public Transform holdPoint;
    public float carryingTop;//제일 위에 들고있는 위치
    public Vector2 dropOffset; // 플레이어 기준 드롭 위치
    public LayerMask carryableMask;
    public LayerMask maskObstacle;
    public float CarryAbleWeight;//들고있는 짐들 무게

    private Vector2 lastObjSize;//들고있는 것중 제일 마지막 오브젝 간격
    public float pickUpRange = 1.5f;
    public int maxCarryCount = 3;
    public float stackOffsetY = 0.5f; // 오브젝트 간격
    Controller2D controller2D;
    private bool showDropGizmo = false;
    Vector2 lastDropPos;
    Vector2 dropPos;
    float lastObjRadius = 0.25f;
    public int collideCarrying = 0;//충돌한 짐 넘버 (현재 들고있는 것보다 높게 유지해야 안떨어짐)닿은거 이상 다 떨어질거야
    BoxCollider2D playerCollider;

    public List<GameObject> carriedObjects = new List<GameObject>();
    public List<Carryable> carryable = new List<Carryable>();

    [Header("Interaction Cooldown")]
    public float interactCooldown = 0.5f; // 쿨타임
    private float lastInteractTime = 0;

    [Header("World Interaction")]
    [SerializeField] private float interactionRange = 1.5f; // 상호작용 감지 범위
    [SerializeField] private LayerMask interactableMask;    // "WorldInteractable" 레이어
    private Collider2D[] interactableHits = new Collider2D[3]; // 감지용 캐시
    private ContactFilter2D interactableFilter; // NonAlloc 방식 변경으로 이 필터가 필요합니다.

    [Header("General Interaction")]
    //[SerializeField] private LayerMask spriteChangerMask;
    private Player playerScript; // Player.cs 참조용
    //private ContactFilter2D spriteChangerFilter;
    //[SerializeField] private LayerMask generalInteractableMask;
    //private ContactFilter2D generalInteractableFilter;
    private void Start()
    {
        playerScript = GetComponent<Player>();
        if (playerScript == null)
        {
            GameLogger.Instance.LogError(this, "Player.cs 스크립트를 찾을 수 없습니다!");
        }
        // HoldPoint 생성
        GameObject hp = new GameObject("HoldPoint");
        hp.transform.parent = transform;
        hp.transform.localPosition = new Vector2(0, 0.5f);
        holdPoint = hp.transform;
        carryingTop = 0f; // 높이 초기화
        controller2D = GetComponent<Controller2D>();


        interactableFilter = new ContactFilter2D();
        interactableFilter.SetLayerMask(interactableMask); // 인스펙터에서 설정한 'WorldInteractable' 레이어를 사용
        interactableFilter.useTriggers = true; // 'Is Trigger'가 체크된 콜라이더도 감지

        //spriteChangerFilter = new ContactFilter2D();
        //spriteChangerFilter.SetLayerMask(spriteChangerMask);
        //spriteChangerFilter.useTriggers = true;


        if (playerCollider == null)
            playerCollider = GetComponent<BoxCollider2D>();

    }
    private void Update()
    {
        if (collideCarrying < carriedObjects.Count)
        {   //짐이 충돌하여 그 넘버를 받으면
            CarryingDrop();
        }
        if (collideCarrying < 0)
        {
            //Debug.Log(999);
        }
    }
    private void LateUpdate()
    {
        carryingTop = 0f; // 누적 높이 초기화
        for (int i = 0; i < carriedObjects.Count; i++)
        {
            if (carriedObjects[i] != null)
            {
                float objHeight = carriedObjects[i].GetComponent<Collider2D>().bounds.size.y;
                // holdPoint 위에 누적된 높이만큼 올려서 배치
                carriedObjects[i].transform.position = holdPoint.position + new Vector3(0, carryingTop + objHeight / 2f, 0);
                carryingTop += objHeight; // 다음 오브젝트를 위해 누적 높이 업데이트
            }
        }
    }

    public void TryInteract()
    {
        if (Time.time - lastInteractTime < interactCooldown) return; //만약 쿨타임 안지났으면 걍 종료
        lastInteractTime = Time.time; //만약 쿨타임 지난 상태면 지난 상호작용 시간을 지금 시간으로 설정
        GameLogger.Instance.LogDebug(this, "쿨타임 지났음");

        //if (TryChangeSprite())
        //{
        //    return; // 성공했으니 줍기/아이템사용 안 함
        //}

        if (TryUseItemOnWorld())
        {
            return; //만약 아이템과 상호작용을 시도하여 성공했으면 줍기 시도 X
        }


        TryPickUp();
    }

    void TryPickUp()
    {
        if (carriedObjects.Count >= maxCarryCount)
        {
            Debug.Log("Cannot pick up: Max carry count reached");
            return;
        }
        // 주변 오브젝트 배열 가져오기
        Collider2D[] hits = Physics2D.OverlapBoxAll(
        new Vector2(transform.position.x + (pickUpRange / 2 * controller2D.collisions.faceDir), transform.position.y),//내 위치의 절반만큼 앞으로
        new Vector2(pickUpRange, playerCollider.bounds.size.y), 0f, carryableMask);//내 높이와 픽업 범위만큼 체크
        GameObject closestObj = null;
        float minDistance = Mathf.Infinity;
        foreach (Collider2D hit in hits)
        {
            GameLogger.Instance.LogDebug(this, "집기 조작" + hit);
            Carryable carryable = hit.GetComponent<Carryable>();
            if (carryable != null && carryable.carrying)
            {
                continue; // 이미 들고 있는 오브젝트는 무시
            }

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObj = hit.gameObject;
            }
        }

        // PickUp 가능한 가장 가까운 오브젝트가 있으면 처리
        if (closestObj != null)
        {
            Rigidbody2D rb = closestObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Z 회전 정리
                Vector3 rot = closestObj.transform.eulerAngles;
                if (rot.z < 90f || rot.z >= 270f)
                    rot.z = 0f;
                else
                    rot.z = 180f;
                closestObj.transform.eulerAngles = rot;

                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = true;
            }

            carriedObjects.Add(closestObj);
            collideCarrying++;//충돌 할 수 있는 물체+ (최대치+1유지해야 안떨어짐)
            Carryable carryable = closestObj.GetComponent<Carryable>();
            if (carryable != null) carryable.carrying = true;
            if (carryable.GetItemName() != ItemName.None)
                InventoryManager.Instance.AddItem(carryable.GetItemName(), closestObj);

            #region Events
            Interactable2D _focus;
            _focus = closestObj.GetComponent<Interactable2D>();

            if (_focus != null)
            {
                // 선행 조건 재검증
                if (!_focus.HasRequiredFlags(QuestFlags.Has)) return;
                if (!_focus.CheckItem(Inventory.HasItem)) return;

                QuestEvents.RaiseInteract(_focus.Id, _focus.transform.position, InteractionKind.Press);
            }
            else
            {
                Debug.Log("_focus is null");
            }

            #endregion
            WeightUpdate();
        }
    }

    public void TryDrop()
    {
        if (Time.time - lastInteractTime < interactCooldown) return;

        lastInteractTime = Time.time; // 드랍 쿨타임

        if (carriedObjects.Count > 0)
        {
            GameObject obj = carriedObjects[carriedObjects.Count - 1];//젤 위에 들고있는 오브젝
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            BoxCollider2D box = obj.GetComponent<BoxCollider2D>();
            Vector2 checkSize;
            float carru = obj.transform.localScale.y;
            if (box != null)
                checkSize = box.size; // 실제 콜라이더 크기 사용
            else
                checkSize = obj.transform.localScale;

            // 플레이어가 바라보는 방향에 드롭 위치 계산
            dropOffset = new Vector2((obj.transform.localScale.x + transform.localScale.x) / 2, 0);//들고있는 것 /2+플레이어 크기
            Vector2 dropPos = (Vector2)transform.position + dropOffset * controller2D.collisions.faceDir;

            //  기즈모용 위치 저장
            lastDropPos = dropPos;
            lastObjSize = obj.GetComponent<Collider2D>().bounds.size * 0.9f;//사이즈
            showDropGizmo = true;

            //  레이캐스트로 드롭할 공간 확인
            Collider2D hit = Physics2D.OverlapBox(dropPos, lastObjSize, 0, LayerMask.GetMask("Obstacle"));
            if (hit != null)
            {
                Debug.Log("막혔어");
                return; // 벽에 막혀 있으면 드롭 취소
            }

            //  안전한 위치라면 드롭 진행
            if (rb != null)
            {
                rb.transform.position = dropPos;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = false;
            }

            Carryable carryable = obj.GetComponent<Carryable>();
            if (carryable != null)
                carryable.carrying = false;

            carriedObjects.RemoveAt(carriedObjects.Count - 1);
        }

        WeightUpdate();
    }


    public void CarryingDrop()
    {
        int count = carriedObjects.Count;
        if (count == 0) { collideCarrying = 0; return; }

        // collideCarrying가 음수/과대일 수 있으니 방어
        int startIndex = Mathf.Clamp(collideCarrying, 0, count);

        // 위에서부터(리스트 끝) collideCarrying 인덱스까지 제거
        for (int i = count - 1; i >= startIndex; --i)
        {
            var go = carriedObjects[i];
            if (!go) { carriedObjects.RemoveAt(i); continue; }

            if (go.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.freezeRotation = false;
            }


            if (go.TryGetComponent<Carryable>(out var car))
                car.carrying = false;

            carriedObjects.RemoveAt(i);
        }

        // 남은 개수에 맞춰 정리
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
    }

    public void WeightUpdate()
    {
        CarryAbleWeight = 0;//들고 있는 것 초기화
        if (carriedObjects.Count <= 0)
        {
            return;//들고있는게 없으면 리턴
        }
        else
        {
            for (int i = 0; i < carriedObjects.Count; i++)
            {
                CarryAbleWeight += carriedObjects[i].GetComponent<Carryable>().weight;
            }
        }
    }

    ///<summary>0번 슬롯의 아이템으로 주변 사물과 상호작용을 시도</summary>
    ///상호작용 시도라도 했으면 -> True, 상호작용 시도할 객체도 없었으면 -> False
    private bool TryUseItemOnWorld()
    {
        // 1. 들고 있는 아이템 ID 가져오기 (맨손이면 null)
        string heldItemId = null;
        if (carriedObjects.Count > 0)
        {
            Carryable topItem = carriedObjects[0].GetComponent<Carryable>();
            if (topItem == null) { GameLogger.Instance.LogError(this, "0번 인덱스에 있는 아이템이 carryable이 아님"); return false; }
            heldItemId = topItem.Id;
        }

        // 2. [수정] TryPickUp과 동일한 사각형 범위로 'interactableMask' 레이어 감지
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            new Vector2(transform.position.x + (pickUpRange / 2 * controller2D.collisions.faceDir), transform.position.y),
            new Vector2(pickUpRange, playerCollider.bounds.size.y), 0f, interactableMask); // *<- interactableMask 사용*

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

        carriedObjects.RemoveAt(index);
        collideCarrying = carriedObjects.Count;
        WeightUpdate();

        Destroy(itemToConsume);
        GameLogger.Instance.LogDebug(this, $"아이템 소모: {itemToConsume.name}");
    }
    /// <summary>
    /// 주변의 'SpriteChangeStation'과 상호작용을 시도합니다.
    /// </summary>
    //private bool TryChangeSprite()
    //{
    //    // 1. 스프라이트 변경 오브젝트 감지
    //    // (이름을 interactionHits로 바꾼 범용 캐시 배열 사용)
    //    int hitCount = Physics2D.OverlapCircle(transform.position, interactionRange, spriteChangerFilter, interactableHits);

    //    if (hitCount > 0)
    //    {
    //        // 2. 스크립트 가져오기 (가장 가까운 것 - 0번 인덱스)
    //        PlayerSpriteChanger station = interactableHits[0].GetComponent<PlayerSpriteChanger>();
    //        if (station != null)
    //        {
    //            // 3. 내 Player.cs 스크립트의 함수 호출
    //            station.ChangeSprite(playerScript); // 스테이션에 Player 참조를 넘겨줌
    //            return true; // 상호작용 성공
    //        }
    //    }
    //    return false; // 상호작용할 스테이션 없음
    //}

    /////<summary>맨손으로 일반 상호작용을 시도</summary>
    //private bool TryGeneralInteract()
    //{
    //    int hitCount = Physics2D.OverlapCircle(transform.position, interactionRange, generalInteractableFilter, interactableHits);
    //    if (hitCount > 0)
    //    {
    //        // 가장 가까운 오브젝트의 IInteractable 가져오기
    //        Collider2D closestHit = interactableHits[0];
    //        // (참고: hitCount > 1일 때 진짜 가장 가까운 것을 찾는 로직 추가 가능)

    //        if (closestHit.TryGetComponent<IInteractable>(out IInteractable interactable))
    //        {
    //            interactable.Interact(); // Interact() 함수 호출
    //            GameLogger.Instance.LogDebug(this, $"일반 상호작용 성공: {closestHit.name}");
    //            return true; // 상호작용 성공
    //        }
    //    }
    //    return false; // 상호작용할 IInteractable이 없음
    //}

}