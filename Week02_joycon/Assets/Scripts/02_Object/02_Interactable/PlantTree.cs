using UnityEngine;

public class PlantTree : MonoBehaviour
{
    [SerializeField] PlantGrow[] plants;
    [SerializeField] int growSpeed = 255;
    [SerializeField] bool Watering = false;

    void Update()
    {
        if (Watering)
        {
            foreach (var tomato in plants)
            {
                if (tomato != null && tomato.spriteRender.enabled)
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