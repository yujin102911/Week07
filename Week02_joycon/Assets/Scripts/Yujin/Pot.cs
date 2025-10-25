using UnityEngine;

// ����� Carryable, WorldInteractable ������Ʈ�� ��� �ʿ��մϴ�.
[RequireComponent(typeof(Carryable), typeof(Collider2D))]
public class Pot : MonoBehaviour
{
    [Header("Ingredients")]
    [SerializeField] private int requiredTomatoes = 2;
    private int currentTomatoes = 0;

    [Header("State")]
    private Stove currentStove = null; // ���� �ö� �ִ� �����
    private bool isCooked = false;

    [Header("Cooking")]
    [SerializeField] private GameObject cookedMealPrefab; // �丮 �Ϸ� �� ������ �ϼ��� �丮 ������
    [SerializeField] private Transform spawnPoint; // �丮�� ������ ��ġ (���� ���ϸ� ���� ��ġ)

    private Carryable selfCarryable;
    private IngredientReceiver ingredientReceiver;
    private Collider2D ingredientCollider; // �ڽ��� �ݶ��̴��� ���� ����

    private void Start()
    {
        selfCarryable = GetComponent<Carryable>();
        ingredientReceiver = GetComponentInChildren<IngredientReceiver>(true);
        if (selfCarryable == null)
        {
            GameLogger.Instance.LogError(this, "Pot�� Carryable ������Ʈ�� �����ϴ�!");
        }
        if (ingredientReceiver == null)
        {
            GameLogger.Instance.LogError(this, "�ڽ� ������Ʈ���� IngredientReceiver.cs�� ã�� �� �����ϴ�!");
        }
        else
        {
            // (�߰�) �ڽ� ������Ʈ�� Collider2D ������Ʈ�� ã�Ƽ� �����մϴ�.
            ingredientCollider = ingredientReceiver.GetComponent<Collider2D>();
            if (ingredientCollider == null)
            {
                GameLogger.Instance.LogError(this, "IngredientReceiver ������Ʈ���� Collider2D�� ã�� �� �����ϴ�!");
            }
        }

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col.isTrigger) { GameLogger.Instance.LogWarning(this, "Pot�� �θ𿡴� IsTrigger�� ���������� �ȵǴµ� ��������"); }
        if (GetComponent<Rigidbody2D>() == null)
        {
            GameLogger.Instance.LogWarning(this, "Pot�θ� ������Ʈ�� Rigidbody�� ����");
            gameObject.AddComponent<Rigidbody2D>();
        }
    }
    private void Update()
    {
        if (selfCarryable != null && ingredientCollider != null)
        {
            // (����) ���� ���������(carrying == true) IsTrigger�� ���ϴ�(false).
            // ���� �ٴڿ� ������(carrying == false) IsTrigger�� �ٽ� �մϴ�(true).
            ingredientCollider.isTrigger = !selfCarryable.carrying;
        }
    }

    /// <summary>
    /// IngredientReceiver�� ���� ȣ��� (�丶�� �߰�)
    /// </summary>
    public void AddTomato()
    {
        if (isCooked) return; // �̹� �丮��

        if (currentTomatoes < requiredTomatoes)
        {
            currentTomatoes++;
            GameLogger.Instance.LogDebug(this, $"���� �丶�� �߰���. ����: {currentTomatoes}��");
            // (���û���) ���⿡ ���� ��������Ʈ�� ���־��� �����ϴ� �ڵ� �߰�

            CheckCookingConditions(); // �丶�䰡 �߰��� ������ �丮 ���� Ȯ��
        }
        else
        {
            GameLogger.Instance.LogDebug(this, "���� �丶�䰡 ���� á���ϴ�.");
        }
    }

    /// <summary>
    /// ����갡 �ڽ��� �� ������ ���� ������ ����/������ �� ȣ��
    /// </summary>
    public void SetCurrentStove(Stove stove)
    {
        currentStove = stove;
    }

    /// <summary>
    /// �丮 ����(��û 3��)�� Ȯ���ϴ� �ٽ� �Լ�
    /// 1. ���� ����� ���� �ִ°�?
    /// 2. �� ����꿡 ���� �پ��°�? (���� 4��)
    /// 3. ���� �丶�䰡 2�� �ִ°�?
    /// </summary>
    public void CheckCookingConditions()
    {
        if (isCooked) return; // �̹� �丮��

        // 1. ����� ���� �ִ°�?
        if (currentStove == null)
        {
            //GameLogger.Instance.LogDebug(this, "�丮 ����: ����� ���� ����");
            return;
        }

        // 2. ����꿡 ���� �پ��°�?
        if (!currentStove.isFueled)
        {
            GameLogger.Instance.LogDebug(this, "�丮 ����: ����꿡 ���� ����");
            return;
        }

        // 3. �丶�䰡 2�� �ִ°�?
        if (currentTomatoes < requiredTomatoes)
        {
            GameLogger.Instance.LogDebug(this, $"�丮 ����: �丶�� ���� ({currentTomatoes}/{requiredTomatoes})");
            return;
        }

        // ��� ���� ����!
        Cook();
    }

    private void Cook()
    {
        isCooked = true;
        GameLogger.Instance.LogDebug(this, "�丮 ����!");

        // 4. �丮�� ������ ����
        if (cookedMealPrefab != null)
        {
            Instantiate(cookedMealPrefab, spawnPoint.position, Quaternion.identity);
        }

        // 5. ����� ���� �ʱ�ȭ
        currentStove.ResetStove();

        // 6. ���� ���� �ʱ�ȭ
        ResetPot();
    }

    /// <summary>
    /// ���� ���� �ʱ�ȭ (��û 4��)
    /// </summary>
    public void ResetPot()
    {
        currentTomatoes = 0;
        isCooked = false;
        // (���û���) ���� ���־� �ʱ�ȭ
        GameLogger.Instance.LogDebug(this, "���� �ʱ�ȭ �Ϸ�.");
    }
}