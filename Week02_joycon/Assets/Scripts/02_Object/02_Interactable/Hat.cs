using UnityEngine;

public enum HatType
{
    None,
    Crown,
    Cone,
    JOL,
    King,
    Witch,
}

public class Hat : MonoBehaviour, IInteractable
{
    [SerializeField] private HatType hatType;

    public bool TryInteract()
    {
        Player.ChangeHat(hatType);
        Destroy(gameObject);
        return true;
    }
}