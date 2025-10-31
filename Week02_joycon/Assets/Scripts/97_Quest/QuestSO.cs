using UnityEngine;
using UnityEngine.Events;
using System;

public enum InteractableId
{
    None,
    Table,
}

public enum FlagId
{
    None,
    ManagingMimic,
    WipingDust,
    Boxes_StoredAll,
    PreparingFood,
    DryingRack,
    EnterCastle,
    PlaceMimic,
    InstallGarlander,
}

[CreateAssetMenu(menuName = "Quest/Quest")]
public sealed class QuestSO : ScriptableObject
{
    [Header("ID / Meta")]
    public uint id;
    public string title;
    [TextArea] public string description;

    [Header("Progress Policy")]
    public bool sequentialObjectives = false;

    [Header("Objectives")]
    public ObjectiveDef[] objectives;

    [Header("Events")]
    [SerializeField] private UnityEvent onAllObjectivesCompleted;
    public event Action<QuestSO> Completed;

    public void RaiseCompleted()
    {
        var handler = Completed;
        handler?.Invoke(this);
        onAllObjectivesCompleted?.Invoke();
    }
}

[Serializable]
public struct ObjectiveDef
{
    public string displayName;
    public FlagId requiredFlagEnum;
    public FlagId[] requiredFlagEnums;
}