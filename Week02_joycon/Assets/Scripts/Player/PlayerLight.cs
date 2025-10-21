using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLight : MonoBehaviour
{
    public float interactionRange = 1.5f;
    public LayerMask interactableMask;

    public void OnTnteract(InputAction.CallbackContext context)
    {
        if (context.performed) FindAndInteract();
    }

    private void FindAndInteract()
    {   //�� �ֺ� interactionRange�ݰ� �ȿ� �ִ� interactableMask���̾� ������Ʈ ã��
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableMask);

        GameObject closestObj = null;
        float minDistance = Mathf.Infinity;

        foreach(Collider2D hit in hits)
        {
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObj = hit.gameObject;
            }
        }
        if (closestObj != null && closestObj.TryGetComponent<IInteractable>(out IInteractable interactable)) interactable.Interact();

    }

    

}
