using UnityEngine;

public class UnlockFreeze : MonoBehaviour
{
    [SerializeField] Rigidbody2D rb;
    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Tree"))

        {
            Debug.Log("UnlockFreeze");
            rb.constraints &= ~RigidbodyConstraints2D.FreezePosition;
        }
    }
}
