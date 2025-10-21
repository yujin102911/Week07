// Assets/Scripts/Spawn/SpawnableSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Spawn/Spawnable Object")]
public sealed class SpawnableSO : ScriptableObject
{
    [Header("Prefab")]
    public GameObject prefab;

    [Header("Weight / Prewarm")]
    [Min(0f)] public float weight = 1f;   // ����ġ(���� ���ÿ�)
    [Min(0)] public int prewarm = 0;     // Ǯ �̸� ä�� ����

    [Header("Placement")]
    public Vector2 localOffset = Vector2.zero;  // ���� �� �߰� ������(���� ����)
    public float rotation = 0;  // ȸ�� ��ȯ
    public bool alignToHitNormal = true;     // �浹 �� ������ ȸ�� ��������
    [Range(0f, 0.5f)] public float surfaceOffset = 0.02f; // ǥ�鿡�� ����
}
