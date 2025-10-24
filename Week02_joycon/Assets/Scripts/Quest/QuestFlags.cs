#nullable enable
using System.Runtime.CompilerServices;

namespace Game.Quests
{
    /// <summary>Thin facade over QuestRuntime (flags only).</summary>
    public static class QuestFlags
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(string? flagId) => QuestRuntime.Instance.HasFlag(flagId ?? string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(int flagHash) => QuestRuntime.Instance.HasFlag(flagHash);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set(string flagId) => QuestRuntime.Instance.SetFlag(flagId ?? string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Clear(string flagId) => QuestRuntime.Instance.ClearFlag(flagId ?? string.Empty);
    }
}