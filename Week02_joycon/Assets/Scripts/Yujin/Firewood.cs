using UnityEngine;

public class Firewood : MonoBehaviour
{
    [Header("Color Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color targetColor = Color.red;

    private Color currentColor;
    private int count = 0;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void TurnFire()
    {
        if (count < 4)
        {
            count++;
            GameLogger.Instance.LogDebug(this, $"지금 카운트: {count}");
            return;
        }
        GameLogger.Instance.LogDebug(this, $"지금 카운트: {count}");
        if (spriteRenderer == null) return;
        GameLogger.Instance.LogDebug(this, "불 켰음!!");
        spriteRenderer.color = targetColor;

    }

}
