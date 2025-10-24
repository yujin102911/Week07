using System;
using UnityEngine;

/// <summary>Minimal quest event bus: Interact + Flag only.</summary>
public static class QuestEvents
{
    // ---- Interact ----
    public struct InteractMsg
    {
        public string id;      // Interactable Id
        public int idHash;     // Animator.StringToHash(id)
        public Vector3 pos;    // World position (optional)
        public InteractionKind kind; // Keep type to avoid breaking callers

        // ---- Backward-compat stub (no-op in slim build) ----
        /// <summary>
        /// Backward-compatible stub. Returns false and does nothing in the slim quest build.
        /// Keep it to avoid compile errors in legacy callers.
        /// </summary>
        public static bool CancelLastInteract(string id) => false;
    }

    public static event Action<InteractMsg> OnInteract;

    public static void RaiseInteract(string id, Vector3 pos, InteractionKind kind = InteractionKind.Press)
    {
        var msg = new InteractMsg
        {
            id = id ?? string.Empty,
            idHash = Animator.StringToHash(id ?? string.Empty),
            pos = pos,
            kind = kind
        };
        OnInteract?.Invoke(msg);
    }

    // ---- Flags ----
    public static event Action<string> OnFlagRaised;

    public static event Action<string> OnFlagCleared;

    public static void RaiseFlag(string flagId)
        => OnFlagRaised?.Invoke(flagId ?? string.Empty);

    public static void RaiseFlagCleared(string flagId)
        => OnFlagCleared?.Invoke(flagId ?? string.Empty);
}