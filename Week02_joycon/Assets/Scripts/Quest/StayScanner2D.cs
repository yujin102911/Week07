using System.Collections.Generic;
using UnityEngine;
using Game.Quests; // QuestRuntime, QuestFlags

/// <summary>
/// Slim StayScanner2D:
/// - Tracks Carryable objects with matching ScannerID inside this trigger.
/// - When at least `requiredCount` eligible objects stay for `requiredStaySeconds`, sets `flagToSet` once.
/// - No Enter/Exit interact events, no cancel/undo. Flags only.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class StayScanner2D : MonoBehaviour
{
    [Header("Match")]
    [SerializeField] private uint scannerId = 1;     // Carryable.ScannerID to accept
    [SerializeField] private LayerMask actorMask = ~0;
    [SerializeField] private bool excludeCarried = true;

    [Header("Goal")]
    [SerializeField] private int requiredCount = 1;  // how many eligible objects must be inside
    [SerializeField] private float requiredStaySeconds = 0.8f;
    [SerializeField] private string flagToSet = "Boxes.StoredAll";

    [Header("Policy")]
    [SerializeField] private bool oneShot = false; // dynamic by default
    [SerializeField] private float cooldown = 0f;

    [Header("Boot")]
    [SerializeField] private bool scanOnEnable = true;

    // ─────────────────────────────────────────────────────────────────────
    private Collider2D _trigger;
    private ContactFilter2D _filter;
    private float _lastFireAt = -999f;
    private bool _firedOnce;
    private bool _flagActive;

    // Tracking
    private readonly Dictionary<Carryable, float> _insideSince = new(64);
    private readonly Dictionary<Collider2D, Carryable> _col2Carry = new(128);
    private static readonly Collider2D[] _hits = new Collider2D[64];

    void Awake()
    {
        _trigger = GetComponent<Collider2D>();
        _trigger.isTrigger = true;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;
        rb.simulated = true;

        _filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = true,
            layerMask = actorMask
        };
    }

    void OnEnable()
    {
        _firedOnce = false;

        if (!scanOnEnable || _trigger == null) return;

#if UNITY_6000_0_OR_NEWER
        int count = _trigger.Overlap(_filter, _hits);
#else
        int count = _trigger.OverlapCollider(_filter, _hits);
#endif
        for (int i = 0; i < count; ++i)
        {
            var c = _hits[i];
            _hits[i] = null;
            if (!c) continue;

            var carry = ResolveCarryable(c);
            if (!IsEligible(carry)) continue;

            _col2Carry[c] = carry;
            if (!_insideSince.ContainsKey(carry))
                _insideSince.Add(carry, Time.time);
        }
    }

    void Update()
    {
        if (oneShot && _firedOnce) return;
        // we allow zero count to clear the flag below

        // purge ineligible (carried, id mismatch, destroyed)
        _toRemove.Clear();
        foreach (var kv in _insideSince)
        {
            var carry = kv.Key;
            if (!IsEligible(carry)) _toRemove.Add(carry);
        }
        for (int i = 0; i < _toRemove.Count; ++i) _insideSince.Remove(_toRemove[i]);

        if (_insideSince.Count < Mathf.Max(1, requiredCount)) return;

        float now = Time.time;
        bool allowFireNow = (now - _lastFireAt) >= cooldown;
        if (!allowFireNow) return;

        // Check all eligible have stayed long enough (we only need requiredCount of them).
        int ok = 0;
        foreach (var kv in _insideSince)
        {
            if (now - kv.Value >= requiredStaySeconds) ok++;
            if (ok >= requiredCount) break;
        }

        bool meets = ok >= requiredCount;
        if (!string.IsNullOrEmpty(flagToSet))
        {
            if (meets && !_flagActive)
            {
                QuestRuntime.Instance.SetFlag(flagToSet);
                _flagActive = true;
                _lastFireAt = now;
                _firedOnce = true;
            }
            else if (!meets && _flagActive && !oneShot)
            {
                QuestRuntime.Instance.ClearFlag(flagToSet);
                _flagActive = false;
                _lastFireAt = now;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & actorMask.value) == 0) return;

        var carry = ResolveCarryable(other);
        if (!IsEligible(carry)) return;

        _col2Carry[other] = carry;
        if (!_insideSince.ContainsKey(carry))
            _insideSince.Add(carry, Time.time);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!_col2Carry.TryGetValue(other, out var carry))
        {
            carry = ResolveCarryable(other);
            if (!IsEligible(carry)) return;
            _col2Carry[other] = carry;
        }

        if (!_insideSince.ContainsKey(carry))
            _insideSince.Add(carry, Time.time);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        _col2Carry.Remove(other);

        var carry = ResolveCarryable(other);
        if (!carry) return;
        _insideSince.Remove(carry);
    }

    private static readonly List<Carryable> _toRemove = new(32);

    private bool IsEligible(Carryable c)
    {
        if (!c) return false;
        if (excludeCarried && c.carrying) return false;
        if (c.ScannerID != scannerId) return false;
        return true;
    }

    private static Carryable ResolveCarryable(Collider2D col)
    {
        if (!col) return null;
        if (col.TryGetComponent(out Carryable c)) return c;
        return col.GetComponentInParent<Carryable>();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (TryGetComponent(out Collider2D col)) col.isTrigger = true;
        if (TryGetComponent(out Rigidbody2D rb))
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.freezeRotation = true;
        }
        requiredCount = Mathf.Max(1, requiredCount);
        requiredStaySeconds = Mathf.Max(0.01f, requiredStaySeconds);
    }
#endif
}