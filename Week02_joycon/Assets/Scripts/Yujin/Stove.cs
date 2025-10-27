using UnityEngine;

public class Stove : MonoBehaviour
{
    [Header("Sprite Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [SerializeField] private Sprite[] firewoodSprites; //(0,1,2,3)일 때 스프라이트
    [SerializeField] private Sprite fueledSprite; //불 붙었을 때 스프라이트

    [Header("Fuel Settings")]
    [SerializeField] private int requiredFirewood = 4;
    private int currentFirewood = 0;

    [Header("Cooking")]
    private Pot potOnStove = null;
    [Tooltip("Pot Position")]
    [SerializeField] private Transform potSnapPoint;

    #region Properties
    public bool isFueled => currentFirewood >= requiredFirewood; //�� ������ ������ �䱸 ���� ������ �Ѿ���� true ��ȯ
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // firewoodSprites 배열이 requiredFirewood 개수(4개)만큼 할당되었는지 확인
        if (firewoodSprites == null || firewoodSprites.Length != requiredFirewood)
        {
            GameLogger.Instance.LogError(this, $"firewoodSprites 배열이 비어있거나, 크기가 requiredFirewood({requiredFirewood})와 일치하지 않습니다. 인스펙터에서 0개부터 {requiredFirewood - 1}개까지의 스프라이트를 할당해주세요.");
        }

        if (potSnapPoint == null)
        {
            GameLogger.Instance.LogWarning(this, "potSnapPoint이 설정되지 않았습니다. 기본 위치로 설정합니다.");
            potSnapPoint = transform; // 임시로 자신의 위치로 설정
        }
        if (GetComponent<Collider2D>() == null || !GetComponent<Collider2D>().isTrigger)
        {
            GameLogger.Instance.LogWarning(this, "Stove에 isTrigger=true 인 Collider2D가 필요합니다. 상호작용이 안 될 수 있습니다.");
        }

        UpdateSprite();

    }
    #endregion

    #region Public Methods

    ///<summary>WorldInteractable 이벤트에 연결될 함수. 장작 하나 추가</summary>
    public void AddFirewood()
    {
        if (isFueled)
        {
            GameLogger.Instance.LogDebug(this, "아궁이에 이미 {requiredFirewood}개의 장작이 있습니다.");
            return;
        }
        currentFirewood++;
        GameLogger.Instance.LogDebug(this, $"장작 추가, 현재: {currentFirewood}개");
        UpdateSprite(); // 장작이 추가될 때마다 스프라이트 업데이트
        if (isFueled)
        {
            //TurnOnFireVisuals();
            if (potOnStove != null)
            {
                potOnStove.CheckCookingConditions();
            }
        }
    }

    ///<summary>아궁이 상태 초기화 함수</summary>
    public void ResetStove()
    {
        if (spriteRenderer == null)
        {
            GameLogger.Instance.LogError(this, "아궁이에 spriteRenderer가 할당되지 않았습니다.");
            return;
        }
        currentFirewood = 0;
        UpdateSprite(); // 스프라이트를 0개 상태(firewoodSprites[0])로 리셋
        GameLogger.Instance.LogDebug(this, "아궁이 초기화 완료!");
    }

    #endregion

    #region Private Methods

    /// <summary>현재 장작 개수와 상태에 맞춰 스프라이트를 업데이트합니다.</summary>
    private void UpdateSprite()
    {
        if (spriteRenderer == null)
        {
            GameLogger.Instance.LogError(this, "UpdateSprite 호출 시 spriteRenderer가 없습니다.");
            return;
        }

        if (isFueled)
        {
            // 연료가 다 차면 불 켜진 스프라이트
            spriteRenderer.sprite = fueledSprite;
        }
        else
        {
            // 연료가 덜 찼으면 현재 장작 개수에 맞는 스프라이트
            if (firewoodSprites != null && currentFirewood < firewoodSprites.Length)
            {
                spriteRenderer.sprite = firewoodSprites[currentFirewood];
            }
            else
            {
                // 배열이 잘못 설정된 경우 경고
                GameLogger.Instance.LogWarning(this, $"현재 장작 개수({currentFirewood})에 해당하는 스프라이트가 firewoodSprites 배열에 없습니다.");
            }
        }
    }

    ///<summary>����꿡 ���� ������ ȿ��</summary>
    private void TurnOnFireVisuals()
    {
        if (spriteRenderer == null)
        {
            GameLogger.Instance.LogError(this, "����꿡 spriteRenderer�� ������� �ʾҽ��ϴ�.");
            return;
        }
        spriteRenderer.sprite = fueledSprite;
        GameLogger.Instance.LogDebug(this, "����꿡 ���� �ٿ����ϴ�");
    }
    ///<summary>����� ���� ���� ����</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<Pot>(out Pot pot))
        {
            if (pot.GetComponent<Carryable>() != null && !pot.GetComponent<Carryable>().carrying)
            {
                potOnStove = pot;
                pot.SetCurrentStove(this);
                GameLogger.Instance.LogDebug(this, "���� ����� ���� �������ϴ�.");

                pot.transform.position = potSnapPoint.position;
                pot.transform.rotation = Quaternion.identity;
                if (pot.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                pot.CheckCookingConditions();
            }
        }
        else if (other.TryGetComponent(out Carryable carryable))
        {
            if (carryable.Id == "Firewood" && !carryable.carrying)
            {
                AddFirewood();
                Destroy(other.gameObject);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Pot>(out Pot pot))
        {
            if (pot.GetComponent<Carryable>() != null && !pot.GetComponent<Carryable>().carrying)
            {
                potOnStove = pot;
                pot.SetCurrentStove(this);
                GameLogger.Instance.LogDebug(this, "���� ����� ���� �������ϴ�.");

                pot.transform.position = potSnapPoint.position;
                pot.transform.rotation = Quaternion.identity;
                if (pot.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                pot.CheckCookingConditions();
            }
        }
        else if (collision.TryGetComponent(out Carryable carryable))
        {
            if (carryable.Id == "Firewood" && !carryable.carrying)
            {
                AddFirewood();
                Destroy(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Pot>(out Pot pot) && pot == potOnStove)
        {
            potOnStove = null;
            pot.SetCurrentStove(null); // ���� ����꿡�� ���
            GameLogger.Instance.LogDebug(this, "���� ����꿡�� ������ϴ�.");
        }
    }
    #endregion

}
