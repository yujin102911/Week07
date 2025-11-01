using UnityEngine;

public class PlantGrow : MonoBehaviour
{
    [SerializeField] GameObject GrowUP;
    [SerializeField] public SpriteRenderer spriteRender;
    [SerializeField] float growmin = 1f;
    [SerializeField] float growmax = 2f;
    [SerializeField] float growcool = 10f;
    public float growCurrent = 0;
    Vector2 firstScale;

    void Start()
    {
        if (spriteRender == null)
        {
            spriteRender = GetComponent<SpriteRenderer>();
        }
        firstScale = transform.localScale;
        ColorUpdate();
        SizeUpdate();
    }

    void Update()
    {
        //isWatered = false; // 매 프레임 초기화
        if (growCurrent >= 255)
        {
            Instantiate(GrowUP,transform.position,Quaternion.identity);//내자리에 수확물 생성
            spriteRender.enabled=false;//성장 끔
            growCurrent = -growcool;// 쿨타임 시작
        }
        else
        {
            growCurrent += Random.Range(growmin, growmax) * Time.deltaTime;
            if (!spriteRender.enabled && growCurrent>0) spriteRender.enabled = true;//성장 켬
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
