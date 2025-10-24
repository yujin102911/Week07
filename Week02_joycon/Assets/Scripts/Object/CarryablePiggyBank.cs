using UnityEngine;

public class CarryablePiggyBank : Carryable
{
    [SerializeField] private int coinsCount;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform groundCheck;
    private float minDropHeight = 3f;
    private float groundCheckRadius = 0.15f;

    private bool _isGrounded;
    private bool _prevGrounded;
    private bool _prevCarrying;

    private float _leaveGroundY; // 지면 떠난 순간의 Y
    private float _maxFall;      // 공중에서 기록한 최대 낙하량

    protected override void Start()
    {
        base.Start();

        _isGrounded = IsGroundedNow();
        _prevGrounded = _isGrounded;
        _prevCarrying = carrying;
        _leaveGroundY = transform.position.y;
        _maxFall = 0f;
    }

    private void FixedUpdate()
    {
        _prevGrounded = _isGrounded;
        _isGrounded = IsGroundedNow();

        // carrying 상태 변화 감지 (들고 있다 -> 내려놓음)
        if (_prevCarrying && !carrying)
        {
            // 내려놓은 순간을 새로운 낙하 시작점으로 “무조건” 설정
            _leaveGroundY = transform.position.y;
            _maxFall = 0f;
        }

        // 지면을 떠난 첫 순간(자연 점프/굴러서 떨어짐)인데 "들고 있지 않을 때만" 추적 시작
        if (_prevGrounded && !_isGrounded && !carrying)
        {
            _leaveGroundY = transform.position.y;
            _maxFall = 0f;
        }

        // 공중에 있고, 들고 있지 않을 때만 낙하량 갱신
        if (!_isGrounded && !carrying)
        {
            float drop = _leaveGroundY - transform.position.y;
            if (drop > _maxFall) _maxFall = drop;
        }

        _prevCarrying = carrying;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & maskObstacle) == 0) return;
        if (carrying) return;

        // 실제 낙하거리로 판정
        if (_maxFall >= minDropHeight) OnHardLanding(_maxFall);

        // 착지 후 초기화
        _maxFall = 0f;
        _leaveGroundY = transform.position.y;
    }

    private bool IsGroundedNow()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, maskObstacle) != null;
    }

    private void OnHardLanding(float dropHeight)
    {
        Debug.Log($"[CarryablePiggyBank] 낙하 판정! 떨어진 높이: {dropHeight:0.00}m 이상 → 실행");

        CreateCoins(coinsCount);
        Destroy(gameObject);
    }

    private void CreateCoins(int count)
    {
        if (count >= coinsCount) count = coinsCount;

        for (int i = 0; i < count; i++)
        {
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                var force = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(5f, 10f);
                coinRb.AddForce(force, ForceMode2D.Impulse);
            }

            coinsCount--;
        }
    }

#if UNITY_EDITOR
    // 에디터에서 GroundCheck 확인용 기즈모
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
#endif
}