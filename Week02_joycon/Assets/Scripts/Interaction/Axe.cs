using UnityEngine;

public class Axe : MonoBehaviour
{
    public int cutCount = 0;
    [SerializeField] private int cutRequire= 13;
    Controller2D controller2D;
    public bool falling;
    [SerializeField] int objectivesNum; //퀘스트 스크립터블 오브젝트 목표 번호
    [SerializeField] Carryable carryable;
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
        falling = controller2D.isFalling && carryable.carrying;
    }
}
