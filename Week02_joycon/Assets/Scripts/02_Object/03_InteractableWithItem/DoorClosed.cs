using UnityEngine;

public class DoorClosed : InteractableWithItem
{
    [SerializeField] private GameObject doorOpened;
    private bool isOpen = false;

    protected override bool CanInteract(Carryable carryable) => isOpen == false;
    protected override bool InteractMethod(Carryable carryable)
    {
        isOpen = true;
        doorOpened.SetActive(true);
        gameObject.SetActive(false);
        GameLogger.Instance.LogDebug(this, "Door opened.");
        return true;
    }
}