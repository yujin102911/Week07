using Game.Quests;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Minimal 2D interaction scanner:
/// - Picks nearest Interactable2D within radius
/// - Shows prompt
/// - On press, raises QuestEvents.RaiseInteract with InteractionKind.Press
/// - Filters by required flags only (items/hold/stay removed)
/// </summary>
[DisallowMultipleComponent]
public sealed class InteractionScanner2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform player;
    [SerializeField] InteractWorldPrompt promptUI;

    [Header("Probe")]
    [SerializeField] LayerMask interactableMask;
    [SerializeField, Min(0.2f)] float radius = 1.8f;

    [Header("Input (New Input System)")]
    [SerializeField] private InputActionReference interactAction; // Button
    [SerializeField] private InputActionReference moveAction;     // Vector2 (optional)
    [SerializeField] private bool autoEnableActions = true;

    InputAction _interact;
    InputAction _move;

    // input buffer (press just before target disappears)
    [Header("Buffer")]
    [SerializeField, Range(0.02f, 0.25f)] float inputBuffer = 0.12f;
    [SerializeField, Range(0.02f, 0.25f)] float coyoteTime = 0.12f;

    [Header("Scan")]
    [SerializeField, Range(0.02f, 0.2f)] float idleScanInterval = 0.06f;

    static readonly Collider2D[] s_Hits = new Collider2D[16];
    ContactFilter2D _filter;

    bool _interactPressedThisFrame;
    float _lastPressAt = -999f;
    float _lastTargetSeenAt = -999f;

    // prompt/scan cache
    Interactable2D _focus;
    float _scanAcc;

    void Awake()
    {
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(interactableMask);
        _filter.useTriggers = true;

        _interact = interactAction ? interactAction.action : null;
        _move = moveAction ? moveAction.action : null;
    }

    void OnEnable()
    {
        if (autoEnableActions)
        {
            _interact?.Enable();
            _move?.Enable();
        }
        if (_interact != null)
            _interact.performed += OnInteractPerformed;
    }

    void OnDisable()
    {
        if (_interact != null)
            _interact.performed -= OnInteractPerformed;

        if (autoEnableActions)
        {
            _interact?.Disable();
            _move?.Disable();
        }
    }

    void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        _interactPressedThisFrame = true;
        _lastPressAt = Time.time;
    }

    bool ConsumePressedThisFrame()
    {
        bool p = _interactPressedThisFrame;
        _interactPressedThisFrame = false;
        return p;
    }

    void Update()
    {
        if (!player) return;

        bool pressed = ConsumePressedThisFrame();

        // low-frequency scan for prompt
        _scanAcc += Time.deltaTime;
        if (_scanAcc >= idleScanInterval || pressed)
        {
            _scanAcc = 0f;
            _focus = FindBestTarget((Vector2)player.position);

            if (_focus)
            {
                _lastTargetSeenAt = Time.time;
                promptUI?.Show(_focus.PromptAnchor, _focus.PromptOffset, _focus.PromptText, showProgress: false);
            }
            else
            {
                promptUI?.Hide();
            }
        }

        // input buffer + coyote
        bool bufferedPressOk =
            (Time.time - _lastPressAt) <= inputBuffer &&
            (Time.time - _lastTargetSeenAt) <= coyoteTime;

        if (!bufferedPressOk || !_focus) return;

        // preconditions
        if (!_focus.HasRequiredFlags(QuestFlags.Has)) return;

        // optional auto-destroy behavior kept for legacy compatibility
        if (_focus.ActiveToDestory == true)
        {
            Debug.Log($"ActiveToDestory {_focus.transform.name}");
            Destroy(_focus.gameObject);
        }

        // only Press kind is handled in the slim quest system
        if (pressed && _focus.Kind == InteractionKind.Press)
        {
            QuestEvents.RaiseInteract(_focus.Id, _focus.transform.position, InteractionKind.Press);
            GameLogger.Instance.LogDebug(this, $"Interaction {_focus.transform.name}");
        }
    }

    // choose nearest valid target
    Interactable2D FindBestTarget(Vector2 pos)
    {
        int count = Physics2D.OverlapCircle(pos, radius, _filter, s_Hits);
        if (count == 0) return null;

        float bestD2 = float.MaxValue;
        Interactable2D best = null;

        for (int i = 0; i < count; ++i)
        {
            var c = s_Hits[i];
            s_Hits[i] = null;
            if (!c || !c.TryGetComponent(out Interactable2D it)) continue;

            // flags gate (items are removed from slim system)
            if (!it.HasRequiredFlags(QuestFlags.Has)) continue;

            // ignore carryables currently being carried (legacy behavior)
            var cb = it.GetComponent<Carryable>();
            if (cb != null && cb.carrying) continue;

            // only consider press interactions in this slim scanner
            if (it.Kind != InteractionKind.Press) continue;

            float d2 = ((Vector2)it.transform.position - pos).sqrMagnitude;
            if (d2 < bestD2) { bestD2 = d2; best = it; }
        }
        return best;
    }
}