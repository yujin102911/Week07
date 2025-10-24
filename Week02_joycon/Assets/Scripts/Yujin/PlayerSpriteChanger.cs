using UnityEngine;

public class PlayerSpriteChanger : MonoBehaviour
{
    [Tooltip("플레이어에게 적용할 새로운 스프라이트")]
    [SerializeField] private Sprite newLookSprite;

    ///<summary>플레이어가 상호작용 시 이 함수를 호출</summary>
    public void ChangeSprite(Player playerScript)
    {
        if ( playerScript != null )
        {
            playerScript.ChangeSprite(newLookSprite);
            gameObject.SetActive(false); //한번만 작동하도록 함
        }
    }

}
