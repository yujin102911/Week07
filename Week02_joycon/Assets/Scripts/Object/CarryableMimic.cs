using System.Collections.Generic;
using UnityEngine;

public class CarryableMimic : Carryable
{
    [SerializeField] private int requiredCoins;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite cleanedSprite;
    [SerializeField] private MimicBubble coinBubble;
    [SerializeField] private MimicBubble heartBubble;
    [SerializeField] private MimicBubble cleanBubble;
    private HashSet<Carryable> coins = new();
    private List<Carryable> toRemove = new();
    private bool isEnumerating;
    private bool isCleaned = false;

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

    private void EatCoin(Carryable coin)
    {
        requiredCoins--;
        heartBubble.SetOn();
        if (coin) Destroy(coin.gameObject);

        // All coins collected
        if (requiredCoins == 0) QuestRuntime.Instance.SetFlag(FlagId.Mimic_Happy);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Carryable coin))
        {
            if (coin.GetItemName() != ItemName.Coin) return;
            coins.Add(coin);
        }

        if (collision.CompareTag("Player"))
        {
            if (isCleaned == false) cleanBubble.SetOn();
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
        isCleaned = true;
    }
}