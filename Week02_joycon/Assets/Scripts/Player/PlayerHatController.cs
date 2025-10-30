using UnityEngine;

public class PlayerHatController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer hatSpriteRenderer;
    [SerializeField] private GameObject currentHatPrefab;

    public void ChangeHat(Sprite newHatSprite, GameObject hatPrefab)
    {
        Vector2 dropPosition = (Vector2)transform.position + new Vector2(Player.GetFaceDir() * -1.0f, 0.5f);
        Instantiate(currentHatPrefab, dropPosition, Quaternion.identity);

        hatSpriteRenderer.sprite = newHatSprite;
        currentHatPrefab = hatPrefab;
    }
}