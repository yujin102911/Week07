using UnityEngine;

public class PlayerInteractGeneral : MonoBehaviour
{
    public bool TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, PlayerConstant.InteractableRange);

        GameObject closestObj = null;
        IInteractable interactable = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable i) == false) continue;
            if (hit.TryGetComponent(out Carryable carryable) == true && carryable.GetIsCarried() == true) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObj = hit.gameObject;
                interactable = i;
            }
        }

        if (closestObj == null) return false;
        else return interactable.TryInteract();
    }
}