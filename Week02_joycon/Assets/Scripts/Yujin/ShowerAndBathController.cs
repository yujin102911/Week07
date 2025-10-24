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

    private bool isTankrOn = false; //물탱크 키심?
    private Vector3 initialWaterScale; //물의 초기 스케일

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

    }
    #endregion


    #region Public Methods

    ///<summary>플레이어가 상호작용할 때 호출</summary>
    public void Interact()
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
        if(bathtupWater != null)
        {
            bathtupWater.gameObject.SetActive(true );
            StartCoroutine(FillBathtubCoroutine());
        }
        isTankrOn = true; //상태 변경
        GameLogger.Instance.LogDebug(this, "물탱크를 작동시키고 욕조에 물을 채우기 시작햇삼");

    }
    #endregion

    #region Private Methods

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
    #endregion
}
