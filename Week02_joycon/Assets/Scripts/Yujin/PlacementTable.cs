using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PlacementTable : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("�� ���̺��� �䱸�ϴ� �������� ID")]
    [SerializeField] private string requiredItemId;

    [Tooltip("�������� ������ ��ġ")]
    [SerializeField] private Transform snapPoint;

    [Header("State")]
    [SerializeField] private bool itemPlaced = false; // �������� �̹� �������� Ȯ��

    [Header("Events")]
    [Tooltip("�������� ���������� �÷������� �� ������ �̺�Ʈ")]
    public UnityEvent OnItemPlaced;

    #region Lifecycle
    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger) { GameLogger.Instance.LogWarning(this, "IsTriggerȰ��ȭ �ȵ���"); }
        if (snapPoint == null) { GameLogger.Instance.LogWarning(this, "IsTriggerȰ��ȭ �ȵż� �� ���̺��� ��."); snapPoint = transform; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (itemPlaced) return; //�̹� �������� ���������� �ƹ��͵� ����
        if (other.TryGetComponent<Carryable>(out Carryable carryable))
        {
            if (!carryable.carrying && carryable.Id == requiredItemId) PlaceItem(carryable);
        }
    }
    #endregion

    #region Private Methods
    ///<summary>�������� ���̺��� ����</summary>
    private void PlaceItem(Carryable item)
    {
        itemPlaced = true;
        GameLogger.Instance.LogDebug(this, $"{item.name}, ���̺��� ����");

        GameObject itemObject = item.gameObject;

        itemObject.transform.position = snapPoint.position;
        itemObject.transform.rotation = Quaternion.identity;

        if (itemObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        item.enabled = false;

        Collider2D itemCollider = itemObject.GetComponent<Collider2D>();
        if (itemCollider != null && !itemCollider.isTrigger)
        {
            itemCollider.enabled = false;
        }

        OnItemPlaced?.Invoke();

        QuestRuntime.Instance.SetFlag(FlagId.Table_Used);
        GameLogger.Instance.LogDebug(this, "음식 퀘스트 완료");
    }
    #endregion
}