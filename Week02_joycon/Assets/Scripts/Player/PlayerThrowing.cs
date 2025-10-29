using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerThrowing : MonoBehaviour
{
    [SerializeField] Player    player;
    [SerializeField] PlayerCarrying playerCarrying;
    [SerializeField] Controller2D   controller2D;
    [SerializeField] float          throwForce=10;
    float lastThrowTime = 0f;

    public InputActionReference throwAction; // 액션 컴포넌트에서 참조

    private void OnEnable()
    {
        throwAction.action.performed += OnThrowPerformed;
        throwAction.action.Enable();
    }

    private void OnDisable()
    {
        throwAction.action.performed -= OnThrowPerformed;
        throwAction.action.Disable();
    }

    private void OnThrowPerformed(InputAction.CallbackContext ctx)
    {
        // 눌렀을 때만 처리
        if (!ctx.ReadValueAsButton()) return;
        OnThrow();
    }

    public void OnThrow()
    {
        if (Time.time - lastThrowTime < playerCarrying.interactCooldown) return;
        lastThrowTime = Time.time;//던지는 타임 쿨타임 갱신, 쿨타임 없으면 유니티 병신 인풋 시스템이 한번 눌러도 3번 호출됨 ㅅㅂ
        if (playerCarrying.carriedObjects.Count <= 0)
        {
            Debug.Log("던질거 없음");
            return;
        }

        // 스택의 최상단(마지막) 아이템 정보 획득
        GameObject obj = playerCarrying.carriedObjects[playerCarrying.carriedObjects.Count - 1];
        Debug.Log(obj);
        // 필요한 컴포넌트들
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            int faceDir = controller2D != null ? controller2D.collisions.faceDir : 1;//플레이어가 바라보는 방향
            
            rb.transform.SetParent(null, true);//<<부모로 설정하는 것도 없는데 왜 있지??
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.freezeRotation = false;
            rb.AddForce( (Vector2.right * faceDir * (throwForce) + new Vector2(player.velocity.x,0f)), ForceMode2D.Impulse);//던지기 힘 적용
            Debug.Log(rb.linearVelocity);
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
        playerCarrying.carriedObjects.RemoveAt(playerCarrying.carriedObjects.Count - 1);
        playerCarrying.collideCarrying = playerCarrying.carriedObjects.Count; // 유지되는 카운터라면 업데이트<<??
        playerCarrying.UpdateWeight();
    }
}
