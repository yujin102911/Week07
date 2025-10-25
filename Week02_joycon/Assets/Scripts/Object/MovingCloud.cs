using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 필요
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [Header("생성 설정")]
    [SerializeField] GameObject[] cloudPrefab; // 생성할 구름 프리팹 (배열)
    [SerializeField] float minSpawnInterval = 1.0f;
    [SerializeField] float maxSpawnInterval = 5.0f;
     float spawnInterval = 3f; // 몇 초마다 생성할지

    [Header("생성 위치 (오른쪽 밖)")]
    [SerializeField] float spawnYMin = 2f; // 생성될 Y 위치 (최소)
    [SerializeField] float spawnYMax = 5f; // 생성될 Y 위치 (최대)

    // 생성된 구름들을 관리할 리스트
    private List<GameObject> activeClouds = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnCloudRoutine());

    }

    // 매 프레임 실행되며 리스트를 청소합니다.
    void Update()
    {
        // 리스트를 뒤에서부터 앞으로 순회합니다.
        for (int i = activeClouds.Count - 1; i >= 0; i--)
        {
            // 만약 리스트의 i번째 항목이 null이라면 (즉, Cloud가 Destroy() 되었다면)
            if (activeClouds[i] == null)
            {
                // 리스트에서 해당 항목을 제거합니다.
                activeClouds.RemoveAt(i);
                Debug.Log("파괴된 구름을 리스트에서 제거. 현재 구름 수: " + activeClouds.Count);
            }
        }
    }

    IEnumerator SpawnCloudRoutine()
    {
        // 게임이 실행되는 동안 무한 반복
        while (true)
        {
            // 1. 생성 위치를 랜덤하게 설정 (Y축)
            float spawnY = transform.position.y + Random.Range(spawnYMin, spawnYMax);
            Vector3 spawnPosition = new Vector3(transform.position.x, spawnY, 0);

            // ▼▼▼ [수정된 부분] ▼▼▼
            // 2. 구름 프리팹 배열(cloudPrefab)에서 랜덤한 인덱스(0 ~ 길이-1)를 뽑습니다.
            int randomIndex = Random.Range(0, cloudPrefab.Length);

            // 3. 해당 인덱스의 프리팹으로 구름을 생성합니다.
            GameObject newCloud = Instantiate(cloudPrefab[randomIndex], spawnPosition, Quaternion.identity);
            // ▲▲▲ [수정된 부분] ▲▲▲

            // 4. 리스트에 추가 (이벤트 연결 코드 없음)
            activeClouds.Add(newCloud);

            spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);

            // 5. 다음 생성까지 spawnInterval 만큼 대기합니다.
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}