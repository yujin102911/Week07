using UnityEngine;

public class CarryableRotation : MonoBehaviour
{
    [SerializeField] private Carryable carryable;
    bool carried;

    private void Start()
    {
        if (carryable == null) TryGetComponent(out carryable);
        if (carryable == null) carryable = GetComponentInChildren<Carryable>();
    }

    void Update()
    {
        if (carryable.carrying)
        {
            carried = true;
            float zRot = transform.localEulerAngles.z; // 0~360 도 단위

            if (zRot >= 90f && zRot < 270f)
            {
                if (transform.localScale.y > 0)
                    transform.localScale = new Vector2(transform.localScale.x, -Mathf.Abs(transform.localScale.y));
            }
            else
            {
                if (transform.localScale.y < 0)
                    transform.localScale = new Vector2(transform.localScale.x, Mathf.Abs(transform.localScale.y));
            }
        }
        else
        {
            carried = false;
        }
    }
}
