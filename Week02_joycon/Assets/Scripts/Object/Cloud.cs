using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] float minMoveSpeed = 2f; // 초당 이동 속도
    [SerializeField] float maxMoveSpeed = 7f; // 초당 이동 속도
    float moveSpeed = 0;
    [SerializeField] float lifetime = 10f; // n초 후 삭제 (수명)

    void Start()
    {

        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        // n초 (lifetime) 후에 이 게임 오브젝트를 파괴하도록 예약합니다.
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // 매 프레임 왼쪽으로 (Vector3.left) moveSpeed 만큼 이동합니다.
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }
}