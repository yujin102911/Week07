using UnityEngine;

public class Chair : InteractableWithItem
{
    [SerializeField] private Transform mimicPos;
    protected override bool CanInteract(Carryable carryable) => QuestUI.questId == 1002;
    protected override bool InteractMethod(Carryable carryable)
    {
        carryable.transform.position = mimicPos.position;
        QuestRuntime.Instance.SetFlag(FlagId.PlaceMimic);
        return true;
    }
}