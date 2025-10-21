using UnityEngine;

public class Carryable : MonoBehaviour
{
    public bool carrying = false;//�����
    public float large = -1;
    public float weight=1;
    private float lxw;

    [Header("Event - Quest")]
    public string Id;
    public int ScannerID;

    private PlayerCarrying player;
    private LayerMask maskObstacle;

    void Start()
    {if (large < 0)//ũ�� ���� ���� ��������
        {
            large = transform.localScale.x* transform.localScale.y;//ũ�� x*y�� ����
        }
        lxw = large*weight;//ũ��*����=���� ����
        GetComponent<Rigidbody2D>().mass = lxw;//���� ����
        player = GameObject.Find("Player").GetComponent<PlayerCarrying>();//�÷��̾� ã�Ƴֱ�
        maskObstacle = LayerMask.GetMask("Obstacle");
    }
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (carrying)
        {
            if (collision.gameObject.layer== LayerMask.NameToLayer("Obstacle"))
            {
                // �÷��̾ ��� �ִ� ������Ʈ ����Ʈ���� ��� ������Ʈ�� �ε��� Ȯ��
                int myIndex = player.carriedObjects.IndexOf(gameObject); // �ڱ� �ڽ��� ���° ������
                if (myIndex != -1)
                {
                    if (player.collideCarrying > myIndex)
                    {
                        Debug.Log("indexChamge");
                        player.collideCarrying = myIndex;
                    }
                    Debug.Log($"�� {myIndex + 1}�浹");
                }
                else
                {
                    Debug.Log("����"+myIndex);
                }
            }
        }
    }

}
