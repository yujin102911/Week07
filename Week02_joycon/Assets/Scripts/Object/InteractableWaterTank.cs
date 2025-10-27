using UnityEngine;
using System.Collections;

public class InteractableWaterTank : MonoBehaviour, IInteractable
{
    [Header("WaterTank")]
    [SerializeField] private GameObject tankSpriteOff;
    [SerializeField] private GameObject tankSpriteOn;

    [Header("Bath Water")]
    [SerializeField] private Transform bathtupWater;
    [SerializeField] private float maxWaterLevelScaleY = 1.0f;
    [SerializeField] private float fillDuration = 5.0f;

    private bool isTankOn = false;
    private bool isWaterFull = false;
    private Vector3 initialWaterScale;

    private void Start()
    {
        LateInitialize();
    }

    private void LateInitialize()
    {
        if (tankSpriteOff != null) tankSpriteOff.SetActive(true);
        if (tankSpriteOn != null) tankSpriteOn.SetActive(false);

        if (bathtupWater != null)
        {
            initialWaterScale = bathtupWater.localScale;
            bathtupWater.localScale = new Vector3(initialWaterScale.x, 0f, initialWaterScale.z);
            bathtupWater.gameObject.SetActive(false);
        }
        else
        {
            GameLogger.Instance.LogError(this, "물 오브젝트가 설정되지 않음");
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.isTrigger)
        {
            GameLogger.Instance.LogWarning(this, "BathController의 콜라이더가 isTrigger가 아님");
        }
    }

    public bool Interact()
    {
        if (isTankOn)
        {
            GameLogger.Instance.LogDebug(this, "이미 물이 차는 중");
            return false;
        }
        if (tankSpriteOff != null) { tankSpriteOff.SetActive(false); }
        if (tankSpriteOn != null) { tankSpriteOn.SetActive(true); }
        if (bathtupWater != null)
        {
            bathtupWater.gameObject.SetActive(true);
            StartCoroutine(FillBathtubCoroutine());
        }
        isTankOn = true;
        GameLogger.Instance.LogDebug(this, "물탱크 작동 시작");
        return true;
    }

    private IEnumerator FillBathtubCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startScale = bathtupWater.localScale;
        Vector3 targetScale = new Vector3(initialWaterScale.x, maxWaterLevelScaleY, initialWaterScale.z);

        while (elapsedTime < fillDuration)
        {
            float progress = elapsedTime / fillDuration;
            bathtupWater.localScale = Vector3.Lerp(startScale, targetScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bathtupWater.localScale = targetScale;
        isWaterFull = true;
        GameLogger.Instance.LogDebug(this, "욕조가 가득 참");
    }
}