using UnityEngine;

public class CarryableMimic : Carryable
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Carryable carryable))
        {
            if (carryable.GetItemName() != ItemName.Coin) return;
            if (carryable.carrying == true) return;

            // TODO: 행복도?
            Destroy(carryable.gameObject);
        }
    }
}