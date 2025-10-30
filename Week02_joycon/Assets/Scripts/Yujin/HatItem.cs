using UnityEngine;

public class HatItem : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite hatSprite;
    [SerializeField] private GameObject hatPrefab;

    public bool TryInteract()
    {
        Player.ChangeHat(hatSprite, hatPrefab);

        Destroy(gameObject);
        return true;
    }
}