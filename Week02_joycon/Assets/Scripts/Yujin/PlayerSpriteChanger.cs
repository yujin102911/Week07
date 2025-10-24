using UnityEngine;

public class PlayerSpriteChanger : MonoBehaviour
{
    [Tooltip("�÷��̾�� ������ ���ο� ��������Ʈ")]
    [SerializeField] private Sprite newLookSprite;

    ///<summary>�÷��̾ ��ȣ�ۿ� �� �� �Լ��� ȣ��</summary>
    public void ChangeSprite(Player playerScript)
    {
        if ( playerScript != null )
        {
            playerScript.ChangeSprite(newLookSprite);
            gameObject.SetActive(false); //�ѹ��� �۵��ϵ��� ��
        }
    }

}
