using UnityEngine;

public class Rag : MonoBehaviour
{
    [SerializeField] GameObject dirty;
    [SerializeField] GameObject water;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] int cleanMax=200;//청결도 최대치
    [SerializeField] int cleanMin = 20;//청결도 최소치
    [SerializeField] int cleanDecrase=20;//청결도 감소량
    [SerializeField] float cleanCurrent = 200;//청결도 현재치
    [SerializeField] int cleanSpeed = 200;//청결도 회복 속도
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ColorUpdate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Dirty"))
        {
            if (cleanCurrent>cleanMin)
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
            cleanCurrent=(int)cleanCurrent;
        }
        ColorUpdate();
    }
    void ColorUpdate()
    {
        spriteRenderer.color = new Color(cleanCurrent / 255, cleanCurrent / 255, cleanCurrent / 255, 1f);
    }
}
