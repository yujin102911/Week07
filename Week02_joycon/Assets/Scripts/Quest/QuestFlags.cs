#nullable enable
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Game.Quests
{
    /// <summary>
    /// ���� ���� �ۻ��: ���� SSOT�� QuestRuntime���� ����.
    /// - ���� �ڵ忡�� static QuestState.HasFlag(...)�� ��ü
    /// - ����ȭ: AggressiveInlining
    /// </summary>
    public static class QuestFlags
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(string? flagId)
            => QuestRuntime.Instance.HasFlag(flagId ?? string.Empty);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(int flagHash)
            => QuestRuntime.Instance.HasFlag(flagHash);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set(string flagId)
            => QuestRuntime.Instance.SetFlag(flagId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Clear(string flagId)
            => QuestRuntime.Instance.ClearFlag(flagId);
    }
}
