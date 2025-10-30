using UnityEngine;

public class FireWood : MonoBehaviour
{
    [SerializeField] BoxCollider2D boxCollider2D;
    [SerializeField] GameObject[] Prefabs;

    void Start()
    {
        if (boxCollider2D == null) boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform[] childs = collision.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in childs)
        {
            if (t.gameObject.CompareTag("Axe") && t.GetComponent<Axe>().falling == true)
            {
                for (int i = 0; i < Prefabs.Length; i++)
                {
                    if (Prefabs[i] != null)
                    {
                        Instantiate(Prefabs[i],
                            new Vector2(transform.position.x/* - boxCollider2Dx / 2 * Prefabs.Length + boxCollider2Dx * i*/, transform.position.y),
                            Quaternion.Euler(0, 0, transform.localRotation.z));
                    }
                }
                Destroy(gameObject);

            }
        }
    }
}