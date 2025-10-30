using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    enum InteractionType { None, Carryable, Interactible}

    MaterialPropertyBlock block;
    [SerializeField] InteractionType type;
    [SerializeField] SpriteRenderer renderer;
    [SerializeField] float thickness = 0.02f;//만들어 두긴 했는데 쓰지 마셈
    [SerializeField] public bool isHighlighted = false;
    public Color color = Color.black;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (renderer == null)   renderer = GetComponent<SpriteRenderer>();
        block = new MaterialPropertyBlock();

    }

    // Update is called once per frame
    void Update()
    {
        block.SetFloat("_Thickness", thickness);
        block.SetColor("_Color", color);
        renderer.SetPropertyBlock(block);
    }
    public void SwitchColor()
    {
        switch (type)
        {
            case InteractionType.Carryable:
                color = Color.green;
                break;
            case InteractionType.Interactible:
                color = Color.blue;
                break;
            default:
                color = Color.black;
                break;
        }
    }
    

}
