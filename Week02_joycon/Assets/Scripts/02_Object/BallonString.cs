using UnityEngine;

public class BallonString : MonoBehaviour
{
    [SerializeField] Transform Target;
    [SerializeField] float Thickness = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 localA = transform.parent.InverseTransformPoint(transform.position);
        Vector3 localB = transform.parent.InverseTransformPoint(Target.position);
        float localDistance = Vector2.Distance(localA, localB);
        transform.localScale = new Vector3 (localDistance, Thickness, 1);
        Vector2 direction = Target.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

    }
}
