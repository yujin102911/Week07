using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class CarryableRotation : MonoBehaviour
{
    [SerializeField]Carryable carryable;
    bool carried;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    // Update is called once per frame
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
