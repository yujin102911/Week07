using UnityEngine;

public class Stove : MonoBehaviour
{
    [Header("Sprite Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite fueledSprite;
    [SerializeField] private Sprite initialSprite;

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
        if (spriteRenderer != null) spriteRenderer.sprite = initialSprite; //���� ���� �� �� ��Ų �������� ����
        if (potSnapPoint == null)
        {
            GameLogger.Instance.LogWarning(this, "potSnapPoint�� �������� �ʾҽ��ϴ�. ���� �������� �ʽ��ϴ�.");
            potSnapPoint = transform; // �ӽ÷� ����� �ڽ��� ��ġ�� ���
        }
        if (GetComponent<Collider2D>() == null || !GetComponent<Collider2D>().isTrigger)
        {
            GameLogger.Instance.LogWarning(this, "Stove�� isTrigger=true �� Collider2D�� �����ϴ�. ���� ������ �� �����ϴ�.");
        }

    }
    #endregion

    #region Public Methods

    ///<summary>WorldInteractable �̺�Ʈ�� ������ �Լ�. ���� �ϳ� �߰�</summary>
    public void AddFirewood()
    {
        if (isFueled)
        {
            GameLogger.Instance.LogDebug(this, "����꿡 �̹� 4���� ������ �ֽ��ϴ�.");
            return;
        }
        currentFirewood++;
        GameLogger.Instance.LogDebug(this, $"���� �߰��� ����: {currentFirewood}��");

        if (isFueled)
        {
            TurnOnFireVisuals();
            if (potOnStove != null)
            {
                potOnStove.CheckCookingConditions();
            }
        }
    }

    ///<summary>����� ���� �ʱ�ȭ �Լ�</summary>
    public void ResetStove()
    {
        if (spriteRenderer == null)
        {
            GameLogger.Instance.LogError(this, "����꿡 spriteRenderer�� ������� �ʾҽ��ϴ�.");
            return;
        }
        currentFirewood = 0;
        spriteRenderer.sprite = initialSprite;
        GameLogger.Instance.LogDebug(this, "����� �ʱ�ȭ �Ϸ�!");
    }

    #endregion

    #region Private Methods
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
