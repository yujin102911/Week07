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

    [Header("Washing Settings")]
    [SerializeField] private string shampooId; //��Ǫ ���̵�
    [SerializeField] private string mimicId; //�̹� ���̵�
    [SerializeField] private GameObject cleanMimicPrefab; //������ �̹� ������
    [SerializeField] private Transform spawnPoint; //������ �̹� ���� ��ġ

    private bool isTankrOn = false; //����ũ Ű��?
    private bool isWaterFull = false; //�� ����?
    private Vector3 initialWaterScale; //���� �ʱ� ������

    //���� �ȿ� ���� ������ ���� ��
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
        Collider2D col = GetComponent<Collider2D>();
        if (col == null || !col.isTrigger)
        {
            GameLogger.Instance.LogWarning(this, "BathController�� isTrigger�� �־�� �ǰų� �ݶ��̴��� ����");
        }
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }
    #endregion


    #region Public Methods

    ///<summary>�÷��̾ ��ȣ�ۿ��� �� ȣ��</summary>
    public bool Interact()
    {
        if (isTankrOn)
        {
            GameLogger.Instance.LogDebug(this, "�̹� �� ���� ��");
            return false;
        }
        //�� ��ũ ��������Ʈ ��ü
        if (tankSpriteOff != null) { tankSpriteOff.SetActive(false); }
        if (tankSpriteOn != null) { tankSpriteOn.SetActive(true); }
        //������ �� ä��� ����
        if (bathtupWater != null)
        {
            bathtupWater.gameObject.SetActive(true);
            StartCoroutine(FillBathtubCoroutine());
        }
        isTankrOn = true; //���� ����
        GameLogger.Instance.LogDebug(this, "����ũ�� �۵���Ű�� ������ ���� ä��� �����޻�");
        return true;
    }
    #endregion

    ///<summary>������ ���� ������ ä��� �ڷ�ƾ</summary>
    private IEnumerator FillBathtubCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startScale = bathtupWater.localScale; //���� ������ y = 0
        Vector3 targetScale = new Vector3(initialWaterScale.x, maxWaterLevelScaleY, initialWaterScale.z);

        while (elapsedTime < fillDuration)
        {
            float progress = elapsedTime / fillDuration;
            bathtupWater.localScale = Vector3.Lerp(startScale, targetScale, progress);

            elapsedTime += Time.deltaTime;
            yield return null; //���� �����ӱ��� ����ϼ�
        }

        bathtupWater.localScale = targetScale;
        GameLogger.Instance.LogDebug(this, "������ ���� ��á��");
    }

    ///<summary>��ô ������ Ȯ���ϴ� �Լ�</summary>
    private void CheckWashingConditions()
    {
        if (!isWaterFull) return; //���� �� ��á���� ��ô�� �غ� �ȵ���

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

    ///<summary>�̹��� �ı�� �������� �Ҹ��ϴ� �Լ�</summary>
    private void WashMimic()
    {
        GameLogger.Instance.LogDebug(this, "��ô ����! ������ �̹� ���ȿ�");

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

    ///<summary>���� ����</summary>
    private void ResetBath()
    {
        GameLogger.Instance.LogDebug(this, "���� ����");
        isTankrOn = false;
        isWaterFull = false;

        if (tankSpriteOff != null) tankSpriteOff.SetActive(true);
        if (tankSpriteOn != null) tankSpriteOn.SetActive(false);

        if (bathtupWater != null)
        {
            bathtupWater.localScale = new Vector3(initialWaterScale.x, 0f, initialWaterScale.z);
            bathtupWater.gameObject.SetActive(false);
        }

    }


    #region Trigger Methods
    ///<summary>Ʈ���� ����</summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isWaterFull) return;

        //���� Ʈ���� �ȿ� ���� �� Carryable�� �ƴϰų�, Carryable������ �÷��̾ ����ִ� ���¸� ����
        if (!other.TryGetComponent<Carryable>(out Carryable carryable) || carryable.carrying) return;

        if (carryable.Id == shampooId || carryable.Id == mimicId)
        {
            if (itemInBath_1 == null)
            {
                itemInBath_1 = other.gameObject;
            }
            else if (itemInBath_2 == null && other.gameObject != itemInBath_1)
            {
                itemInBath_2 = other.gameObject;
            }
            else
            {
                //�̹� �� á��
                return;
            }
            GameLogger.Instance.LogDebug(this, $"{carryable.Id} �������� ������ �־���");
            CheckWashingConditions();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == itemInBath_1)
        {
            itemInBath_1 = null;
            GameLogger.Instance.LogDebug(this, "������1�� �������� ����");
        }
        else if (other.gameObject == itemInBath_2)
        {
            itemInBath_2 = null;
            GameLogger.Instance.LogDebug(this, "������2�� �������� ����");
        }
    }
    #endregion
}
