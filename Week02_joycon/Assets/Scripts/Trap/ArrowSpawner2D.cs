using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ArrowSpawner2D : MonoBehaviour
{
    [Header("Prefab / Muzzle")]
    [SerializeField] private ArrowProjectile2D arrowPrefab;
    [SerializeField] private Transform muzzle;                 // ������ this.transform
    [SerializeField] private bool useMuzzleForward = true;     // true: muzzle�� �� �������� �߻�
    [SerializeField] private bool forwardIsRight = true;       // true: right, false: up
    [SerializeField, Range(-180f, 180f)] private float fireAngleDeg = 0f; // useMuzzleForward=false �� �� ���

    [Header("Fire Pattern")]
    [SerializeField, Min(0f)] private float startDelay = 0f;
    [SerializeField, Min(0.05f)] private float interval = 1.5f;
    [SerializeField, Min(1)] private int burstCount = 1;
    [SerializeField, Min(0f)] private float burstSpacing = 0.1f; // ����Ʈ �� ����
    [SerializeField, Range(0f, 30f)] private float jitterDeg = 0f; // ���� ����

    [Header("Projectile Params")]
    [SerializeField, Min(0.1f)] private float projectileSpeed = 12f;
    [SerializeField, Min(0.1f)] private float projectileLife = 5f;

    [Header("Pooling")]
    [SerializeField, Min(0)] private int prewarmCount = 8;
    [SerializeField] private bool allowGrowth = true;

    [Header("Collision Ignore (Optional)")]
    [SerializeField] private Collider2D spawnerColliderToIgnore; // ������ ��ü �ݶ��̴�(������ ȭ��� �浹 ����)

    [Header("Run")]
    [SerializeField] private bool playOnEnable = true;

    readonly Queue<ArrowProjectile2D> _pool = new Queue<ArrowProjectile2D>(32);
    float _timer;
    float _burstTimer;
    int _burstLeft;
    bool _firing;

    Transform _muzzle => muzzle ? muzzle : transform;

    void Awake()
    {
        if (!arrowPrefab)
        {
            Debug.LogError("[ArrowSpawner2D] arrowPrefab ������");
            enabled = false;
            return;
        }

        // Prewarm
        for (int i = 0; i < prewarmCount; ++i)
            _pool.Enqueue(CreateInstance());
    }

    void OnEnable()
    {
        if (playOnEnable) StartFiring();
    }

    void OnDisable()
    {
        StopFiring();
    }

    public void StartFiring()
    {
        _firing = true;
        _timer = startDelay;
        _burstLeft = 0;
        _burstTimer = 0f;
    }

    public void StopFiring()
    {
        _firing = false;
    }

    void Update()
    {
        if (!_firing) return;

        if (_burstLeft > 0)
        {
            _burstTimer -= Time.deltaTime;
            if (_burstTimer <= 0f)
            {
                SpawnOnce();
                _burstLeft--;
                _burstTimer = (_burstLeft > 0) ? burstSpacing : 0f;
            }
        }
        else
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                // �� ����Ʈ ����
                _burstLeft = burstCount;
                _burstTimer = 0f;
                _timer = interval; // ���� ����Ʈ���� ���
            }
        }
    }

    void SpawnOnce()
    {
        var dir = ComputeDirection();
        var pos = (Vector2)_muzzle.position;

        var arrow = Rent();
        arrow.Activate(pos, dir, projectileSpeed, projectileLife, this, spawnerColliderToIgnore);
    }

    Vector2 ComputeDirection()
    {
        Vector2 baseDir;
        if (useMuzzleForward)
        {
            var t = _muzzle;
            baseDir = forwardIsRight ? (Vector2)t.right : (Vector2)t.up;
        }
        else
        {
            var rad = fireAngleDeg * Mathf.Deg2Rad;
            baseDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        if (jitterDeg > 0f)
        {
            float j = Random.Range(-jitterDeg, jitterDeg);
            float rad = j * Mathf.Deg2Rad;
            var cs = Mathf.Cos(rad);
            var sn = Mathf.Sin(rad);
            return new Vector2(baseDir.x * cs - baseDir.y * sn, baseDir.x * sn + baseDir.y * cs).normalized;
        }

        return baseDir.normalized;
    }

    ArrowProjectile2D CreateInstance()
    {
        var inst = Instantiate(arrowPrefab);
        inst.gameObject.SetActive(false);
        return inst;
    }

    ArrowProjectile2D Rent()
    {
        if (_pool.Count > 0)
        {
            var a = _pool.Dequeue();
            a.gameObject.SetActive(true);
            return a;
        }
        if (allowGrowth) return CreateInstance();

        // �����ϸ� ���� ������ �� ��Ȱ���ϴ� ���� ��å�� ����������, ���⼱ �α׸�.
        Debug.LogWarning("[ArrowSpawner2D] Ǯ ��. allowGrowth=false");
        return CreateInstance();
    }

    public void Despawn(ArrowProjectile2D arrow)
    {
        if (!arrow) return;
        arrow.gameObject.SetActive(false);
        _pool.Enqueue(arrow);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        var origin = muzzle ? muzzle.position : transform.position;
        var dir = Application.isPlaying ? ComputeDirection()
                 : (useMuzzleForward
                    ? (forwardIsRight ? (Vector2)(muzzle ? muzzle.right : transform.right)
                                      : (Vector2)(muzzle ? muzzle.up : transform.up))
                    : new Vector2(Mathf.Cos(fireAngleDeg * Mathf.Deg2Rad),
                                  Mathf.Sin(fireAngleDeg * Mathf.Deg2Rad)));
        Gizmos.DrawLine(origin, origin + (Vector3)(dir.normalized * 1.2f));
        Gizmos.DrawSphere(origin, 0.04f);
    }
#endif
}
