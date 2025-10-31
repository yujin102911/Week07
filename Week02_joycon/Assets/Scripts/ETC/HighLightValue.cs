using UnityEngine;

public class HighLightValue : MonoBehaviour
{
    [SerializeField] float highlightIntensity = 1.0f;
    [SerializeField] string thicknessPropertyName = "_OutlineThickness";
    enum ColorValue { Red, Green, Blue };
    SpriteRenderer renderer;
    MaterialPropertyBlock block;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        block = new MaterialPropertyBlock();
    }

    // Update is called once per frame
    void Update()
    {
        if (renderer == null)
            return;

        if (block == null)
            block = new MaterialPropertyBlock();

        renderer.GetPropertyBlock(block);
        int thicknessId = Shader.PropertyToID(thicknessPropertyName);
        block.SetFloat(thicknessId, highlightIntensity);
        renderer.SetPropertyBlock(block);

    }
}
