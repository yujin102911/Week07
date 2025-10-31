using UnityEngine;

public class Chair : InteractableWithItem
{
    [SerializeField] private Transform mimicPos;
    //protected override bool CanInteract(Carryable carryable) => QuestUI.questId == 1002;
    protected override bool CanInteract(Carryable carryable) => true;
    protected override bool InteractMethod(Carryable carryable)
    {
        carryable.transform.position = mimicPos.position;
        if(carryable.TryGetComponent(out Rigidbody2D rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        QuestRuntime.Instance.SetFlag(FlagId.PlaceMimic);
        return true;
    }
}