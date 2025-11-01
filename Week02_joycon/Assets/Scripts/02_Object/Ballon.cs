using UnityEngine;

public class Ballon : MonoBehaviour
{
    private Vector2 firstScale;
    [SerializeField] GameObject ballon;
    public float pumping;
    public float maxScale = 100f; // 최대 크기
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        firstScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (pumping > maxScale)
        {
            Instantiate(ballon, transform.position, Quaternion.identity);//내자리에 풍선 생성
            pumping = 0f;//펌핑 초기화
        }
        transform.localScale = firstScale * pumping / maxScale;
    }
}
