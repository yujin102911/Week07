using System.Collections.Generic;
using UnityEngine;

public class InteractableStove : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> firewoodSprites;
    [SerializeField] private Transform potSnapPoint;
    private int currentFirewood = 0;
    public bool isFireOn => currentFirewood == firewoodSprites.Count - 1;
    private InteractablePot potOnStove = null;

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public bool TryInteract()
    {
        if (potOnStove == null)
        {
            if (InventoryManager.Instance.HasItem(ItemName.Firewood) == false)
            {
                if (InventoryManager.Instance.HasItem(ItemName.Pot) == false) return false;
                InventoryManager.Instance.GetComponent<PlayerInteractCarryable>().TryDrop(ItemName.Pot);
                return true;
            }

            InventoryManager.Instance.RemoveAndDestroyItem(ItemName.Firewood);
            return AddFirewood();
        }

        if (potOnStove.TryInteract() == true) return true;
        if (InventoryManager.Instance.HasItem(ItemName.Firewood) == false) return false;

        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.Firewood);
        return AddFirewood();
    }

    private bool AddFirewood()
    {
        if (isFireOn == true) return false;

        currentFirewood++;
        GameLogger.Instance.LogDebug(this, $"장작 추가, 현재: {currentFirewood}개");

        UpdateSprite();
        if (isFireOn == true && potOnStove != null) potOnStove.CheckCookingConditions();

        return true;
    }

    private void UpdateSprite() => spriteRenderer.sprite = firewoodSprites[currentFirewood];

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out InteractablePot pot))
        {
            if (pot.GetComponent<Carryable>() != null && !pot.GetComponent<Carryable>().GetIsCarried())
            {
                potOnStove = pot;
                pot.SetCurrentStove(this);

                pot.transform.position = potSnapPoint.position;
                pot.transform.localScale = Vector2.one * 1.2f;
                pot.transform.rotation = Quaternion.identity;
                if (pot.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                pot.CheckCookingConditions();
            }
        }
        else if (collision.TryGetComponent(out Carryable carryable))
        {
            if (carryable.GetItemName() == ItemName.Firewood && !carryable.GetIsCarried())
            {
                AddFirewood();
                Destroy(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out InteractablePot pot) && pot == potOnStove)
        {
            potOnStove = null;
            pot.SetCurrentStove(null);
        }
    }
}