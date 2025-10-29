using UnityEngine;

public class Carryable : MonoBehaviour
{
    [SerializeField] protected ItemName itemName;
    [SerializeField] private float large = -1;
    [SerializeField] private float weight = 1;
    private Rigidbody2D _rigidbody;
    protected LayerMask obstacleMask;
    protected float lxw;
    protected bool isCarried = false;

    public ItemName GetItemName() => itemName;
    public float GetWeight() => weight;
    public bool GetIsCarried() => isCarried;
    public void SetIsCarried(bool isCarried)
    {
        this.isCarried = isCarried;
        SetState();
    }

    protected virtual void Start()
    {
        if (large < 0) large = transform.localScale.x * transform.localScale.y;
        lxw = large * weight;
        obstacleMask = LayerMask.GetMask(PlayerConstant.ObstacleMask);

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 1f + weight * 0.1f;
        _rigidbody.mass = lxw;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isCarried == false) return;
        if (collision.gameObject.layer != obstacleMask) return;

        SetIsCarried(false);
        GameLogger.Instance.LogDebug(this, $"충돌로 떨어뜨림. 위치 : {transform.position}");
    }

    private void SetState()
    {
        if (isCarried == false) _rigidbody.transform.SetParent(null, true);
        _rigidbody.bodyType = isCarried ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        _rigidbody.freezeRotation = isCarried;
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0.0f;

        // Rotation
        float zRot = transform.localEulerAngles.z;
        int sign = zRot > 90f && zRot < 270f ? -1 : 1;

        var localScale = transform.localScale;
        localScale.y = Mathf.Abs(localScale.y) * sign;
        transform.localScale = localScale;

        var rot = transform.eulerAngles;
        rot.z = (rot.z < 90f || rot.z > 270f) ? 0f : 180f;
        transform.eulerAngles = rot;
    }
}