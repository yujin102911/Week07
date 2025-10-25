#nullable enable
using System.Runtime.CompilerServices;

namespace Game.Quests
{
    public static class QuestFlags
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Has(FlagId flag) => QuestRuntime.Instance.HasFlag(flag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Set(FlagId flag) => QuestRuntime.Instance.SetFlag(flag);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Clear(FlagId flag) => QuestRuntime.Instance.ClearFlag(flag);
    }
}