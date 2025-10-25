using UnityEngine;

// 냄비는 Carryable, WorldInteractable 컴포넌트가 모두 필요합니다.
[RequireComponent(typeof(Carryable), typeof(Collider2D))]
public class Pot : MonoBehaviour
{
    [Header("Ingredients")]
    [SerializeField] private int requiredTomatoes = 2;
    private int currentTomatoes = 0;

    [Header("State")]
    private Stove currentStove = null; // 내가 올라가 있는 스토브
    private bool isCooked = false;

    [Header("Cooking")]
    [SerializeField] private GameObject cookedMealPrefab; // 요리 완료 시 생성할 완성된 요리 프리팹
    [SerializeField] private Transform spawnPoint; // 요리가 생성될 위치 (설정 안하면 냄비 위치)

    private Carryable selfCarryable;
    private IngredientReceiver ingredientReceiver;
    private Collider2D ingredientCollider; // 자식의 콜라이더를 직접 참조

    private void Start()
    {
        selfCarryable = GetComponent<Carryable>();
        ingredientReceiver = GetComponentInChildren<IngredientReceiver>(true);
        if (selfCarryable == null)
        {
            GameLogger.Instance.LogError(this, "Pot에 Carryable 컴포넌트가 없습니다!");
        }
        if (ingredientReceiver == null)
        {
            GameLogger.Instance.LogError(this, "자식 오브젝트에서 IngredientReceiver.cs를 찾을 수 없습니다!");
        }
        else
        {
            // (추가) 자식 오브젝트의 Collider2D 컴포넌트를 찾아서 저장합니다.
            ingredientCollider = ingredientReceiver.GetComponent<Collider2D>();
            if (ingredientCollider == null)
            {
                GameLogger.Instance.LogError(this, "IngredientReceiver 오브젝트에서 Collider2D를 찾을 수 없습니다!");
            }
        }

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col.isTrigger) { GameLogger.Instance.LogWarning(this, "Pot의 부모에는 IsTrigger가 켜져있으면 안되는데 켜져있음"); }
        if (GetComponent<Rigidbody2D>() == null)
        {
            GameLogger.Instance.LogWarning(this, "Pot부모 오브젝트에 Rigidbody가 없음");
            gameObject.AddComponent<Rigidbody2D>();
        }
    }
    private void Update()
    {
        if (selfCarryable != null && ingredientCollider != null)
        {
            // (수정) 냄비가 들려있으면(carrying == true) IsTrigger를 끕니다(false).
            // 냄비가 바닥에 있으면(carrying == false) IsTrigger를 다시 켭니다(true).
            ingredientCollider.isTrigger = !selfCarryable.carrying;
        }
    }

    /// <summary>
    /// IngredientReceiver에 의해 호출됨 (토마토 추가)
    /// </summary>
    public void AddTomato()
    {
        if (isCooked) return; // 이미 요리됨

        if (currentTomatoes < requiredTomatoes)
        {
            currentTomatoes++;
            GameLogger.Instance.LogDebug(this, $"냄비에 토마토 추가됨. 현재: {currentTomatoes}개");
            // (선택사항) 여기에 냄비 스프라이트나 비주얼을 변경하는 코드 추가

            CheckCookingConditions(); // 토마토가 추가될 때마다 요리 조건 확인
        }
        else
        {
            GameLogger.Instance.LogDebug(this, "냄비에 토마토가 가득 찼습니다.");
        }
    }

    /// <summary>
    /// 스토브가 자신을 이 냄비의 현재 스토브로 설정/해제할 때 호출
    /// </summary>
    public void SetCurrentStove(Stove stove)
    {
        currentStove = stove;
    }

    /// <summary>
    /// 요리 조건(요청 3번)을 확인하는 핵심 함수
    /// 1. 냄비가 스토브 위에 있는가?
    /// 2. 그 스토브에 불이 붙었는가? (장작 4개)
    /// 3. 냄비에 토마토가 2개 있는가?
    /// </summary>
    public void CheckCookingConditions()
    {
        if (isCooked) return; // 이미 요리됨

        // 1. 스토브 위에 있는가?
        if (currentStove == null)
        {
            //GameLogger.Instance.LogDebug(this, "요리 실패: 스토브 위에 없음");
            return;
        }

        // 2. 스토브에 불이 붙었는가?
        if (!currentStove.isFueled)
        {
            GameLogger.Instance.LogDebug(this, "요리 실패: 스토브에 불이 없음");
            return;
        }

        // 3. 토마토가 2개 있는가?
        if (currentTomatoes < requiredTomatoes)
        {
            GameLogger.Instance.LogDebug(this, $"요리 실패: 토마토 부족 ({currentTomatoes}/{requiredTomatoes})");
            return;
        }

        // 모든 조건 충족!
        Cook();
    }

    private void Cook()
    {
        isCooked = true;
        GameLogger.Instance.LogDebug(this, "요리 성공!");

        // 4. 요리된 아이템 생성
        if (cookedMealPrefab != null)
        {
            Instantiate(cookedMealPrefab, spawnPoint.position, Quaternion.identity);
        }

        // 5. 스토브 상태 초기화
        currentStove.ResetStove();

        // 6. 냄비 상태 초기화
        ResetPot();
    }

    /// <summary>
    /// 냄비 상태 초기화 (요청 4번)
    /// </summary>
    public void ResetPot()
    {
        currentTomatoes = 0;
        isCooked = false;
        // (선택사항) 냄비 비주얼 초기화
        GameLogger.Instance.LogDebug(this, "냄비 초기화 완료.");
    }
}