using UnityEngine;

public sealed class SaveRuntime : MonoBehaviour
{
    [SerializeField] private CheckpointSaveSO sourceAsset;
    public static CheckpointSaveSO Current { get; private set; }

    void Awake()
    {
        if (Current != null) { Destroy(gameObject); return; }

        Current = Instantiate(sourceAsset);
        Current.name = sourceAsset ? sourceAsset.name + " (Runtime)" : "Checkpoint(Runtime)";
        Current.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
    }
}