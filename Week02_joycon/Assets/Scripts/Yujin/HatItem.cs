using UnityEngine;

/// <summary>
/// 모자 아이템 프리팹에
/// </summary>
public class HatItem : MonoBehaviour, IInteractable
{
    [Header("Hat Data")]
    [SerializeField] private Sprite hatSpriteToWear; //플레이어가 착용할 스프라이트
    [SerializeField] private GameObject hatItemPrefab;

    public bool TryInteract()
    {
        Player playerScript = Player.Instance;
        if (playerScript == null) return false;
        playerScript.ChangeHat(this);

        Destroy(gameObject);
        return true;
    }

    public Sprite GetHatSprite()
    {
        return hatSpriteToWear;
    }

    public GameObject GetHatPrefab()
    {
        if (hatItemPrefab == null)
        {
            GameLogger.Instance.LogError(this, "hatItemPrefab이 없음");
            return null;
        }
        if (hatItemPrefab.scene.IsValid())
        {
            // 만약 true가 나온다면, 씬에 있는 오브젝트가 잘못 할당되었다는 뜻입니다.
            GameLogger.Instance.LogError(this,
                $"[치명적 오류] {this.name}의 hatItemPrefab 필드가 씬에 있는 오브젝트({hatItemPrefab.name})를 참조하고 있습니다. " +
                "프로젝트 폴더의 프리팹 에셋을 참조해야 합니다!");

            // 잘못된 참조이므로 null을 반환해서 "Missing" 오류라도 막습니다.
            return null;
        }
        return hatItemPrefab;
    }

}
