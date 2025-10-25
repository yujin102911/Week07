using System.Collections;
using System.Collections.Generic; // List�� ����ϱ� ���� �ʿ�
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [Header("���� ����")]
    [SerializeField] GameObject[] cloudPrefab; // ������ ���� ������ (�迭)
    [SerializeField] float minSpawnInterval = 1.0f;
    [SerializeField] float maxSpawnInterval = 5.0f;
     float spawnInterval = 3f; // �� �ʸ��� ��������

    [Header("���� ��ġ (������ ��)")]
    [SerializeField] float spawnYMin = 2f; // ������ Y ��ġ (�ּ�)
    [SerializeField] float spawnYMax = 5f; // ������ Y ��ġ (�ִ�)

    // ������ �������� ������ ����Ʈ
    private List<GameObject> activeClouds = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnCloudRoutine());

    }

    // �� ������ ����Ǹ� ����Ʈ�� û���մϴ�.
    void Update()
    {
        // ����Ʈ�� �ڿ������� ������ ��ȸ�մϴ�.
        for (int i = activeClouds.Count - 1; i >= 0; i--)
        {
            // ���� ����Ʈ�� i��° �׸��� null�̶�� (��, Cloud�� Destroy() �Ǿ��ٸ�)
            if (activeClouds[i] == null)
            {
                // ����Ʈ���� �ش� �׸��� �����մϴ�.
                activeClouds.RemoveAt(i);
                Debug.Log("�ı��� ������ ����Ʈ���� ����. ���� ���� ��: " + activeClouds.Count);
            }
        }
    }

    IEnumerator SpawnCloudRoutine()
    {
        // ������ ����Ǵ� ���� ���� �ݺ�
        while (true)
        {
            // 1. ���� ��ġ�� �����ϰ� ���� (Y��)
            float spawnY = transform.position.y + Random.Range(spawnYMin, spawnYMax);
            Vector3 spawnPosition = new Vector3(transform.position.x, spawnY, 0);

            // ���� [������ �κ�] ����
            // 2. ���� ������ �迭(cloudPrefab)���� ������ �ε���(0 ~ ����-1)�� �̽��ϴ�.
            int randomIndex = Random.Range(0, cloudPrefab.Length);

            // 3. �ش� �ε����� ���������� ������ �����մϴ�.
            GameObject newCloud = Instantiate(cloudPrefab[randomIndex], spawnPosition, Quaternion.identity);
            // ���� [������ �κ�] ����

            // 4. ����Ʈ�� �߰� (�̺�Ʈ ���� �ڵ� ����)
            activeClouds.Add(newCloud);

            spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);

            // 5. ���� �������� spawnInterval ��ŭ ����մϴ�.
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}