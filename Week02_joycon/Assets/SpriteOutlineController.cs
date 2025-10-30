using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutlineController : MonoBehaviour
{
    public Color outlineColor = Color.black;
    [Range(0f, 10f)] public float outlineSize = 1f;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }

    void LateUpdate()
    {
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_OutlineColor", outlineColor);
        propertyBlock.SetFloat("_OutlineSize", outlineSize);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}
