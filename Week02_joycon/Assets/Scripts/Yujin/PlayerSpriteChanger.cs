using UnityEngine;

public class PlayerSpriteChanger : MonoBehaviour, IInteractable
{
    [Tooltip("플레이어에게 적용할 새로운 스프라이트")]
    [SerializeField] private Sprite newLookSprite;

    ///<summary>IInteractable요구사항 함수</summary>
    public void Interact()
    {
        Player playerScript = FindObjectOfType<Player>();
        if (playerScript != null)
        {
            ChangePlayerSpriteOnInteract(playerScript);
        }
        else
        {
            GameLogger.Instance.LogError(this, "상호작용 실패: 씬에서 Player스크립트를 찾을 수 없음");
        }
    }


    ///<summary>플레이어가 상호작용 시 이 함수를 호출</summary>
    private void ChangePlayerSpriteOnInteract(Player playerScript)
    {
        if (playerScript != null && newLookSprite != null)
        {
            playerScript.ChangeSprite(newLookSprite);
            gameObject.SetActive(false);
            GameLogger.Instance.LogDebug(this, $"플레이어 스프라이트를 {newLookSprite.name}으로 교체함");
        }
        else
        {
            GameLogger.Instance.LogError(this, "스프라이트 변경 실패: Player스크립트 또는 newLookSprite가 업슴");
        }
    }

}
