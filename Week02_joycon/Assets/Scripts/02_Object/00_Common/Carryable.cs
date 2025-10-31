using UnityEngine;

public class Carryable : MonoBehaviour
{
    [SerializeField] protected ItemName itemName;
    [SerializeField] private float large = -1;
    [SerializeField] private float weight = 1;
    [SerializeField] private float gravityScale = 1;
    public float spinAngle = 10;
    private Rigidbody2D _rigidbody;
    protected LayerMask obstacleMask;
    protected bool isCarried = false;
    public bool throwing = false;

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
        obstacleMask = LayerMask.GetMask(PlayerConstant.ObstacleMask);

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.gravityScale = weight * gravityScale;
        _rigidbody.mass = large * weight;
        if (throwing) { transform.Rotate(0f, 0f, spinAngle * Time.deltaTime); }//투척중이면 회전
    }

    private void Update()
    {
        if (isCarried)
        {
            throwing=false;//들고 있으면 투척 상태 해제
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (throwing)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle")) throwing = false;//충돌하면 투척 상태 해제
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isCarried == false) return;
        if ((obstacleMask.value & (1 << collision.gameObject.layer)) == 0) return;

        SetIsCarried(false);
        GameLogger.Instance.LogDebug(this, $"충돌로 떨어뜨림. 위치 : {transform.position}");

    }

    private void SetState()
    {
        _rigidbody.transform.SetParent(null);
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