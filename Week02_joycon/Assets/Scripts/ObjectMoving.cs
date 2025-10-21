using UnityEngine;

public class ObjectMoving : MonoBehaviour
{
    public Transform target;      // ����ų ��ǥ ��ġ (Inspector�� �巡��)
    public float speed = 2f;      // �̵� �ӵ� (units/sec)
    public float stopDistance = 0.1f;

    void Update()
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        if (dir.magnitude <= stopDistance) return; // �����ϸ� ����

        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }
}
