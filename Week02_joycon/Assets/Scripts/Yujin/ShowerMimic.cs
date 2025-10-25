using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ShowerMimic : MonoBehaviour
{
    [Header("Recipe")]
    [SerializeField] private List<string> requiredItemIds; //필요한 아이템 아이디들
    [SerializeField] private GameObject resultPrefab; //결과 오브젝트 프리팹
    [SerializeField] private float processingTime = 1.0f; //결과 오브젝트가 나오기까지 걸릴 시간

    [Header("Transform")]
    [SerializeField] private List<Transform> snapPoints; //필요한 아이템들이 스냅될 위치 (requiredItemIds랑 순서, 개수 맞춰야됨)
    [SerializeField] private Transform resultSpawnPoint; //결과물이 생성될 위치

    [Header("State (Internal)")]
    private bool isComplete = false;
    private HashSet<string> placedItemIds = new HashSet<string>(); //현재 올려진 아이템들의 ID를 추적
    private List<GameObject> placedItemObjects = new List<GameObject>(); //현재 올려진 아이템들의 GameObject를 저장(나중에 파괴하기위해서)

    #region Lifecycle
    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger) { GameLogger.Instance.LogWarning(this, "IsTrigger안켰음"); }
        if (requiredItemIds.Count != snapPoints.Count)
        {
            GameLogger.Instance.LogError(this, $"필요한 아이템의 개수: {requiredItemIds.Count}와 스냅 포인트 개수{snapPoints.Count}가 일치하지 않음");
        }
        if (resultSpawnPoint == null)
        {
            GameLogger.Instance.LogWarning(this, "resultSpawnPoint가 설정되지않음. 임시로 현 아이템의 위치를 spawnPoint로 설정");
            resultSpawnPoint = transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isComplete) return; //만약 이미 조합이 끝났으면 끝

        if (!other.TryGetComponent<Carryable>(out Carryable carryable)) return; //만약 Carryable이 아니면 끝

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
        GameLogger.Instance.LogDebug(this, $"재료 추가! {item.Id}");

        CheckForCompletion();

    }

    ///<summary>모든 재료가 다 모였는지 확인하는 함수</summary>
    private void CheckForCompletion()
    {
        // 1. 현재 놓인 아이템 수가 필요한 아이템 수와 같은지 확인
        if (placedItemObjects.Count < requiredItemIds.Count)
        {
            // 아직 재료가 부족함
            return;
        }

        // 2. (선택적) 모든 ID가 정확히 일치하는지 재확인 (보통은 위 1번으로 충분)
        // requiredItemIds 리스트의 모든 ID가 placedItemIds 해시셋에 포함되어 있는지 확인
        foreach (string requiredId in requiredItemIds)
        {
            if (!placedItemIds.Contains(requiredId))
            {
                // 이 경우는 로직상 거의 발생하지 않지만, 안전장치
                GameLogger.Instance.LogError(this, "재료 개수는 맞으나 ID가 불일치합니다. 레시피 확인 필요.");
                return;
            }
        }

        // 모든 조건 충족! 조합 시작
        isComplete = true; // 중복 실행 방지
        GameLogger.Instance.LogDebug(this, "조합 성공! 결과물 생성 시작...");
        StartCoroutine(ProcessCombination());
    }

    ///<summary>딜레이 후 재료를 파괴하고 결과물을 생성</summary>
    private IEnumerator ProcessCombination()
    {
        // 1. 유저가 볼 수 있도록 잠시 대기
        yield return new WaitForSeconds(processingTime);

        // 2. 모든 재료(더러운 미믹, 샴푸) 오브젝트 파괴
        foreach (GameObject itemObject in placedItemObjects)
        {
            Destroy(itemObject);
        }
        placedItemObjects.Clear(); // 리스트 비우기

        // 3. 결과물(씻겨진 미믹) 생성
        if (resultPrefab != null)
        {
            Instantiate(resultPrefab, resultSpawnPoint.position, Quaternion.identity);
        }

        // 4. (선택) 조합대 초기화 (필요하다면)
        // ResetStation();
    }

    /// <summary>
    /// 조합대를 다시 사용하도록 초기화하는 함수
    /// </summary>
    public void ResetStation()
    {
        isComplete = false;
        placedItemIds.Clear();
        // placedItemObjects는 이미 비워짐
        GameLogger.Instance.LogDebug(this, "조합대가 초기화되었습니다.");
    }
    
    #endregion

}
