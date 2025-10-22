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
    [SerializeField] Player player;//플레이어 스크립트

    private Vector2 lastObjSize;//들고있는 것중 제일 마지막 오브젝 간격
    public float pickUpRange = 0f;//들 수 있는 범위(x값) 없으면 스타트에서 내 콜라이더*2로 설정
    public int maxCarryCount = 3;
    public float stackOffsetY = 0.5f; // 오브젝트 간격
    Controller2D controller2D;
    BoxCollider2D playerCollider;
    private bool showDropGizmo = false;
    Vector2 lastDropPos;

    public int collideCarrying=0;//충돌한 짐 넘버 (현재 들고있는 것보다 높게 유지해야 안떨어짐)닿은거 이상 다 떨어질거야

    public List<GameObject> carriedObjects = new List<GameObject>();
    public List<Carryable> carryable = new List<Carryable>();

    [Header("Interaction Cooldown")]
    public float interactCooldown = 0.5f; // 쿨타임
    private float lastInteractTime = 0;

    private void Start()
    {
        if (player == null)
            player = GetComponent<Player>();
        // HoldPoint 생성
        GameObject hp = new GameObject("HoldPoint");
        hp.transform.parent = transform;
        hp.transform.localPosition = new Vector2(0, 0.5f);
        holdPoint = hp.transform;
        carryingTop = 0f; // 높이 초기화
        controller2D=GetComponent<Controller2D>();
        playerCollider=GetComponent<BoxCollider2D>();
        if (pickUpRange==0) pickUpRange =playerCollider.bounds.size.x * 2;//내 콜라이더*2

    }
    private void Update()
    {
        if (collideCarrying < carriedObjects.Count)//collideCarrying은 Carryable에서 충돌할 때마다 조정됨
        {   //짐이 충돌하여 그 넘버를 받으면
            CarryingDrop();
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

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastInteractTime >= interactCooldown)
        {
            lastInteractTime = Time.time;
            TryPickUp();
        }
    }

    void TryPickUp()
    {
        if (player.onLadder||!controller2D.collisions.below)//땅에 닿지 않거나 사다리 타는 중이면
        {//생각해보니 사다리 탈 때는 땅위가 아니니 하나만 체크해도 될듯?
            Debug.LogWarning("땅에 닿거나 사다리 타는 중에는 픽업 불가");
            return;
        }
        if (carriedObjects.Count >= maxCarryCount)
        {
            Debug.LogWarning("픽업 최대 수 초과");
            return;
        }

        // 주변 오브젝트 배열 가져오기
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            new Vector2(transform.position.x+ (pickUpRange/2*controller2D.collisions.faceDir), transform.position.y),//내 위치의 절반만큼 앞으로
            new Vector2(pickUpRange, playerCollider.bounds.size.y),0f, carryableMask);//내 높이와 픽업 범위만큼 체크
        GameObject closestObj = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
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
            if (carryable != null)
                carryable.carrying = true;

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
            #endregion
            WeightUpdate();
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastInteractTime >= interactCooldown)
        {
            lastInteractTime = Time.time; // 드랍 쿨타임

            if (carriedObjects.Count > 0)
            {
                GameObject obj = carriedObjects[carriedObjects.Count - 1];//젤 위에 들고있는 오브젝 가져옴.
                Debug.Log("log.드롭 위치:" + obj.transform);
                Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
                BoxCollider2D box = obj.GetComponent<BoxCollider2D>();
                Vector2 checkSize;
                float carru = obj.transform.localScale.y;
                if (box != null)
                    checkSize = box.size; // 실제 콜라이더 크기 사용
                else
                    checkSize = obj.transform.localScale;

                // 플레이어가 바라보는 방향에 드롭 위치 계산
                dropOffset = new Vector2((obj.transform.localScale.x+transform.localScale.x)/2,0);//들고있는 것 /2+플레이어 크기
                Vector2 dropPos = (Vector2)transform.position + dropOffset * controller2D.collisions.faceDir;

                //  기즈모용 위치 저장
                lastDropPos = dropPos;
                lastObjSize = obj.GetComponent<Collider2D>().bounds.size*0.9f;//사이즈
                showDropGizmo = true;

                //  레이캐스트로 드롭할 공간 확인
                Collider2D hit = Physics2D.OverlapBox(dropPos, lastObjSize,0, LayerMask.GetMask("Obstacle"));
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
        Gizmos.color = Color.yellow;//픽업 범위
        Gizmos.DrawWireCube(
            new Vector2(transform.position.x + (pickUpRange / 2 * controller2D.collisions.faceDir), transform.position.y),//내 위치의 절반만큼 앞으로
            new Vector2(pickUpRange, playerCollider.bounds.size.y));//영역
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
}
