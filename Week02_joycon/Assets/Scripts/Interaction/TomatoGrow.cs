using UnityEngine;

public class TomatoGrow : MonoBehaviour
{
    [SerializeField] GameObject tomatoGrowUP;
    [SerializeField] SpriteRenderer spriteRender;
    public float growCurrent = 0;
    Vector2 firstScale;

    void Start()
    {
        firstScale = transform.localScale;
        ColorUpdate();
        SizeUpdate();
    }

    void Update()
    {
        //isWatered = false; // 매 프레임 초기화
        if (growCurrent >= 255)
        {
            tomatoGrowUP.SetActive(true);//토마토 온
            gameObject.SetActive(false);//토마토 성장 끔
        }
        ColorUpdate();
        SizeUpdate();
    }

    void ColorUpdate()
    {
        spriteRender.color = new Color(growCurrent / 255f, 1f, growCurrent / 255f, 1f);
    }

    void SizeUpdate()
    {
        transform.localScale = firstScale * (1f + growCurrent / 255f);
    }
}
