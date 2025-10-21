using UnityEngine;

[DisallowMultipleComponent]
public sealed class StageSaveable : MonoBehaviour
{
    [Header("Stage / Key")]
    [SerializeField, Range(0, StageManager.MaxStages - 1)]
    private int stageIndex = 0;

    [SerializeField] private string objectKey = "Root#001";

    [Header("Options")]
    [SerializeField] private bool useLocal = false;     // ���� ��ǥ�� ����/����
    [SerializeField] private bool includeRoot = true;   // ��Ʈ �ڽ� ���� ����

    public int StageIndex => stageIndex;
    public string ObjectKey => objectKey;
    public bool UseLocal => useLocal;
    public bool IncludeRoot => includeRoot;

    void OnEnable() { if (StageManager.Instance) StageManager.Instance.Register(this); }
    void OnDisable() { if (StageManager.Instance) StageManager.Instance.Unregister(this); }

    // ------ Save ------
    public void SaveHierarchyWithCurrentStage()
    {
        var mgr = StageManager.Instance;
        if (!mgr) return;
        mgr.SaveHierarchy(mgr.CurrentStage, this, useLocal, includeRoot);
    }

    public void SaveHierarchyMyStage()
    {
        var mgr = StageManager.Instance;
        if (!mgr) return;
        mgr.SaveHierarchy(stageIndex, this, useLocal, includeRoot);
    }

    // ------ Restore (�籸��) ------
    // �⺻: ���� �ڽ� ���� ���� ��, ����� Ʈ���� �ٽ� ����
    public void RestoreHierarchyWithCurrentStage()
    {
        var mgr = StageManager.Instance;
        if (!mgr) return;
        mgr.RebuildHierarchy(mgr.CurrentStage, this, useLocal, includeRoot, factory: null);
    }

    public void RestoreHierarchyMyStage()
    {
        var mgr = StageManager.Instance;
        if (!mgr) return;
        mgr.RebuildHierarchy(stageIndex, this, useLocal, includeRoot, factory: null);
    }

    // �ʿ� ��, ������ ���丮 ���� ����(��: ��κ� ����)
    public void RestoreWithFactory(System.Func<string, Transform, GameObject> factory)
    {
        var mgr = StageManager.Instance;
        if (!mgr) return;
        mgr.RebuildHierarchy(mgr.CurrentStage, this, useLocal, includeRoot, factory);
    }
}
