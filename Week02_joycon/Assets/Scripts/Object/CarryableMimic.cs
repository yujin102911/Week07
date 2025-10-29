using System.Collections.Generic;
using UnityEngine;

public class CarryableMimic : Carryable, IInteractable
{
    [SerializeField] private int requiredCoins = 4;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite cleanedSprite;
    [SerializeField] private SpriteRenderer coinSpriteRenderer;
    [SerializeField] private List<Sprite> coinSprites;
    [SerializeField] private MimicBubble coinBubble;
    [SerializeField] private MimicBubble heartBubble;
    [SerializeField] private MimicBubble cleanBubble;
    [SerializeField] private GameObject bubbles;
    private HashSet<Carryable> coins = new();
    private List<Carryable> toRemove = new();
    private bool isEnumerating;
    private bool isCleaned = false;
    private bool addShampoo = false;

    private void Update()
    {
        if (coins.Count == 0) return;
        if (carrying == true) return;

        isEnumerating = true;
        toRemove.Clear();

        foreach (var coin in coins)
        {
            if (coin == false)
            {
                toRemove.Add(coin);
                continue;
            }

            if (coin.carrying == true) continue;

            EatCoin(coin);
            toRemove.Add(coin);
        }

        isEnumerating = false;

        if (toRemove.Count > 0)
        {
            foreach (var coin in toRemove) coins.Remove(coin);
            toRemove.Clear();
        }
    }

    private void UpdateCoinSprite()
    {
        coinSpriteRenderer.sprite = coinSprites[requiredCoins];
    }

    private bool EatCoin(Carryable coin)
    {
        if (requiredCoins <= 0) return false;

        requiredCoins--;
        heartBubble.SetOn();
        if (coin) Destroy(coin.gameObject);

        UpdateCoinSprite();
        CheckQuest();
        return true;
    }

    private bool EatCoin()
    {
        if (requiredCoins <= 0) return false;
        if (InventoryManager.Instance.HasItem(ItemName.Coin) == false) return false;

        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.Coin);
        requiredCoins--;
        heartBubble.SetOn();

        UpdateCoinSprite();
        CheckQuest();
        return true;
    }

    public bool TryInteract()
    {
        var coin = EatCoin();
        if (coin == false) return ShampooInteract();
        return coin;
    }

    private bool ShampooInteract()
    {
        if (isCleaned == true) return false;
        if (InventoryManager.Instance.HasItem(ItemName.Shampoo) == false) return false;

        addShampoo = true;
        InventoryManager.Instance.RemoveAndDestroyItem(ItemName.Shampoo);
        bubbles.SetActive(true);
        return false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (requiredCoins <= 0) return;

        if (collision.TryGetComponent(out Carryable carryable))
        {
            if (carryable.GetItemName() == ItemName.Coin) coins.Add(carryable);
            else if (carryable.GetItemName() == ItemName.Shampoo && carryable.carrying == false)
            {
                addShampoo = true;
                bubbles.SetActive(true);
                Destroy(collision.gameObject);
            }
            else return;
        }

        if (collision.CompareTag("Player"))
        {
            if (isCleaned == false)
            {
                if (addShampoo == false) cleanBubble.SetOn();
            }
            else if (requiredCoins > 0) coinBubble.SetOn();
            else heartBubble.SetOn();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (requiredCoins <= 0) return;

        if (collision.TryGetComponent(out Carryable coin))
        {
            if (coin.GetItemName() != ItemName.Coin) return;

            if (isEnumerating == false) coins.Remove(coin);
            else if (toRemove.Contains(coin) == false) toRemove.Add(coin);
        }
    }

    private void OnDisable()
    {
        coins.Clear();
        toRemove.Clear();

        isEnumerating = false;
    }

    public void CleanUp()
    {
        spriteRenderer.sprite = cleanedSprite;
        enabled = true;
        isCleaned = true;

        CheckQuest();
    }

    private void CheckQuest()
    {
        if (requiredCoins > 0) return;
        if (isCleaned == false) return;

        QuestRuntime.Instance.SetFlag(FlagId.ManagingMimic);
        GameLogger.Instance.LogDebug(this, "미믹 퀘스트 완료");
    }
}