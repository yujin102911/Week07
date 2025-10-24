using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerInteraction : MonoBehaviour
{
    [Header("General Interaction")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private LayerMask ganeralInteractableMask;

    private Collider2D[] interactionHits = new Collider2D[3];
    private ContactFilter2D generalInteractableFilter;

    [SerializeField] private float interactCooldown = 0.5f;
    private float lastInteractTime = -1f;

    public void OnInteractAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            TryInteract();
        }
    }
    #region Unity Lifecycle
    private void Start()
    {
        LateInitialize();
    }
    #endregion  

    #region Initialization
    private void LateInitialize()
    {
        generalInteractableFilter = new ContactFilter2D();
        generalInteractableFilter.SetLayerMask(ganeralInteractableMask);
        generalInteractableFilter.useTriggers = true;
    }
    #endregion

    #region Public Methods
    public bool TryInteract()
    {
        if (Time.time - lastInteractTime < interactCooldown)
        {
            GameLogger.Instance.LogDebug(this, "�Ϲ� ��ȣ�ۿ� ��Ÿ�� ������");
            return false;
        }
        int hitCount = Physics2D.OverlapCircle(transform.position, interactionRange, generalInteractableFilter, interactionHits);

        if(hitCount > 0)
        {
            Collider2D closestHit = interactionHits[0];

            if (closestHit.TryGetComponent<IInteractable>(out IInteractable interactable))
            {
                interactable.Interact();
                lastInteractTime = Time.time;
                GameLogger.Instance.LogDebug(this, "��ȣ�ۿ� ����");
                return true;
            }
        }
        return false;
    }
    #endregion

}
