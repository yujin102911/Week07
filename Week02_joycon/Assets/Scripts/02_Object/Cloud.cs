using UnityEngine;

public class Cloud : MonoBehaviour
{
    [SerializeField] float minMoveSpeed = 2f;
    [SerializeField] float maxMoveSpeed = 7f;
    float moveSpeed = 0;
    [SerializeField] float lifetime = 10f;

    void Start()
    {
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
    }
}