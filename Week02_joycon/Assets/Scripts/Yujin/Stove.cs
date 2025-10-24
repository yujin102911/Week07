using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;

public class Stove : MonoBehaviour
{
    [Header("Color Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color fueledColor = Color.red; //연료 다 찼을 때 색깔
    [SerializeField] private Color initialColor = Color.white; //연료 없을 때 색깔

    [Header("Fuel Settings")]
    [SerializeField] private int requiredFirewood = 4;
    private int currentFirewood = 0;

    [Header("Cooking")]
    private Pot potOnStove = null;

    #region Properties
    public bool isFueled => currentFirewood >= requiredFirewood; //현 장작의 개수가 요구 장작 개수를 넘어섰으면 true 반환
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = initialColor; //게임 시작 시 불 안킨 색상으로 설정

        if (GetComponent<Collider2D>() == null || !GetComponent<Collider2D>().isTrigger)
        {
            GameLogger.Instance.LogWarning(this, "Stove에 isTrigger=true 인 Collider2D가 없습니다. 냄비를 감지할 수 없습니다.");
        }

    }
    #endregion

    #region Public Methods

    ///<summary>WorldInteractable 이벤트에 연결할 함수. 장작 하나 추가</summary>
    public void AddFirewood()
    {
        if (isFueled)
        {
            GameLogger.Instance.LogDebug(this, "스토브에 이미 4개의 장작이 있습니다.");
            return;
        }
        currentFirewood++;
        GameLogger.Instance.LogDebug(this, $"장작 추가됨 현재: {currentFirewood}개");

        if (isFueled)
        {
            TurnOnFireVisuals();
            if (potOnStove != null)
            {
                potOnStove.CheckCookingConditions();
            }
        }
    }

    ///<summary>스토브 상태 초기화 함수</summary>
    public void ResetStove()
    {
        if (spriteRenderer == null)
        {
            GameLogger.Instance.LogError(this, "스토브에 spriteRenderer가 연결되지 않았습니다.");
            return;
        }
        currentFirewood = 0;
        spriteRenderer.color = initialColor;
        GameLogger.Instance.LogDebug(this, "스토브 초기화 완료!");
    }

    #endregion

    #region Private Methods
    ///<summary>스토브에 불이 켜지는 효과</summary>
    private void TurnOnFireVisuals()
    {
        if (spriteRenderer == null)
        {
            GameLogger.Instance.LogError(this, "스토브에 spriteRenderer가 연결되지 않았습니다.");
            return;
        }
        spriteRenderer.color = fueledColor;
        GameLogger.Instance.LogDebug(this, "스토브에 불을 붙였습니다");
    }
    ///<summary>냄비 감지 로직</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 냄비가 내려놓아졌을 때만 감지 (들고 지나가는 것 방지)
        if (other.TryGetComponent<Pot>(out Pot pot))
        {
            if (pot.GetComponent<Carryable>() != null && !pot.GetComponent<Carryable>().carrying)
            {
                potOnStove = pot;
                pot.SetCurrentStove(this); // 냄비에게 자신이 어떤 스토브 위에 있는지 알려줌
                GameLogger.Instance.LogDebug(this, "냄비가 스토브 위에 놓였습니다.");
                pot.CheckCookingConditions(); // 냄비를 올려놓는 순간에도 요리 조건 확인
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Pot>(out Pot pot) && pot == potOnStove)
        {
            potOnStove = null;
            pot.SetCurrentStove(null); // 냄비가 스토브에서 벗어남
            GameLogger.Instance.LogDebug(this, "냄비가 스토브에서 벗어났습니다.");
        }
    }
    #endregion

}
