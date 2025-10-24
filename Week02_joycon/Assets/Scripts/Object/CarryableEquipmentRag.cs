using System.Collections.Generic;
using UnityEngine;

public class CarryableEquipmentRag : CarryableEquipment
{
    [SerializeField] private bool canInteractAutomatically;
    [SerializeField] private List<Sprite> dirtyRagSprites;
    private Dictionary<int, Sprite> spriteDict;
    private int currentState;
    private InteractableMirror mirror;

    protected override void Start()
    {
        base.Start();

        itemName = ItemName.Rag;
        spriteDict = new();
        for (int i = 0; i < dirtyRagSprites.Count; i++)
            spriteDict[i] = dirtyRagSprites[i];
        currentState = 0;
    }

    public override bool UseItem(IInteractable mirror = null)
    {
        if (currentState == dirtyRagSprites.Count - 1) return false;
        if (canInteractAutomatically == true)
        {
            if (mirror == null) return false;
            if ((mirror as InteractableMirror).CleanUp() == false) return false;
        }

        SetCurrentState(currentState + 1);
        return true;
    }

    public void WashRag()
    {
        SetCurrentState(0);
    }

    private void SetCurrentState(int state)
    {
        if (state < 0 || state >= dirtyRagSprites.Count) return;

        currentState = state;
        GetComponent<SpriteRenderer>().sprite = spriteDict[currentState];
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.name);
        if (collision.TryGetComponent(out mirror) && canInteractAutomatically)
        {
            Debug.Log("Auto Clean Mirror");
            UseItem(mirror);
        }
    }
}