using System.Collections.Generic;
using UnityEngine;

public class Stove : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<Sprite> firewoodSprites;
    [SerializeField] private Transform potSnapPoint;
    private int currentFirewood = 0;
    public bool isFireOn => currentFirewood == firewoodSprites.Count - 1;
    private Pot potOnStove = null;

    private void Start()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    public bool Interact() => AddFirewood();

    private bool AddFirewood()
    {
        if (isFireOn == true) return false;
        if (InventoryManager.Instance.HasItem(ItemName.Firewood) == false) return false;

        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.Firewood);

        currentFirewood++;
        GameLogger.Instance.LogDebug(this, $"장작 추가, 현재: {currentFirewood}개");

        UpdateSprite();
        if (isFireOn == true && potOnStove != null)
            potOnStove.CheckCookingConditions();

        return true;
    }

    public void ResetStove()
    {
        currentFirewood = 0;
        UpdateSprite();
        GameLogger.Instance.LogDebug(this, "아궁이 초기화 완료!");
    }

    private void UpdateSprite() => spriteRenderer.sprite = firewoodSprites[currentFirewood];

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Pot>(out Pot pot))
        {
            if (pot.GetComponent<Carryable>() != null && !pot.GetComponent<Carryable>().carrying)
            {
                potOnStove = pot;
                pot.SetCurrentStove(this);
                GameLogger.Instance.LogDebug(this, "���� ����� ���� �������ϴ�.");

                pot.transform.position = potSnapPoint.position;
                pot.transform.localScale = Vector2.one * 1.2f;
                pot.transform.rotation = Quaternion.identity;
                if (pot.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }

                pot.CheckCookingConditions();
            }
        }
        else if (collision.TryGetComponent(out Carryable carryable))
        {
            if (carryable.Id == "Firewood" && !carryable.carrying)
            {
                AddFirewood();
                Destroy(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<Pot>(out Pot pot) && pot == potOnStove)
        {
            potOnStove = null;
            pot.SetCurrentStove(null); // ���� ����꿡�� ���
            GameLogger.Instance.LogDebug(this, "���� ����꿡�� ������ϴ�.");
        }
    }
}