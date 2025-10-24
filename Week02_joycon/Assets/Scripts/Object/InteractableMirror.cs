using System.Collections.Generic;
using UnityEngine;

public class InteractableMirror : MonoBehaviour, IInteractable
{
    [SerializeField] private List<GameObject> dirtys;

    public bool Interact()
    {
        // if (InventoryManager.Instance.HasItem(ItemName.Rag) == false) return;

        return CleanUp();
    }

    public bool CleanUp()
    {
        if (dirtys.Count == 0) return false;

        var randIndex = Random.Range(0, dirtys.Count);
        dirtys[randIndex].SetActive(false);
        dirtys.RemoveAt(randIndex);

        return true;
    }

    void IInteractable.Interact()
    {
        throw new System.NotImplementedException();
    }
}