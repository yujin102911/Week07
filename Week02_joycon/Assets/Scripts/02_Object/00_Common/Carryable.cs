using UnityEngine;

public class Carryable : MonoBehaviour
{
    [SerializeField] protected ItemName itemName;
    [SerializeField] private float large = -1;
    [SerializeField] private float weight = 1;
    [SerializeField] private float gravityScale = 1;
    public float spinAngle = 360;
    private Rigidbody2D _rigidbody;
    protected LayerMask obstacleMask => LayerMask.GetMask(PlayerConstant.ObstacleMask);
    protected bool isCarried;
    public bool throwing = false;
    [SerializeField, Tooltip("투척 끝나기까지 충돌 가능 횟수")] private int throwingCollisionNum = 10;
    private int throwingCollisionCurrent;

    public ItemName GetItemName() => itemName;
    public bool NameIs(ItemName name) => itemName == name;
    public float GetWeight() => weight;
    public bool GetIsCarried() => isCarried;
    public void SetIsCarried(bool isCarried)
    {
        this.isCarried = isCarried;
        SetState();
        if (isCarried == true)
        {
            GameLogger.Instance.LogInfo(this, $"Event:PickUp, Item:{itemName}, InstanceID:{this.GetInstanceID()}, Time:{Time.time}");
        }
        else
        {
            GameLogger.Instance.LogInfo(this, $"Event:Drop, Item:{itemName}, InstanceID:{this.GetInstanceID()}, Time:{Time.time}");
        }
    }

    protected virtual void Start()
    {
        throwingCollisionCurrent = throwingCollisionNum;
        if (large < 0) large = transform.localScale.x * transform.localScale.y;

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = weight * gravityScale;
        _rigidbody.mass = large * weight;

        SetIsCarried(false);
    }

    private void Update()
    {
        if (isCarried) throwing = false;
        if (throwing) transform.Rotate(0f, 0f, -spinAngle * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (throwing)
        {
            if (collision.gameObject.layer == obstacleMask) throwingCollisionCurrent = 0;
            else throwingCollisionCurrent--;

            if (throwingCollisionCurrent <= 0)
            {
                throwing = false;
                throwingCollisionCurrent = throwingCollisionNum;
            }
        }
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
        _rigidbody.bodyType = RigidbodyType2D.Dynamic;
        _rigidbody.constraints = RigidbodyConstraints2D.None;
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