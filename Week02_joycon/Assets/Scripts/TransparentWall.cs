using UnityEngine;

public class TransparentWall : MonoBehaviour
{// 1. ����Ƽ �����Ϳ��� �������ϰ� ���� ���� ������ ����
    [Tooltip("�����ϰ� ���� ���� Sprite Renderer�� ���⿡ �����ϼ���.")]
    public SpriteRenderer wallSpriteRenderer;

    // 2. �󸶳� �����ϰ� ������ ���� (0 = ���� ����, 1 = ���� ������)
    [Tooltip("���� �󸶳� ���������� �����մϴ�. (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float transparencyLevel = 0.5f;

    private Color originalColor; // ���� ���� ������ ������ ����

    void Start()
    {
        // 3. ��ũ��Ʈ�� ���۵� ��, ���� ���� ������ ������ �Ӵϴ�.
        if (wallSpriteRenderer != null)
        {
            originalColor = wallSpriteRenderer.color;
        }
        else
        {
            Debug.LogError("Wall Sprite Renderer�� ������� �ʾҽ��ϴ�! �ν����� â���� �������ּ���.");
        }
    }

    // 4. Ʈ���� �ȿ� ���� ������ �� ȣ��Ǵ� �Լ�
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���� ������Ʈ�� �±�(Tag)�� "Player"���� Ȯ���մϴ�.
        if (other.CompareTag("Player"))
        {
            // ���ο� ������ ������ ����ϴ�.
            Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, transparencyLevel);
            // ���� ������ �����ϰ� �����մϴ�.
            wallSpriteRenderer.color = newColor;
        }
    }

    // 5. Ʈ���ſ��� ���� ������ �� ȣ��Ǵ� �Լ�
    private void OnTriggerExit2D(Collider2D other)
    {
        // ���� ������Ʈ�� �±�(Tag)�� "Player"���� Ȯ���մϴ�.
        if (other.CompareTag("Player"))
        {
            // ���� ������ ������� �ǵ����ϴ�.
            wallSpriteRenderer.color = originalColor;
        }
    }
}
