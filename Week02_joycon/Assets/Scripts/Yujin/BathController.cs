using UnityEngine;
using System.Collections; // Coroutine 사용을 위해 추가

public class BathController : MonoBehaviour, IInteractable
{
    [Header("WaterTank")]
    [SerializeField] private GameObject tankSpriteOff; //물 켜기 전
    [SerializeField] private GameObject tankSpriteOn; //물 켬

    [Header("Bath Water")]
    [SerializeField] private Transform bathtupWater; //물 오브젝트
    [SerializeField] private float maxWaterLevelScaleY = 1.0f;
    [SerializeField] private float fillDuration = 5.0f;

    [Header("Washing Settings")]
    [SerializeField] private string shampooId; //샴푸 아이디
    [SerializeField] private string mimicId; //미믹 아이디
    [SerializeField] private GameObject cleanMimicPrefab; //깨끗한 미믹 프리팹
    [SerializeField] private Transform spawnPoint; //깨끗한 미믹 생성 위치

    private bool isTankrOn = false; //물탱크 키심?
    private bool isWaterFull = false; //물 다참?
    private Vector3 initialWaterScale; //물의 초기 스케일

    //욕조 안에 놓인 아이템 추적 용
    private GameObject itemInBath_1 = null;
    private GameObject itemInBath_2 = null;

    #region Unity Lifecycle
    private void Start()
    {
        LateInitialize();
    }
    #endregion

    #region Initialization
    private void LateInitialize()
    {
        //꺼짐이 기본으로 켜져있고 켜짐은 꺼놔야함.
        if (tankSpriteOff != null) tankSpriteOff.SetActive(true);
        if (tankSpriteOn != null) tankSpriteOn.SetActive(false);

        if (bathtupWater != null)
        {
            initialWaterScale = bathtupWater.localScale;//초기 사이즈 저장
            bathtupWater.localScale = new Vector3(initialWaterScale.x, 0f, initialWaterScale.z);
            bathtupWater.gameObject.SetActive(false); //처음엔 물 안보이게!!
        }
        else
        {
            GameLogger.Instance.LogError(this, "욕조 물이 연결되지 않았음");
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.isTrigger)
        {
            GameLogger.Instance.LogWarning(this, "BathController에 isTrigger이 있어야 되거나 콜라이더가 없아");
        }
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }
    #endregion


    #region Public Methods

    ///<summary>플레이어가 상호작용할 때 호출</summary>
    public void Interact()
    {
        FillInter();

    }
    #endregion

    #region Private Methods
    ///<summary>Interact시 욕조에 물 채우는 함수</summary>
    private void FillInter()
    {
        if (isTankrOn)
        {
            GameLogger.Instance.LogDebug(this, "이미 물 차는 중");
            return;
        }
        //물 탱크 스프라이트 교체
        if (tankSpriteOff != null) { tankSpriteOff.SetActive(false); }
        if (tankSpriteOn != null) { tankSpriteOn.SetActive(true); }
        //욕조에 물 채우기 시작
        if (bathtupWater != null)
        {
            bathtupWater.gameObject.SetActive(true);
            StartCoroutine(FillBathtubCoroutine());
        }
        isTankrOn = true; //상태 변경
        GameLogger.Instance.LogDebug(this, "물탱크를 작동시키고 욕조에 물을 채우기 시작햇삼");
    }

    ///<summary>욕조에 물을 서서히 채우는 코루틴</summary>
    private IEnumerator FillBathtubCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startScale = bathtupWater.localScale; //현재 스케일 y = 0
        Vector3 targetScale = new Vector3(initialWaterScale.x,  maxWaterLevelScaleY, initialWaterScale.z);

        while(elapsedTime < fillDuration)
        {
            float progress = elapsedTime / fillDuration;
            bathtupWater.localScale = Vector3.Lerp(startScale, targetScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null; //다음 프레임까지 대기하쇼
        }

        bathtupWater.localScale = targetScale;
        GameLogger.Instance.LogDebug(this, "욕조에 물이 다찼다");
    }

    ///<summary>세척 조건을 확인하는 함수</summary>
    private void CheckWashingConditions()
    {
        if (!isWaterFull) return; //물도 다 안찼으면 세척할 준비가 안됐음
        
        if (itemInBath_1 == null || itemInBath_2 == null) return;

        string id1 = itemInBath_1.GetComponent<Carryable>().Id;
        string id2 = itemInBath_2.GetComponent<Carryable>().Id;

        bool hasMimic = (id1 == mimicId || id2 == mimicId);
        bool hasShampoo = (id1 == shampooId || id2 == shampooId);

        if (hasMimic && hasShampoo)
        {
            WashMimic();
        }

    }

    ///<summary>미믹을 씻기고 아이템을 소모하는 함수</summary>
    private void WashMimic()
    {
        GameLogger.Instance.LogDebug(this, "세척 성공! 깨끗한 미믹 나옴요");

        if (cleanMimicPrefab != null)
        {
            Instantiate(cleanMimicPrefab, spawnPoint.position, Quaternion.identity);
        }
        Destroy(itemInBath_1);
        Destroy(itemInBath_2);
        itemInBath_1 = null;
        itemInBath_2 = null;

        ResetBath();
    }

    ///<summary>욕조 리셋</summary>
    private void ResetBath()
    {
        GameLogger.Instance.LogDebug(this, "욕조 리셋");
        isTankrOn = false;
        isWaterFull = false;
        
        if(tankSpriteOff != null) tankSpriteOff.SetActive(true);
        if(tankSpriteOn != null) tankSpriteOn.SetActive(false);

        if (bathtupWater != null)
        {
            bathtupWater.localScale = new Vector3(initialWaterScale.x, 0f, initialWaterScale.z);
            bathtupWater.gameObject.SetActive(false);
        }

    }


    #region Trigger Methods
    ///<summary>트리거 로직</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isWaterFull) return;

        //만약 트리거 안에 들어온 게 Carryable이 아니거나, Carryable이지만 플레이어가 들고있는 상태면 무시
        if (!other.TryGetComponent<Carryable>(out Carryable carryable) || carryable.carrying) return;

        if (carryable.Id == shampooId || carryable.Id == mimicId)
        {
            if (itemInBath_1 == null)
            {
                itemInBath_1 = other.gameObject;
            }
            else if(itemInBath_2 == null && other.gameObject != itemInBath_1)
            {
                itemInBath_2 = other.gameObject;
            }
            else
            {
                //이미 다 찼음
                return;
            }
            GameLogger.Instance.LogDebug(this, $"{carryable.Id} 아이템을 욕조에 넣었음");
            CheckWashingConditions();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == itemInBath_1)
        {
            itemInBath_1 = null;
            GameLogger.Instance.LogDebug(this, "아이템1을 욕조에서 뺐음");
        }
        else if (other.gameObject == itemInBath_2)
        {
            itemInBath_2 = null;
            GameLogger.Instance.LogDebug(this, "아이템2를 욕조에서 뺐음");
        }
    }
    #endregion
    #endregion
}
