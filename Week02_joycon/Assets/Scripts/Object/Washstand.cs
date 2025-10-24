using UnityEngine;

public class Washstand : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        if (InventoryManager.Instance.HasItem(ItemName.Rag))
        {
            Debug.Log("Used Rag at Washstand");
            // Additional logic for using the rag at the washstand can be added here
        }
        else
        {
            Debug.Log("You need a Rag to use the Washstand.");
        }
    }
}