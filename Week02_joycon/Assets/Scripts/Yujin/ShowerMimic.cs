using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ShowerMimic : MonoBehaviour
{
    [Header("Recipe")]
    [SerializeField] private List<string> requiredItemIds; //�ʿ��� ������ ���̵��
    [SerializeField] private GameObject resultPrefab; //��� ������Ʈ ������
    [SerializeField] private float processingTime = 1.0f; //��� ������Ʈ�� ��������� �ɸ� �ð�

    [Header("Transform")]
    [SerializeField] private List<Transform> snapPoints; //�ʿ��� �����۵��� ������ ��ġ (requiredItemIds�� ����, ���� ����ߵ�)
    [SerializeField] private Transform resultSpawnPoint; //������� ������ ��ġ

    [Header("State (Internal)")]
    private bool isComplete = false;
    private HashSet<string> placedItemIds = new HashSet<string>(); //���� �÷��� �����۵��� ID�� ����
    private List<GameObject> placedItemObjects = new List<GameObject>(); //���� �÷��� �����۵��� GameObject�� ����(���߿� �ı��ϱ����ؼ�)

    #region Lifecycle
    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger) { GameLogger.Instance.LogWarning(this, "IsTrigger������"); }
        if (requiredItemIds.Count != snapPoints.Count)
        {
            GameLogger.Instance.LogError(this, $"�ʿ��� �������� ����: {requiredItemIds.Count}�� ���� ����Ʈ ����{snapPoints.Count}�� ��ġ���� ����");
        }
        if (resultSpawnPoint == null)
        {
            GameLogger.Instance.LogWarning(this, "resultSpawnPoint�� ������������. �ӽ÷� �� �������� ��ġ�� spawnPoint�� ����");
            resultSpawnPoint = transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isComplete) return; //���� �̹� ������ �������� ��

        if (!other.TryGetComponent<Carryable>(out Carryable carryable)) return; //���� Carryable�� �ƴϸ� ��

        if (!carryable.carrying && requiredItemIds.Contains(carryable.Id) && !placedItemIds.Contains(carryable.Id))
        {
            PlaceItem(carryable);
        }
    }
    #endregion

    #region Private Methods
    private void PlaceItem(Carryable item)
    {
        GameObject itemObject = item.gameObject;

        Transform snapPoint = snapPoints[placedItemObjects.Count];

        itemObject.transform.position = snapPoint.position;
        itemObject.transform.rotation = Quaternion.identity;

        if (itemObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        item.enabled = false;

        placedItemIds.Add(item.Id);
        placedItemObjects.Add(itemObject);
        GameLogger.Instance.LogDebug(this, $"��� �߰�! {item.Id}");

        CheckForCompletion();

    }

    ///<summary>��� ��ᰡ �� �𿴴��� Ȯ���ϴ� �Լ�</summary>
    private void CheckForCompletion()
    {
        // 1. ���� ���� ������ ���� �ʿ��� ������ ���� ������ Ȯ��
        if (placedItemObjects.Count < requiredItemIds.Count)
        {
            // ���� ��ᰡ ������
            return;
        }

        // 2. (������) ��� ID�� ��Ȯ�� ��ġ�ϴ��� ��Ȯ�� (������ �� 1������ ���)
        // requiredItemIds ����Ʈ�� ��� ID�� placedItemIds �ؽü¿� ���ԵǾ� �ִ��� Ȯ��
        foreach (string requiredId in requiredItemIds)
        {
            if (!placedItemIds.Contains(requiredId))
            {
                // �� ���� ������ ���� �߻����� ������, ������ġ
                GameLogger.Instance.LogError(this, "��� ������ ������ ID�� ����ġ�մϴ�. ������ Ȯ�� �ʿ�.");
                return;
            }
        }

        // ��� ���� ����! ���� ����
        isComplete = true; // �ߺ� ���� ����
        GameLogger.Instance.LogDebug(this, "���� ����! ����� ���� ����...");
        StartCoroutine(ProcessCombination());
    }

    ///<summary>������ �� ��Ḧ �ı��ϰ� ������� ����</summary>
    private IEnumerator ProcessCombination()
    {
        // 1. ������ �� �� �ֵ��� ��� ���
        yield return new WaitForSeconds(processingTime);

        // 2. ��� ���(������ �̹�, ��Ǫ) ������Ʈ �ı�
        foreach (GameObject itemObject in placedItemObjects)
        {
            Destroy(itemObject);
        }
        placedItemObjects.Clear(); // ����Ʈ ����

        // 3. �����(�İ��� �̹�) ����
        if (resultPrefab != null)
        {
            Instantiate(resultPrefab, resultSpawnPoint.position, Quaternion.identity);
        }

        // 4. (����) ���մ� �ʱ�ȭ (�ʿ��ϴٸ�)
        // ResetStation();
    }

    /// <summary>
    /// ���մ븦 �ٽ� ����ϵ��� �ʱ�ȭ�ϴ� �Լ�
    /// </summary>
    public void ResetStation()
    {
        isComplete = false;
        placedItemIds.Clear();
        // placedItemObjects�� �̹� �����
        GameLogger.Instance.LogDebug(this, "���մ밡 �ʱ�ȭ�Ǿ����ϴ�.");
    }
    
    #endregion

}
