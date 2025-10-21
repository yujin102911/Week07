// Assets/Scripts/Quests/QuestEvents.cs
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class QuestEvents
{
    // =========================
    // �޽���/�̺�Ʈ ���� (���� ����)
    // =========================

    // ---- ��ȣ�ۿ� �� �̺�Ʈ ----
    public struct InteractMsg
    {
        public string id;             // Interactable Id
        public int idHash;            // Animator.StringToHash(id)
        public Vector3 pos;           // ��ȣ�ۿ� ��ġ
        public InteractionKind kind;  // Press / Hold / UseItem
    }

    public static event Action<InteractMsg> OnInteract;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RaiseInteract(string id, Vector3 pos, InteractionKind kind)
    {
        var msg = new InteractMsg
        {
            id = id,
            idHash = Animator.StringToHash(id ?? string.Empty),
            pos = pos,
            kind = kind
        };

        OnInteract?.Invoke(msg);

        // ĳ��: ������ ��ȣ�ۿ� ����(��ҿ�)
        _lastInteractByHash[msg.idHash] = msg;
    }

    // ---- Hold UI/���� �̺�Ʈ ----
    public static event Action<string, float> OnHoldStarted;    // (id, requiredSeconds)
    public static event Action<string, float> OnHoldProgress;   // (id, elapsedSeconds)
    public static event Action<string> OnHoldCanceled;          // (id)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RaiseHoldStarted(string id, float requiredSeconds)
        => OnHoldStarted?.Invoke(id, requiredSeconds);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RaiseHoldProgress(string id, float elapsedSeconds)
        => OnHoldProgress?.Invoke(id, elapsedSeconds);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RaiseHoldCanceled(string id)
        => OnHoldCanceled?.Invoke(id);

    // ---- �÷���(TriggerFlags) ----
    public static event Action<string> OnFlagRaised;   // (flagId)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RaiseFlag(string flagId)
    {
        OnFlagRaised?.Invoke(flagId);
        _raisedFlags.Add(flagId); // ĳ��(��ҿ�)
    }

    // ---- StayInArea ----
    public static event Action<string, float, Vector3> OnAreaStayTick; // (areaId, deltaSeconds, pos)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RaiseAreaStayTick(string areaId, float deltaSeconds, Vector3 pos)
    {
        OnAreaStayTick?.Invoke(areaId, deltaSeconds, pos);
        _activeAreas[areaId] = pos; // ������ ��ġ�� ����(���/�����)
    }

    // ---- Delivery ----
    public static event Action<string, string, Vector3> OnDelivery;    // (itemId, receiverId, pos)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RaiseDelivery(string itemId, string receiverId, Vector3 pos)
    {
        OnDelivery?.Invoke(itemId, receiverId, pos);
        _lastDeliveryByReceiver[receiverId] = (itemId, pos); // ĳ��(��ҿ�)
    }

    // ======================================================
    // "���"�� ���� �߰� �̺�Ʈ & API (�ּ� ħ��, ������ ����)
    // ======================================================

    public static event Action<InteractMsg> OnInteractCanceled;

    /// <summary>
    /// �־��� ID�� ��ȣ�ۿ� ��� ��ȣ�� ��ε�ĳ��Ʈ�մϴ�.
    /// �Ŵ����� �� ��ȣ�� �޾� "���� ���¸� ��ĵ"�Ͽ� ��Ī�Ǵ� �Ϸ� ��ǥ�� �ѹ��մϴ�.
    /// </summary>
    public static bool CancelLastInteract(string id)
    {
        if (string.IsNullOrEmpty(id)) return false; // �� ID ����
        var msg = new InteractMsg
        {
            id = id,
            idHash = UnityEngine.Animator.StringToHash(id),
            pos = default,
            kind = InteractionKind.None
        };
        OnInteractCanceled?.Invoke(msg); // ĳ�� �˻� ���� ��� ��ε�ĳ��Ʈ
        return true;
    }



    // Hold ���(�̹� OnHoldCanceled�� �־� ���۸� ����)
    /// <summary>���� ���� Hold�� ���(�����ڴ� �� ��ȣ�� UI/���� �ѹ�).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CancelHold(string id)
    {
        OnHoldCanceled?.Invoke(id);
        return true; // ���� ���� Ʈ��ŷ ���� ��ȣ��
    }

    // �÷��� ���(����)
    public static event Action<string> OnFlagCleared; // (flagId)

    /// <summary>������ Raise�� Flag�� ����.</summary>
    public static bool CancelFlag(string flagId)
    {
        if (_raisedFlags.Remove(flagId))
        {
            OnFlagCleared?.Invoke(flagId);
            return true;
        }
        return false;
    }

    // AreaStay ���/����
    public static event Action<string> OnAreaStayCanceled; // (areaId)

    /// <summary>���������� ������ AreaStay�� ���� ��ȣ�� ���.</summary>
    public static bool CancelAreaStay(string areaId)
    {
        if (_activeAreas.Remove(areaId))
        {
            OnAreaStayCanceled?.Invoke(areaId);
            return true;
        }
        return false;
    }

    // ��� ���
    public static event Action<string, string, Vector3> OnDeliveryCanceled; // (itemId, receiverId, pos)

    /// <summary>receiver ���� ������ Delivery�� ���.</summary>
    public static bool CancelLastDeliveryTo(string receiverId)
    {
        if (_lastDeliveryByReceiver.TryGetValue(receiverId, out var data))
        {
            OnDeliveryCanceled?.Invoke(data.itemId, receiverId, data.pos);
            _lastDeliveryByReceiver.Remove(receiverId);
            return true;
        }
        return false;
    }

    // ��ƿ: ĳ�� �ʱ�ȭ
    public static void ClearAllCaches()
    {
        _lastInteractByHash.Clear();
        _raisedFlags.Clear();
        _activeAreas.Clear();
        _lastDeliveryByReceiver.Clear();
    }

    // =========================
    // ���� ĳ��(���ν����� ����)
    // =========================
    static readonly Dictionary<int, InteractMsg> _lastInteractByHash = new(128);
    static readonly HashSet<string> _raisedFlags = new();
    static readonly Dictionary<string, Vector3> _activeAreas = new();
    static readonly Dictionary<string, (string itemId, Vector3 pos)> _lastDeliveryByReceiver = new();
}
