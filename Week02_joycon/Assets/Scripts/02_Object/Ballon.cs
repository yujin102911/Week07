using UnityEngine;

public class Ballon : MonoBehaviour
{
    private Vector2 firstScale;
    [SerializeField] GameObject ballon;
    [SerializeField] Color[] colors;
    [SerializeField] SpriteRenderer sr;
    public float pumping;
    public float maxScale = 100f; // 최대 크기
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        firstScale = transform.localScale;
        sr.color = colors[Random.Range(0, colors.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        if (pumping > maxScale)
        {
            var newBallon =Instantiate(ballon, transform.position, Quaternion.identity);//내자리에 풍선 생성
            newBallon.GetComponentInChildren<SpriteRenderer>().color = sr.color;//색상 복사
            pumping = 0f;//펌핑 초기화
            sr.color = colors[Random.Range(0, colors.Length)];//새 색상 지정
        }
        transform.localScale = firstScale * pumping / maxScale;
    }
}
