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
    public float carryingTop;
    public Vector2 dropOffset;
    public LayerMask carryableMask;
    public LayerMask maskObstacle;
    public float CarryAbleWeight;

    private Vector2 lastObjSize;
    public float pickUpRange = 1.5f;
    public int maxCarryCount = 3;
    public float stackOffsetY = 0.5f;
    Controller2D controller2D;
    private bool showDropGizmo = false;
    Vector2 lastDropPos;
    Vector2 dropPos;
    float lastObjRadius = 0.25f;
    public int collideCarrying = 0;
    BoxCollider2D playerCollider;

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

        if (playerCollider == null)
            playerCollider = GetComponent<BoxCollider2D>();
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
        var hits = Physics2D.OverlapBoxAll(
            new Vector2(transform.position.x + (pickUpRange / 2 * controller2D.collisions.faceDir), transform.position.y),
            new Vector2(pickUpRange, playerCollider.bounds.size.y), 0f, carryableMask);

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
            BoxCollider2D box = obj.GetComponent<BoxCollider2D>();
            Vector2 checkSize = box ? box.size : obj.transform.localScale;

            dropOffset = new Vector2((obj.transform.localScale.x + transform.localScale.x) / 2, 0);
            Vector2 dropPos = (Vector2)transform.position + dropOffset * controller2D.collisions.faceDir;

            lastDropPos = dropPos;
            var objCol = obj.GetComponent<Collider2D>();
            lastObjSize = objCol ? objCol.bounds.size * 0.9f : Vector2.one * 0.5f;
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

        int startIndex = Mathf.Clamp(collideCarrying, 0, count);
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
        CarryAbleWeight = 0;
        for (int i = carriedObjects.Count - 1; i >= 0; --i)
            if (carriedObjects[i] == null) carriedObjects.RemoveAt(i);

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
        return false;
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
