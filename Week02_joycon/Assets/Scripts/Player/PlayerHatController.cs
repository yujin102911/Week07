using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HatData
{
    public HatType hatType;
    public Sprite hatSprite;
    public GameObject hatPrefab;
}

public class PlayerHatController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer hatSpriteRenderer;
    [SerializeField] private HatType currentHatType;
    [SerializeField] private List<HatData> hatDatas;

    public void ChangeHat(HatType hatType)
    {
        Vector2 dropPosition = (Vector2)transform.position + new Vector2(Player.GetFaceDir() * -1.0f, 0.5f);
        Instantiate(GetHatData(currentHatType).hatPrefab, dropPosition, Quaternion.identity);

        currentHatType = hatType;
        hatSpriteRenderer.sprite = GetHatData(hatType).hatSprite;
    }

    private HatData GetHatData(HatType hatType) => hatDatas.Find(h => h.hatType == hatType);
}