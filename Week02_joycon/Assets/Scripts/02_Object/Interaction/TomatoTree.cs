using UnityEngine;

public class TomatoTree : MonoBehaviour
{
    [SerializeField] TomatoGrow[] tomatos;
    [SerializeField] int growSpeed = 255;
    [SerializeField] bool Watering = false;

    void Update()
    {
        if (Watering)
        {
            foreach (var tomato in tomatos)
            {
                if (tomato != null)
                    tomato.growCurrent += growSpeed * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Water")) Watering = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Water")) Watering = false;
    }
}