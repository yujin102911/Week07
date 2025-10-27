using UnityEngine;

public class PlayerGeneralInteract : MonoBehaviour
{
    public float interactionRange = 1.5f;
    public LayerMask interactableMask;

    public bool FindAndInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange);

        GameObject closestObj = null;
        IInteractable interactable = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable i) == false) continue;
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObj = hit.gameObject;
                interactable = i;
            }
        }

        if (closestObj == null) return false;
        else return interactable.Interact();
    }
}