using UnityEngine;
using UnityEngine.Events;
using System;

public enum InteractableId
{
    None,
    Table,
    DryingRack,
}

public enum FlagId
{
    None,
    Mimic_Happy,
    Dust_AllCleared,
    Boxes_StoredAll,
    Table_Used,
    DryingRack,
}

public enum ObjectiveType : byte
{
    InteractSet,        // Interact with each specified ID
    InteractSequence,   // (unused now)
    HoldOnTargets,      // (unused now)
    TriggerFlags,       // Complete when all flags are raised
    StayInArea,         // (unused now)
    Delivery            // (unused now)
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
    // Basic
    public string displayName;
    public ObjectiveType type;
    public bool optional;

    // Targets (InteractSet)
    public InteractableId targetEnum;
    public InteractableId[] targetEnums;
    [NonSerialized] public int[] targetHashes;

    // Flags (TriggerFlags)
    public FlagId requiredFlagEnum;
    public FlagId[] requiredFlagEnums;
    [NonSerialized] public int[] requiredFlagHashes;

    // Count
    public int requiredCount;
}