using UnityEngine;
using UnityEngine.Events;

public class WorldInteractable : MonoBehaviour
{
    #region Public Fields
    [Tooltip("Required Item Id")]
    public string requiredItemId;
    [Tooltip("if Success, will item consume")]
    public bool consumeItemOnSuccess = false; //아이템 사용 후 없어질건지 아닌지
    [Tooltip("SeccessEvent")]
    public UnityEvent OnInteractionSuccess;
    ///[Tooltip("실패 시 실행할 이벤트")]
    ///public UnityEvent OnInteractionFail;

    #endregion

    #region Public Methods
    ///<summary>플레이어가 상호작용을 시도할 때 호출</summary>
    ///<param name="heldItemId">플레이어 0번 슬롯의 아이템 ID</param>
    public bool AttemptInteraction(string heldItemId, PlayerCarrying player)
    {
        if (string.IsNullOrEmpty(requiredItemId)) //만약 필요한 아이템이 없다면
        {
            GameLogger.Instance.LogDebug(this, $"{gameObject.name}와 맨손 상호작용 성공");
            OnInteractionSuccess?.Invoke();
            return true;
        }

        if (!string.IsNullOrEmpty(heldItemId) && requiredItemId == heldItemId) //만약 필요한 아이템이 있고 그게 들고 있는 아이템과 일치하다면
        {
            GameLogger.Instance.LogDebug(this, $"{gameObject.name}와 {heldItemId}의 상호작용 성공");
            OnInteractionSuccess?.Invoke();
            if (consumeItemOnSuccess) //만약 사용 후 아이템을 소모해야 한다면
            {
                player.ConsumeItem(0); //0번 슬롯 아이템 소모
            }

            return true;
        }
        else //일치하지 않는다면
        {
            GameLogger.Instance.LogDebug(this, $"{gameObject.name}과 상호작용 가능한 {requiredItemId}가 없습니다.");
            //OnIntercationFail?.Invoke();
            return false;
        }
    }
    #endregion
}
