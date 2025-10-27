using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class AreaZoneDetector : MonoBehaviour
{
    public AreaNameUI areaUI;
    public string defaultName = "�Ÿ�";

    [Header("Filter")]
    [SerializeField] private LayerMask zoneMask;

    QuestTodoUI questTodoUI;

    // --- Dictionary ���� ---
    private static readonly Dictionary<string, uint> ZoneToQuestId = new()
    {
        { "����", 1000 },
        { "�Ĵ�", 2000 },
        { "ȭ���", 2000 },
        { "�Ʒü�", 2000 },
        { "�����", 3000 },
        { "����â��", 3000 },
        { "�׽�Ʈ", 9000 },
        // �ʿ��ϸ� ��� �߰�
    };

    void Awake()
    {
        if (areaUI == null)
            Debug.LogWarning("[AreaZoneDetector] areaUI�� ������ϴ�.", this);

        questTodoUI = GameObject.FindAnyObjectByType<QuestTodoUI>();
    }

    void Start()
    {
        if (areaUI != null)
            areaUI.SetAreaName(defaultName);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (zoneMask.value != 0 && (zoneMask.value & (1 << other.gameObject.layer)) == 0)
            return;

        var zone = other.GetComponentInParent<AreaZone>();
        if (zone == null) return;

        // UI ����
        areaUI?.SetAreaName(zone.displayName);
    }
}
