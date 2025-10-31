using UnityEngine;

public class DartMaker : MonoBehaviour
{
    [SerializeField] GameObject dartPrefab;

    /// <summary>
    /// 외부에서 호출할 수 있는 새 다트 생성 함수
    /// </summary>
    public void CreateNewDart()
    {
        if (dartPrefab != null)
        {
            // DartMaker 자신의 위치에 새 다트를 생성합니다.
            Instantiate(dartPrefab, transform.position, Quaternion.identity);
            
        }
        else
        {
            Debug.LogError("DartMaker에 dartPrefab이 할당되지 않았습니다!", this);
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

        // 부딪힌 다트는 파괴하고
        Destroy(carryable.gameObject);

        // 새로 만든 public 함수를 호출해서 새 다트를 생성합니다.
        CreateNewDart();
    }
}