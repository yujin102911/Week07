using UnityEngine;

public class PlayerSpriteChanger : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite newLookSprite;

    public bool TryInteract()
    {
        Player playerScript = Player.Instance;
        ChangePlayerSpriteOnInteract(playerScript);
        return true;
    }

    private void ChangePlayerSpriteOnInteract(Player playerScript)
    {
        if (playerScript == null || newLookSprite == null) return;

        playerScript.ChangeSprite(newLookSprite);
        gameObject.SetActive(false);
        GameLogger.Instance.LogDebug(this, $"Player sprite changed to {newLookSprite.name}");
    }
}