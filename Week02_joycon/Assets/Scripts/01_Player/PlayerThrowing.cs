using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerThrowing : MonoBehaviour
{
    [SerializeField] float throwForce = 10;
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
        if (Time.time - lastThrowTime < PlayerConstant.InteractCoolTime) return;
        lastThrowTime = Time.time;//던지는 타임 쿨타임 갱신, 쿨타임 없으면 유니티 병신 인풋 시스템이 한번 눌러도 3번 호출됨 ㅅㅂ

        var ownedItems = InventoryManager.Instance.GetOwnedItems();
        if (ownedItems.Count <= 0) return;

        GameObject obj = ownedItems[ownedItems.Count - 1].gameObject;

        if (obj.TryGetComponent(out Carryable carryable) == true) carryable.SetIsCarried(false);
        if (obj.TryGetComponent(out Rigidbody2D rigidbody) == true)
        {
            
            var force = Vector2.right * Player.GetFaceDir() * throwForce;
            force.x += Player.Instance.velocity.x;
            rigidbody.AddForce(force, ForceMode2D.Impulse);
            carryable.spinAngle = Mathf.Abs(carryable.spinAngle) * Player.GetFaceDir();//던질때 회전 방향 설정
            carryable.throwing = true;
        }
    }
}