using UnityEngine;

public class Rag : MonoBehaviour
{
    [SerializeField] GameObject dirty;
    [SerializeField] GameObject water;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] int cleanMax = 200;//청결도 최대치
    [SerializeField] int cleanMin = 20;//청결도 최소치
    [SerializeField] int cleanDecrase = 20;//청결도 감소량
    [SerializeField] float cleanCurrent = 200;//청결도 현재치
    [SerializeField] int cleanSpeed = 200;//청결도 회복 속도

    [Header("색상 설정")]
    [SerializeField] Color cleanColor = Color.white; // 가장 깨끗할 때의 색
    [SerializeField] Color dirtyColor = new Color(0.3f, 0.2f, 0.1f); // 가장 더러울 때의 색 (갈색 계열)


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ColorUpdate();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Dirty"))
        {
            if (cleanCurrent > cleanMin)
            {
                Debug.Log("더러워짐");
                Destroy(collision.gameObject);
                cleanCurrent -= cleanDecrase;
                if (cleanCurrent < cleanMin)
                {
                    cleanCurrent = cleanMin;
                }
            }
        }
        ColorUpdate();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            if (cleanCurrent < cleanMax)
            {
                cleanCurrent += cleanSpeed * Time.deltaTime;
                Debug.Log("깨끗해짐" + cleanCurrent);
            }

        }
        ColorUpdate();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            cleanCurrent = (int)cleanCurrent;
        }
        ColorUpdate();
    }
    void ColorUpdate()
    {
        // 1. 현재 청결도(cleanMin ~ cleanMax)를 0.0 ~ 1.0 사이의 비율로 변환합니다.
        //    - cleanCurrent가 cleanMin이면 0.0
        //    - cleanCurrent가 cleanMax이면 1.0
        float t = Mathf.InverseLerp(cleanMin, cleanMax, cleanCurrent);

        // 2. 변환된 비율(t)을 사용하여 dirtyColor에서 cleanColor로 보간합니다.
        //    - t가 0이면 dirtyColor
        //    - t가 1이면 cleanColor
        //    - t가 0.5이면 두 색상의 정확한 중간색
        Color newColor = Color.Lerp(dirtyColor, cleanColor, t);

        // 3. 계산된 색상을 스프라이트에 적용합니다.
        spriteRenderer.color = newColor;
    }
}
