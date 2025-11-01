using System.Collections.Generic;
using UnityEngine;

public class Stove : InteractableWithItem
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> firewoodSprites;
    [SerializeField] private Transform potSnapPoint;
    private int currentFirewood = 0;
    public bool isFireOn => currentFirewood == firewoodSprites.Count - 1;
    private Pot currentPot = null;

    public override bool TryInteract()
    {
        if (base.TryInteract() == true) return true;
        if (currentPot != null) return currentPot.TryInteract();

        return false;
    }

    public override bool Interact(Carryable target)
    {
        if (base.Interact(target) == true) return true;
        if (currentPot != null) return currentPot.Interact(target);

        return false;
    }

    protected override bool InteractMethod(Carryable carryable)
    {
        if (carryable.NameIs(ItemName.Firewood) == true) return AddFirewood();
        if (carryable.NameIs(ItemName.Pot) == true) return PutOnPot(carryable.GetComponent<Pot>());

        return false;
    }

    private bool AddFirewood()
    {
        if (isFireOn == true) return false;

        currentFirewood++;
        GameLogger.Instance.LogDebug(this, $"장작 추가, 현재: {currentFirewood}개");

        UpdateSprite();
        CheckCookingConditions();

        return true;
    }

    protected override bool CanInteract(Carryable carryable)
    {
        if (carryable.NameIs(ItemName.Pot) == true && currentPot != null) return false;
        return true;
    }

    private bool PutOnPot(Pot pot)
    {
        if (currentPot != null) return false;

        currentPot = pot;

        currentPot.transform.position = potSnapPoint.position;
        currentPot.transform.parent = potSnapPoint;
        currentPot.transform.rotation = Quaternion.identity;

        if (currentPot.TryGetComponent(out Rigidbody2D rigidbody))
        {
            rigidbody.bodyType = RigidbodyType2D.Kinematic;
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.angularVelocity = 0.0f;
            rigidbody.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
        }

        CheckCookingConditions();
        return true;
    }

    private void UpdateSprite() => spriteRenderer.sprite = firewoodSprites[currentFirewood];
    private void CheckCookingConditions()
    {
        if (isFireOn == false) return;
        if (currentPot != null) currentPot.CheckCookingConditions();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out Pot pot) && pot == currentPot)
        {
            currentPot = null;
        }
    }
}