using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Playables;

public class InteractablePump : MonoBehaviour
{
    [SerializeField] Controller2D controller2D;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Ballon ballon;//연결된 풍선
    [SerializeField] float returnSpeed=1f;//원상복구 속도
    float startY;
    float pushedY;//눌린 높이
    float pumpingAmount = 1f;//펌핑량
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startY = transform.position.y;
        if (controller2D == null)
            controller2D = Player.Instance.GetComponent<Controller2D>();
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.excludeLayers = (controller2D.isFalling) ? 0 : rb.excludeLayers = 1 << LayerMask.NameToLayer("Player");//플레이어 점프중일 때 제외 레이어 제거
        if (transform.position.y < startY)//펌프 원상복구 코드
        {
            if (startY-transform.position.y > pushedY)
            {
                pushedY = startY - transform.position.y;//최대 눌린 높이 저장
                ballon.pumping += pushedY;//풍선 펌핑
            }
            transform.position += Vector3.up * Time.deltaTime * returnSpeed;
        }
        else//펌프 넘기면
        {
            transform.position = new Vector3 (transform.position.x,startY);//넘어가면 위치 되돌리기
            pushedY=0f;//눌린 높이 초기화
        }
    }

}
