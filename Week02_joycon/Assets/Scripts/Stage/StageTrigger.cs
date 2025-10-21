using UnityEngine;

public class StageTrigger : MonoBehaviour
{
    public bool isEnd;
    [SerializeField] private StageSaveable stageSaveable;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (isEnd)
        {
            stageSaveable.SaveHierarchyWithCurrentStage();
        }
        else
        {
            // ���� �ڽ� ���� ���� + ����� ��������� �籸��
            stageSaveable.RestoreHierarchyWithCurrentStage();
        }
    }
}
