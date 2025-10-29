using UnityEngine;

public class CarryableRotation : MonoBehaviour
{
    [SerializeField] private Carryable carryable;
    private void Start()
    {
        if (carryable == null) TryGetComponent(out carryable);
        if (carryable == null) carryable = GetComponentInChildren<Carryable>();
    }
}