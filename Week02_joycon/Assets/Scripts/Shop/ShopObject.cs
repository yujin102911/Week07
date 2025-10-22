using UnityEngine;

public class ShopObject : MonoBehaviour
{
    [SerializeField] private bool isSellable;
    private bool isPlayerInRange;

    private void Update()
    {
        if (isPlayerInRange == false) return;
        if (Input.GetKeyDown(ShopConstant.OpenShopKey) == false) return;

        // TODO: Open Shop UI
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == false) return;
        isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == false) return;
        isPlayerInRange = false;
    }
}