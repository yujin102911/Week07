using UnityEngine;

public class CarryablePiggyBank : Carryable
{
    [SerializeField] private int coinsCount;
    [SerializeField] private GameObject coinPrefab;
    private float minImpactSpeed = 10f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & obstacleMask) == 0) return;
        if (isCarried) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed >= minImpactSpeed) OnHardHit();
    }

    private void OnHardHit()
    {
        CreateCoins(coinsCount);
        Destroy(gameObject);
    }

    private void CreateCoins(int count)
    {
        if (count >= coinsCount) count = coinsCount;

        for (int i = 0; i < count; i++)
        {
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);

            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                var force = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(5f, 10f);
                coinRb.AddForce(force, ForceMode2D.Impulse);
            }

            coinsCount--;
        }
    }
}