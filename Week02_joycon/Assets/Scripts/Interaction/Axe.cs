using UnityEngine;

public class Axe : MonoBehaviour
{
    public int cutCount = 0;
    [SerializeField] private int cutRequire= 13;
    Controller2D controller2D;
    public bool falling;
    [SerializeField] QuestSO soDef;//퀘스트 스크립터블 오브젝트
    [SerializeField] int objectivesNum; //퀘스트 스크립터블 오브젝트 목표 번호
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (controller2D == null)
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo != null)
            {
                controller2D = pgo.GetComponent<Controller2D>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        falling = controller2D.isFalling?  true: false;
        if (cutCount >= cutRequire)
        {
            string[] targetId = soDef.objectives[objectivesNum].comp;//퀘스트 타겟 아이디 배열
            for (int i = 0; i < targetId.Length; i++)
            {
                if (targetId[objectivesNum] == "AxeCutTree")//내 아이디와 같으면
                {
                    targetId[objectivesNum] = "";//공백으로(제거 처리)
                    break;
                }
            }
            Destroy(gameObject);
        }
    }
}
