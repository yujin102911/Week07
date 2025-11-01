using UnityEngine;

public class Rag : MonoBehaviour
{
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
        UpdateColor();
    }

    public bool TryCleanDirt()
    {
        if (cleanCurrent == cleanMin) return false;

        cleanCurrent = Mathf.Max(cleanMin, cleanCurrent - cleanDecrase);
        UpdateColor();
        return true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water"))
        {
            if (cleanCurrent < cleanMax) cleanCurrent += cleanSpeed * Time.deltaTime;
        }
        UpdateColor();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Water")) cleanCurrent = (int)cleanCurrent;

        UpdateColor();
    }

    void UpdateColor()
    {
        float t = Mathf.InverseLerp(cleanMin, cleanMax, cleanCurrent);
        Color newColor = Color.Lerp(dirtyColor, cleanColor, t);
        spriteRenderer.color = newColor;
    }
}