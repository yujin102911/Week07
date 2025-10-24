using UnityEngine;
using System.Collections; // Coroutine ����� ���� �߰�

public class BathController : MonoBehaviour, IInteractable
{
    [Header("WaterTank")]
    [SerializeField] private GameObject tankSpriteOff; //�� �ѱ� ��
    [SerializeField] private GameObject tankSpriteOn; //�� ��

    [Header("Bath Water")]
    [SerializeField] private Transform bathtupWater; //�� ������Ʈ
    [SerializeField] private float maxWaterLevelScaleY = 1.0f;
    [SerializeField] private float fillDuration = 5.0f;

    private bool isTankrOn = false; //����ũ Ű��?
    private Vector3 initialWaterScale; //���� �ʱ� ������

    #region Unity Lifecycle
    private void Start()
    {
        LateInitialize();
    }
    #endregion

    #region Initialization
    private void LateInitialize()
    {
        //������ �⺻���� �����ְ� ������ ��������.
        if (tankSpriteOff != null) tankSpriteOff.SetActive(true);
        if (tankSpriteOn != null) tankSpriteOn.SetActive(false);

        if (bathtupWater != null)
        {
            initialWaterScale = bathtupWater.localScale;//�ʱ� ������ ����
            bathtupWater.localScale = new Vector3(initialWaterScale.x, 0f, initialWaterScale.z);
            bathtupWater.gameObject.SetActive(false); //ó���� �� �Ⱥ��̰�!!
        }
        else
        {
            GameLogger.Instance.LogError(this, "���� ���� ������� �ʾ���");
        }

    }
    #endregion


    #region Public Methods

    ///<summary>�÷��̾ ��ȣ�ۿ��� �� ȣ��</summary>
    public void Interact()
    {
        if (isTankrOn)
        {
            GameLogger.Instance.LogDebug(this, "�̹� �� ���� ��");
            return;
        }
        //�� ��ũ ��������Ʈ ��ü
        if (tankSpriteOff != null) { tankSpriteOff.SetActive(false); }
        if (tankSpriteOn != null) { tankSpriteOn.SetActive(true); }
        //������ �� ä��� ����
        if(bathtupWater != null)
        {
            bathtupWater.gameObject.SetActive(true );
            StartCoroutine(FillBathtubCoroutine());
        }
        isTankrOn = true; //���� ����
        GameLogger.Instance.LogDebug(this, "����ũ�� �۵���Ű�� ������ ���� ä��� �����޻�");

    }
    #endregion

    #region Private Methods

    ///<summary>������ ���� ������ ä��� �ڷ�ƾ</summary>
    private IEnumerator FillBathtubCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startScale = bathtupWater.localScale; //���� ������ y = 0
        Vector3 targetScale = new Vector3(initialWaterScale.x,  maxWaterLevelScaleY, initialWaterScale.z);

        while(elapsedTime < fillDuration)
        {
            float progress = elapsedTime / fillDuration;
            bathtupWater.localScale = Vector3.Lerp(startScale, targetScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null; //���� �����ӱ��� ����ϼ�
        }

        bathtupWater.localScale = targetScale;
        GameLogger.Instance.LogDebug(this, "������ ���� ��á��");
    }
    #endregion
}
