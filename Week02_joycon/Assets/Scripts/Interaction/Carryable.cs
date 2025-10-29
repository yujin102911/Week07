using UnityEngine;

public class Carryable : MonoBehaviour
{
    [SerializeField] protected ItemName itemName;
    [SerializeField] private float large = -1;
    [SerializeField] private float weight = 1;
    private Rigidbody2D _rigidbody;
    protected LayerMask obstacleMask;
    protected float lxw;
    protected bool isCarrying = false;

    public ItemName GetItemName() => itemName;
    public float GetWeight() => weight;
    public bool GetIsCarrying() => isCarrying;
    public void SetIsCarrying(bool isCarrying)
    {
        this.isCarrying = isCarrying;
        SetUpRigidbody();
    }

    public string Id;
    public int ScannerID;

    protected virtual void Start()
    {
        if (large < 0) large = transform.localScale.x * transform.localScale.y;
        lxw = large * weight;
        obstacleMask = LayerMask.GetMask(PlayerConstant.ObstacleMask);

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 1f + weight * 0.1f;
        _rigidbody.mass = lxw;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isCarrying == false) return;
        if (collision.gameObject.layer != obstacleMask) return;

        SetIsCarrying(false);
        GameLogger.Instance.LogDebug(this, $"충돌로 인해 짐 떨어뜨림. 위치 : {transform.position}");
    }

    private void SetUpRigidbody()
    {
        if (isCarrying == false) _rigidbody.transform.SetParent(null, true);
        _rigidbody.bodyType = isCarrying ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        _rigidbody.freezeRotation = isCarrying;
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0.0f;


        // Rotation
        var localScale = transform.localScale;
        float zRot = transform.localEulerAngles.z;

        if (zRot > 90f && zRot < 270f)
        {
            if (localScale.y > 0) localScale.y = -Mathf.Abs(localScale.y);
        }
        else if (localScale.y < 0) localScale.y = Mathf.Abs(localScale.y);

        transform.localScale = localScale;
    }
}