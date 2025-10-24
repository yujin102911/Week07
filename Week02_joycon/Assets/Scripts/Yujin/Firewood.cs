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
            GameLogger.Instance.LogDebug(this, $"���� ī��Ʈ: {count}");
            return;
        }
        GameLogger.Instance.LogDebug(this, $"���� ī��Ʈ: {count}");
        if (spriteRenderer == null) return;
        GameLogger.Instance.LogDebug(this, "�� ����!!");
        spriteRenderer.color = targetColor;

    }

}
