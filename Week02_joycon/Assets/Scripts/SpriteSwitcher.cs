using UnityEngine;

public class SpriteSwitcher : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject spriteObjectA; //�ʱ⿡ Ȱ��ȭ �Ǿ��ִ� ������Ʈ ����
    [SerializeField] private GameObject spriteObjectB; //A�� ��Ȱ��ȭ�ϰ� Ȱ��ȭ �� ������Ʈ ����

    private bool hasSwithced = false;

    private void Start()
    {
        if (spriteObjectA != null)
        {
            spriteObjectA.SetActive(true);
        }if (spriteObjectB != null)
        {
            spriteObjectB.SetActive(false);
        }
    }

    public void Interact()
    {
        if (!hasSwithced)
        {
            if (spriteObjectA != null) { spriteObjectA.SetActive(false); }
            if (spriteObjectB != null) { spriteObjectB.SetActive(true); }
            hasSwithced=true;
            Debug.Log($"{spriteObjectA.name}�� ��Ȱ��ȭ�ϰ� {spriteObjectB.name}�� Ȱ��ȭ �߽��ϴ�.");
        }
    }

}
