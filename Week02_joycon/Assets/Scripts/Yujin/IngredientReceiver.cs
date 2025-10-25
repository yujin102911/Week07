using UnityEngine;

/// <summary>
/// Pot의 자식 오브젝트에게 붙어서 토마토 감지
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class IngredientReceiver : MonoBehaviour
{
    private Pot parentPot; //부모 냄비
    [SerializeField]private string RequiredId = "Tomato";

    #region Unity Lifecycle
    private void Start()
    {
        parentPot = GetComponentInParent<Pot>();
        if (parentPot == null)
        {
            GameLogger.Instance.LogWarning(this, "부모 오브젝트에서 Pot.cs를 찾을 수 없음");
        }
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            GameLogger.Instance.LogWarning(this, $"{gameObject.name}에게 IsTrigger가 안켜져있음");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (parentPot == null) return; //부모 냄비 없으면 중단

        if (other.TryGetComponent<Carryable>(out Carryable carryable)) //들어온게 Carryable이면
        {
            if (carryable.carrying) return;

            if (carryable.Id == RequiredId)
            {
                parentPot.AddTomato();
                Destroy(other.gameObject);
            }
        }

    }
    #endregion
}
