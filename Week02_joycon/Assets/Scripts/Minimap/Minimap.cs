using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Camera))]
public class Minimap : MonoBehaviour
{
    [Header("Follow")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    [Range(0f, 1f)] public float followLerp = 0.25f;

    [Header("View")]
    public float orthoSize = 18f; // ���� �������� �� Ȯ��
    public bool northUp = true;   // true: ȸ�� ����, false: �÷��̾� ȸ���� �Բ�
    public bool rotateWithPlayer = false;

    [Header("Bounds (what minimap can show)")]
    public string[] includeLayers = { "MinimapTerrain" }; // ��� ���� ���̾��
    public float paddingWorld = 2f;                       // ��迡 ����

    private Camera _cam;
    private Bounds _mapBounds;
    private bool _hasBounds;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
        _cam.orthographicSize = orthoSize;
    }

    void Start()
    {
        RefreshBounds();
    }

    public void RefreshBounds()
    {
        int mask = 0;
        foreach (var ln in includeLayers)
            mask |= 1 << LayerMask.NameToLayer(ln);

        var rs = FindObjectsOfType<Renderer>()
            .Where(r => r.enabled && ((1 << r.gameObject.layer) & mask) != 0)
            .ToArray();

        if (rs.Length == 0)
        {
            _hasBounds = false;
            Debug.LogWarning("Minimap: no renderers found for bounds. Check includeLayers.");
            return;
        }

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);
        b.Expand(paddingWorld * 2f);
        _mapBounds = b;
        _hasBounds = true;
    }

    void LateUpdate()
    {
        if (!target) return;

        // ȸ��
        if (northUp) transform.rotation = Quaternion.identity;
        else if (rotateWithPlayer) transform.rotation = Quaternion.Euler(0f, 0f, target.eulerAngles.z);

        // ���ϴ� ��ġ(�÷��̾� ����)
        Vector3 desired = target.position + offset;

        // �� ��� ������ ī�޶� ��ġ Ŭ����
        if (_hasBounds)
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            // ���� ī�޶󺸴� ���� �� ���: �߾� ����
            float minX = _mapBounds.min.x + halfW;
            float maxX = _mapBounds.max.x - halfW;
            float minY = _mapBounds.min.y + halfH;
            float maxY = _mapBounds.max.y - halfH;

            if (minX > maxX) desired.x = _mapBounds.center.x;
            else desired.x = Mathf.Clamp(desired.x, minX, maxX);

            if (minY > maxY) desired.y = _mapBounds.center.y;
            else desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        // �ε巴�� �̵�
        transform.position = Vector3.Lerp(transform.position, desired, followLerp);
    }

    // ��Ÿ�� Ȯ��/���
    public void SetZoom(float size)
    {
        orthoSize = Mathf.Max(0.1f, size);
        _cam.orthographicSize = orthoSize;
    }
}
