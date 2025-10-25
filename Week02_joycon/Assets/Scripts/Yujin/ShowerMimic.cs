using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ShowerMimic : MonoBehaviour
{
    [Header("Recipe")]
    [SerializeField] private List<string> requiredItemIds;
    [SerializeField] private float processingTime = 1.0f;

    [Header("Transform")]
    [SerializeField] private List<Transform> snapPoints;
    [SerializeField] private Transform resultSpawnPoint;

    [Header("State (Internal)")]
    private bool isComplete = false;
    private HashSet<string> placedItemIds = new();
    private List<GameObject> placedItemObjects = new();

    #region Lifecycle

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isComplete) return;
        if (!other.TryGetComponent(out Carryable carryable)) return;

        if (!carryable.carrying && requiredItemIds.Contains(carryable.Id) && !placedItemIds.Contains(carryable.Id))
            PlaceItem(carryable);

    }
    #endregion

    #region Private Methods
    // 아이템을 다음 스냅 포인트 위치에 고정/등록
    private void PlaceItem(Carryable item)
    {
        GameObject itemObject = item.gameObject;

        // 배치 순서대로 스냅 포인트 선택
        Transform snapPoint = snapPoints[placedItemObjects.Count];

        // 위치/회전 고정
        itemObject.transform.position = snapPoint.position;
        itemObject.transform.rotation = Quaternion.identity;

        // 물리 중지(흔들림 방지)
        if (itemObject.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // 더 이상 상호작용되지 않게 비활성화
        item.enabled = false;

        // 내부 상태 갱신
        placedItemIds.Add(item.Id);
        placedItemObjects.Add(itemObject);

        GameLogger.Instance.LogDebug(this, $"재료 배치 완료: {item.Id}");

        // 모두 모였는지 검사
        CheckForCompletion();
    }

    /// <summary>
    /// 모든 필요한 재료가 정확히 모였는지 검사하고, 완료 시 처리 시작
    /// </summary>
    private void CheckForCompletion()
    {
        // 개수 부족이면 아직 미완료
        if (placedItemObjects.Count < requiredItemIds.Count) return;

        // 필요한 모든 ID가 배치되었는지 확인(중복 없는 고유 ID를 가정)
        foreach (string requiredId in requiredItemIds)
        {
            if (!placedItemIds.Contains(requiredId))
            {
                GameLogger.Instance.LogError(this, "필요한 ID가 모두 배치되지 않았습니다. 설정을 확인하세요.");
                return;
            }
        }

        // 여기까지 왔으면 완성
        isComplete = true;
        GameLogger.Instance.LogDebug(this, "조합 완료! 결과 생성 절차를 시작합니다...");
        StartCoroutine(ProcessCombination());
    }

    private IEnumerator ProcessCombination()
    {
        yield return new WaitForSeconds(processingTime);

        foreach (GameObject itemObject in placedItemObjects)
        {
            if (itemObject.TryGetComponent(out CarryableMimic carryableMimic))
            {
                carryableMimic.enabled = true;
                carryableMimic.CleanUp();
            }
            else Destroy(itemObject);
        }

        placedItemObjects.Clear();
    }
    #endregion
}