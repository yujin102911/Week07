using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class ArrowProjectile2D : MonoBehaviour
{
    [Header("Visual/Rot")]
    [SerializeField] private bool rotateToVelocity = true;
    [SerializeField] private bool forwardIsRight = true; // true: sprite�� ������ �ٶ�, false: ��

    [Header("Collision")]
    [SerializeField] private LayerMask blockMask = ~0;
    [SerializeField] private string playerTag = "Player";

    Rigidbody2D _rb;
    Collider2D _col;

    Vector2 _dir;
    float _speed;
    float _life;
    ArrowSpawner2D _owner;

    Collider2D _ignored; // ������ �浹���ÿ�
    bool _active;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();

        // ��õ ����: Kinematic + Continuous + isTrigger = true
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.simulated = true;
        _rb.gravityScale = 0f;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _col.isTrigger = true;
    }

    public void Activate(Vector2 position, Vector2 direction, float speed, float life, ArrowSpawner2D owner, Collider2D ignore)
    {
        _owner = owner;
        _dir = direction.normalized;
        _speed = speed;
        _life = life;
        _active = true;

        // ��ġ/ȸ��
        transform.position = position;
        if (rotateToVelocity)
        {
            var angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, forwardIsRight ? angle : (angle - 90f));
        }

        // ���� ignore ����
        if (_ignored && ignore != _ignored && _col)
            Physics2D.IgnoreCollision(_col, _ignored, false);

        // �� ignore ���
        _ignored = ignore;
        if (_ignored && _col)
            Physics2D.IgnoreCollision(_col, _ignored, true);
    }

    void Update()
    {
        if (!_active) return;

        // �̵�(Ʈ���� �浹 �̺�Ʈ�� �ŷ�)
        var pos = (Vector2)transform.position + _dir * (_speed * Time.deltaTime);
        _rb.MovePosition(pos);

        // ����ð�
        _life -= Time.deltaTime;
        if (_life <= 0f)
            Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_active) return;

        // ������ ����
        if (_ignored && other == _ignored) return;

        // �÷��̾� Ÿ��
        if (other.CompareTag(playerTag))
        {
            PlayerRespawn2D_RuntimeSO respawn = null;

            // Rigidbody2D�� ���� ��Ʈ���� ���� Ž��
            var rb2d = other.attachedRigidbody;
            if (rb2d) respawn = rb2d.GetComponent<PlayerRespawn2D_RuntimeSO>();
            if (!respawn) respawn = other.GetComponentInParent<PlayerRespawn2D_RuntimeSO>();

            if (respawn) respawn.Die();

            Despawn();
            return;
        }

        // ��/�ٴ� �� ���ŷ ���̾�
        if (((1 << other.gameObject.layer) & blockMask) != 0)
        {
            Despawn();
            return;
        }
    }

    void OnDisable()
    {
        // ���� ���: ���� ignore ���� ����
        if (_ignored && _col)
            Physics2D.IgnoreCollision(_col, _ignored, false);
        _ignored = null;
        _active = false;
    }

    void Despawn()
    {
        _active = false;
        if (_owner) _owner.Despawn(this);
        else gameObject.SetActive(false);
    }
}
