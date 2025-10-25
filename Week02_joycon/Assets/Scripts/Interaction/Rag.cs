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
                Destroy(collision.gameObject);
                cleanCurrent -= cleanDecrase;

                if (cleanCurrent < cleanMin) cleanCurrent = cleanMin;
            }
        }
        ColorUpdate();
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            if (cleanCurrent < cleanMax) cleanCurrent += cleanSpeed * Time.deltaTime;
        }
        ColorUpdate();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water")) cleanCurrent = (int)cleanCurrent;

        ColorUpdate();
    }
    void ColorUpdate()
    {
        float t = Mathf.InverseLerp(cleanMin, cleanMax, cleanCurrent);
        Color newColor = Color.Lerp(dirtyColor, cleanColor, t);
        spriteRenderer.color = newColor;
    }
}