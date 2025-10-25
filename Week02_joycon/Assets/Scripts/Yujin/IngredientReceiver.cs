using UnityEngine;

/// <summary>
/// Pot�� �ڽ� ������Ʈ���� �پ �丶�� ����
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class IngredientReceiver : MonoBehaviour
{
    private Pot parentPot; //�θ� ����
    [SerializeField] private string RequiredId = "Tomato";

    #region Unity Lifecycle
    private void Start()
    {
        parentPot = GetComponentInParent<Pot>();
        if (parentPot == null)
        {
            GameLogger.Instance.LogWarning(this, "�θ� ������Ʈ���� Pot.cs�� ã�� �� ����");
        }
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            GameLogger.Instance.LogWarning(this, $"{gameObject.name}���� IsTrigger�� ����������");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (parentPot == null) return; //�θ� ���� ������ �ߴ�

        if (other.TryGetComponent<Carryable>(out Carryable carryable)) //���°� Carryable�̸�
        {
            if (carryable.carrying) return;

            if (carryable.Id == RequiredId)
            {
                parentPot.AddTomato();
                Destroy(other.gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (parentPot == null) return; //�θ� ���� ������ �ߴ�

        if (other.TryGetComponent<Carryable>(out Carryable carryable)) //���°� Carryable�̸�
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