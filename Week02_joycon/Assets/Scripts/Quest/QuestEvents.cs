using System;
using UnityEngine;
using Game.Quests;

public static class QuestEvents
{
    public struct InteractMsg
    {
        public InteractableId id;
        public Vector3 pos;
        public InteractionKind kind;
    }

    public static event Action<InteractMsg> OnInteract;

    public static void RaiseInteract(InteractableId id, Vector3 pos, InteractionKind kind = InteractionKind.Press)
        => OnInteract?.Invoke(new InteractMsg { id = id, pos = pos, kind = kind });

    public static event Action<FlagId> OnFlagRaised;
    public static event Action<FlagId> OnFlagCleared;

    public static void RaiseFlag(FlagId flag) => OnFlagRaised?.Invoke(flag);
    public static void RaiseFlagCleared(FlagId flag) => OnFlagCleared?.Invoke(flag);

    public static bool CancelLastInteract(InteractableId id) => false;
}