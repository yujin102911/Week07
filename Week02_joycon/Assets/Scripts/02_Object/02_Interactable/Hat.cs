using UnityEngine;

public enum HatType
{
    None,
    Crown,
    Cone,
    JOL,
    King,
    Witch,
    Cloud,
    Candy,
    Cooker,
    Santa,
    Clown
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