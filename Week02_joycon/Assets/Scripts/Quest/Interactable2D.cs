using UnityEngine;
using Game.Quests; // InteractableId, FlagId

public enum InteractionKind : byte
{
    None,
    Press,      // 키 눌렀을 때
    Hold,       // 일정 시간 홀드
    EnterArea,  // 트리거 진입형(스캐너 없이 트리거가 Raise)
    UseItem     // (아이템 시스템 쓰면 추후 enum화해서 붙일 것)
}

[DisallowMultipleComponent]
public sealed class Interactable2D : MonoBehaviour
{
    [Header("ID / Type")]
    [SerializeField] private InteractableId idEnum = InteractableId.Table;
    [SerializeField] private InteractionKind kind = InteractionKind.Press;

    [Header("Hold Option")]
    [SerializeField, Min(0f)] private float requiredHoldSeconds = 0.8f; // 0이면 무시
    [SerializeField] private bool cancelOnMove = false;

    [Header("Stay Option")]
    [SerializeField, Min(0f)] private float requiredStaySeconds = 0.8f; // 0이면 무시

    [Header("Prompt (UI only)")]
    [SerializeField] private string promptText = "상호작용"; // UI 표시용 텍스트만 문자열 허용
    [SerializeField] private Transform promptAnchor;         // 없으면 this
    [SerializeField] private Vector3 promptOffset = new Vector3(0, 1f, 0);

    [Header("Preconditions (Flags)")]
    [SerializeField] private FlagId[] requiredEventFlags;    // enum만

    // 런타임용 프로퍼티
    public InteractableId IdEnum => idEnum;
    public InteractionKind Kind => kind;
    public float RequiredHoldSeconds => requiredHoldSeconds;
    public float RequiredStaySeconds => requiredStaySeconds;
    public bool CancelOnMove => cancelOnMove;

    public bool ActiveToDestory = false;

    public Transform PromptAnchor => promptAnchor ? promptAnchor : transform;
    public Vector3 PromptOffset => promptOffset;
    public string PromptText => promptText;

    /// <summary>필요 플래그 충족 여부. enum 기반 델리게이트만 받음.</summary>
    public bool HasRequiredFlags(System.Func<FlagId, bool> flagChecker)
    {
        if (requiredEventFlags == null || requiredEventFlags.Length == 0) return true;
        if (flagChecker == null) return false;
        for (int i = 0; i < requiredEventFlags.Length; ++i)
            if (!flagChecker(requiredEventFlags[i])) return false;
        return true;
    }
}