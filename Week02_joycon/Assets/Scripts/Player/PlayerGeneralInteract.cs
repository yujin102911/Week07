using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGeneralInteract : MonoBehaviour
{
    public float interactionRange = 1.5f;
    public LayerMask interactableMask;

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed) FindAndInteract();
    }

    private void FindAndInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableMask);

        GameObject closestObj = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObj = hit.gameObject;
            }
        }
        Debug.Log("Interacted with " + closestObj);
        if (closestObj != null && closestObj.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            Debug.Log("Interacted with " + closestObj.name);
            interactable.Interact();
        }
    }
}