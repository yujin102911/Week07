using UnityEngine;

public class DartMaker : MonoBehaviour
{
    [SerializeField] GameObject dartPrefab;

    // ▼▼▼ 1. 100점 보상 프리팹을 등록할 변수 추가 ▼▼▼
    [SerializeField] GameObject specialRewardPrefab;

    /// <summary>
    /// 외부에서 호출할 수 있는 새 다트 생성 함수
    /// </summary>
    public void CreateNewDart()
    {
        if (dartPrefab != null)
        {
            Instantiate(dartPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("DartMaker에 dartPrefab이 할당되지 않았습니다!", this);
        }
    }

    // ▼▼▼ 2. 100점 보상 프리팹을 생성할 새 public 함수 추가 ▼▼▼
    /// <summary>
    /// 100점 달성 시 특별한 보상을 생성합니다.
    /// </summary>
    public void SpawnSpecialReward()
    {
        if (specialRewardPrefab != null)
        {
            // DartMaker 자신의 위치에 특별 보상을 생성합니다.
            Instantiate(specialRewardPrefab, transform.position, Quaternion.identity);
            Debug.Log("100점 달성! 특별 보상을 생성했습니다!");
        }
        else
        {
            Debug.LogError("DartMaker에 specialRewardPrefab이 할당되지 않았습니다!", this);
        }
    }


    // 이 트리거는 다트를 '재활용'하거나 '반납'하는 장소로 계속 사용할 수 있습니다.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent(out Carryable carryable) == false)
        {
            return; // Carryable 아이템이 아니면 무시
        }

        if (carryable.GetItemName() != ItemName.dart)
        {
            return; // 다트가 아니면 무시
        }

        Destroy(carryable.gameObject);
        CreateNewDart();
    }
}