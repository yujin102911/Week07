using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 이 줄을 반드시 추가해야 합니다.

[RequireComponent(typeof(Collider2D))]
public class DartsTarget : MonoBehaviour
{
    [Header("참조 설정 (References)")]
    [SerializeField] ParticleSystem dartParticle;
    [SerializeField] private DartMaker dartMaker;
    [SerializeField] private TextMeshProUGUI scoreText; // 스코어를 표시할 TMP UI

    [Header("움직임 설정 (Movement)")]
    [Tooltip("다트판이 중심 위치에서 위/아래로 움직일 최대 거리(진폭)입니다.")]
    [SerializeField] private float movementDistance = 1.5f;
    [Tooltip("다트판의 시작 속도입니다.")]
    [SerializeField] private float initialMovementSpeed = 2.0f;
    [Tooltip("다트판이 도달할 수 있는 최대 속도입니다.")]
    [SerializeField] private float maxMovementSpeed = 5.0f;

    [Header("스코어 및 속도 증가 설정 (Scoring)")]
    [Tooltip("다트가 맞을 때마다 얻는 점수입니다.")]
    [SerializeField] private int scorePerHit = 10;
    [Tooltip("속도가 증가하기 위해 필요한 점수입니다.")]
    [SerializeField] private int scorePerSpeedIncrease = 10;
    [Tooltip("점수 도달 시 속도가 얼마나 증가할지입니다.")]
    [SerializeField] private float speedIncreaseAmount = 0.5f;

    // --- 비공개 변수 ---
    private Vector3 startPosition;      // 다트판의 초기 시작 위치
    private float currentMovementSpeed; // 현재 속도 (점점 빨라짐)
    private int dartScore;              // 현재 다트 스코어
    private int nextSpeedIncreaseThreshold; // 다음 속도 증가가 일어날 목표 점수

    // ▼▼▼ 위치 초기화(점프) 현상을 막기 위한 누적 시간 변수 ▼▼▼
    private float cycleTime = 0f;

    private void Awake()
    {
        startPosition = transform.position;

        // 속도와 스코어 초기화
        currentMovementSpeed = initialMovementSpeed;
        dartScore = 0;
        nextSpeedIncreaseThreshold = scorePerSpeedIncrease; // 첫 번째 목표 점수 설정 (예: 10점)

        UpdateScoreText(); // UI 텍스트 초기화
    }

    // ▼▼▼ [수정됨] Update 메서드 ▼▼▼
    private void Update()
    {
        // Time.time 대신, 속도가 적용된 Time.deltaTime을 cycleTime에 계속 더해줍니다.
        // 이렇게 하면 속도가 변해도 cycleTime 값이 튀지 않고 부드럽게 증가율만 바뀝니다.
        cycleTime += Time.deltaTime * currentMovementSpeed;

        // Sin 함수에는 Time.time * speed 대신 누적된 cycleTime을 사용합니다.
        float yOffset = Mathf.Sin(cycleTime) * movementDistance;

        // 계산된 yOffset 값을 startPosition에 더해 새 위치를 만듭니다.
        Vector3 newPosition = new Vector3(startPosition.x, startPosition.y + yOffset, startPosition.z);

        // 오브젝트의 실제 위치를 계산된 새 위치로 업데이트합니다.
        transform.position = newPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Carryable 스크립트 가져오기
        if (collision.gameObject.TryGetComponent(out Carryable carryable) == false) return;

        // 2. 다트인지 확인
        if (carryable.GetItemName() != ItemName.dart) return;

        // 3. 던지는 중인지 확인
        if (carryable.throwing == false) return;

        // 4. Rigidbody 및 Collider 가져오기
        Rigidbody2D dartRigidbody = carryable.GetComponent<Rigidbody2D>();
        Collider2D dartCollider = carryable.GetComponent<Collider2D>();
        if (dartRigidbody == null) return;

        // 5. 다트 고정 (Trigger 켜기, Static으로 변경, 부모로 설정)
        dartCollider.isTrigger = true;
        dartRigidbody.bodyType = RigidbodyType2D.Static;
        carryable.transform.SetParent(this.transform);

        // 6. 파티클 재생
        if (dartParticle != null)
        {
            dartParticle.Play();
        }

        // 7. 새 다트 생성 요청
        if (dartMaker != null)
        {
            dartMaker.CreateNewDart();
        }
        else
        {
            Debug.LogWarning("DartsTarget에 DartMaker가 연결되지 않았습니다!", this);
        }

        // ▼▼▼ 8. 스코어 추가 및 속도 증가 로직 ▼▼▼
        AddScore(scorePerHit);
    }

    /// <summary>
    /// 스코어를 추가하고 UI를 업데이트하며, 속도 증가를 체크합니다.
    /// </summary>
    private void AddScore(int amount)
    {
        dartScore += amount;
        UpdateScoreText();
        CheckForSpeedIncrease();
    }

    /// <summary>
    /// TMP UI 텍스트를 현재 스코어로 업데이트합니다.
    /// </summary>
    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = dartScore.ToString();
        }
    }

    /// <summary>
    /// 스코어가 목표치에 도달했는지 확인하고 속도를 증가시킵니다.
    /// </summary>
    private void CheckForSpeedIncrease()
    {
        // 현재 스코어가 목표 점수(예: 10점, 20점...)에 도달했고,
        // 현재 속도가 최대 속도보다 낮은지 확인합니다.
        // (한 번에 20점 이상을 얻을 수도 있으니 while문 사용)
        while (dartScore >= nextSpeedIncreaseThreshold && currentMovementSpeed < maxMovementSpeed)
        {
            // 현재 속도를 증가시킵니다.
            currentMovementSpeed += speedIncreaseAmount;

            // 다음 목표 점수를 설정합니다. (예: 10 -> 20, 20 -> 30)
            nextSpeedIncreaseThreshold += scorePerSpeedIncrease;

            // 혹시 속도가 최대치를 초과했다면 최대치로 고정합니다.
            currentMovementSpeed = Mathf.Min(currentMovementSpeed, maxMovementSpeed);
        }
    }
}