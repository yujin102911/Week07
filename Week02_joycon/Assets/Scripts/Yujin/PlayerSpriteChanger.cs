using UnityEngine;

public class PlayerSpriteChanger : MonoBehaviour, IInteractable
{
    [Tooltip("�÷��̾�� ������ ���ο� ��������Ʈ")]
    [SerializeField] private Sprite newLookSprite;

    ///<summary>IInteractable�䱸���� �Լ�</summary>
    public void Interact()
    {
        Player playerScript = FindObjectOfType<Player>();
        if (playerScript != null)
        {
            ChangePlayerSpriteOnInteract(playerScript);
        }
        else
        {
            GameLogger.Instance.LogError(this, "��ȣ�ۿ� ����: ������ Player��ũ��Ʈ�� ã�� �� ����");
        }
    }


    ///<summary>�÷��̾ ��ȣ�ۿ� �� �� �Լ��� ȣ��</summary>
    private void ChangePlayerSpriteOnInteract(Player playerScript)
    {
        if (playerScript != null && newLookSprite != null)
        {
            playerScript.ChangeSprite(newLookSprite);
            gameObject.SetActive(false);
            GameLogger.Instance.LogDebug(this, $"�÷��̾� ��������Ʈ�� {newLookSprite.name}���� ��ü��");
        }
        else
        {
            GameLogger.Instance.LogError(this, "��������Ʈ ���� ����: Player��ũ��Ʈ �Ǵ� newLookSprite�� ����");
        }
    }

}
