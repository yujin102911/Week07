using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] float minMoveSpeed = 2f; // �ʴ� �̵� �ӵ�
    [SerializeField] float maxMoveSpeed = 7f; // �ʴ� �̵� �ӵ�
    float moveSpeed = 0;
    [SerializeField] float lifetime = 10f; // n�� �� ���� (����)

    void Start()
    {

        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        // n�� (lifetime) �Ŀ� �� ���� ������Ʈ�� �ı��ϵ��� �����մϴ�.
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // �� ������ �������� (Vector3.left) moveSpeed ��ŭ �̵��մϴ�.
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }
}