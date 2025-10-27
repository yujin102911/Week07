using UnityEngine;

public class TargetTracker : MonoBehaviour
{
    [SerializeField] Transform targetPosition;
    [SerializeField] Vector3 targetOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position=targetPosition.position+ targetOffset;
        transform.localRotation= targetPosition.localRotation;
        if (Mathf.Sign(targetPosition.localScale.y) < 0)
        {
            transform.localScale = -Vector3.one;//상대가 뒤집히면 같이 뒤집히기
        }
        else
        {
            transform.localScale = Vector3.one;
        }
        
    }
}
