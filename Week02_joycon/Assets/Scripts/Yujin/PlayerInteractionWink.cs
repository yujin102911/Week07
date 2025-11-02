using UnityEngine;
using System.Collections;

public class PlayerInteractionWink : MonoBehaviour, IInteractable
{
    [Header("Sprite")]
    [SerializeField] private Sprite expressionSprite;
    [SerializeField] private float pauseDuration = 2.0f;

    [Header("Options")]
    [SerializeField] private bool disableAfterUse = false; //
    private bool _isInteraction = false; //중복 방지

    public bool TryInteract()
    {
        if (_isInteraction)
        {
            return false;
        }
        Player player = Player.Instance;
        if (player == null)
        {
            GameLogger.Instance.LogError(this, "Player.Instance를 찾을 수 없음");
            return false;
        }
        if (expressionSprite == null)
        {
            GameLogger.Instance.LogError(this, "찡긋 Sprite가 없습");
            return false;
        }
        StartCoroutine(PauseAndWinkRoutine(player));
        return true;

    }

    private IEnumerator PauseAndWinkRoutine(Player player)
    {
        _isInteraction = true;
        player.velocity = Vector3.zero;
        player.enabled = false;
        Sprite originalSprite = player.CurrentPlayerSprite;
        player.ChangeSprite(expressionSprite);
        yield return new WaitForSeconds(pauseDuration);
        player.ChangeSprite(originalSprite);
        player.enabled=true;
        _isInteraction=false;
        if (disableAfterUse)
        {
            gameObject.SetActive(false);
        }
    }

}
