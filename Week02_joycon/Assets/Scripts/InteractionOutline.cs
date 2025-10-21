using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InteractionOutline : MonoBehaviour
{
    [Header("�ڽ� ������Ʈ (�ۺ����� �Ҵ�)")]
    public GameObject Outline; // ��: �ƿ�����
    public GameObject Effect; // ��: ������

    [Tooltip("�÷��̾� �±� �̸�")]
    public string playerTag = "Player";

    void Reset()
    {
        // Ʈ���ŷ� �ڵ� ����
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Start()
    {
        // ���� �� ���α�
        SetChildrenActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            SetChildrenActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            SetChildrenActive(false);
        }
    }

    private void SetChildrenActive(bool active)
    {
        if (Outline != null) Outline.SetActive(active);
        if (Effect != null) Effect.SetActive(active);
    }
}
