using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class PlacementTable : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("이 테이블이 요구하는 아이템의 ID")]
    [SerializeField] private string requiredItemId;

    [Tooltip("아이템이 고정될 위치")]
    [SerializeField] private Transform snapPoint;

    [Header("State")]
    [SerializeField] private bool itemPlaced = false; // 아이템이 이미 놓였는지 확인

    [Header("Events")]
    [Tooltip("아이템을 성공적으로 올려놓았을 때 실행할 이벤트")]
    public UnityEvent OnItemPlaced;

    #region Lifecycle
    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger) { GameLogger.Instance.LogWarning(this, "IsTrigger활성화 안됐음"); }
        if (snapPoint == null) { GameLogger.Instance.LogWarning(this, "IsTrigger활성화 안돼서 걍 테이블로 함."); snapPoint = transform; }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (itemPlaced) return; //이미 아이템이 놓여있으면 아무것도 안함
        if (other.TryGetComponent<Carryable>(out Carryable carryable))
        {
            if (!carryable.carrying && carryable.Id == requiredItemId) PlaceItem(carryable);
        }
    }
    #endregion

    #region Private Methods
    ///<summary>아이템을 테이블에 고정</summary>
    private void PlaceItem(Carryable item)
    {
        itemPlaced = true;
        GameLogger.Instance.LogDebug(this, $"{item.name}, 테이블에 고정");

        GameObject itemObject = item.gameObject;

        itemObject.transform.position = snapPoint.position;
        itemObject.transform.rotation = Quaternion.identity;

        if(itemObject.TryGetComponent<Rigidbody2D>(out var rb))
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
    }
    #endregion

}
