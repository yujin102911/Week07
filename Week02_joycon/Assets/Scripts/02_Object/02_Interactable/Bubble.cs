using UnityEngine;

public class Bubble : MonoBehaviour
{
    [SerializeField] GameObject[] bubbles;

    [SerializeField] int cleanSpeedmin = 3;
    [SerializeField] int cleanSpeedmax = 5;
    [SerializeField] bool Watering = false;
    [SerializeField] float bubbleCount;

    void Start()
    {
        bubbleCount = bubbles.Length;
    }

    void Update()
    {
        if (Watering)
        {
            int cleanSpeed = Random.Range(cleanSpeedmin, cleanSpeedmax + 1);
            bubbleCount -= cleanSpeed * Time.deltaTime;

            if ((int)bubbleCount >= 0 && (int)bubbleCount < bubbles.Length && bubbles[(int)bubbleCount].activeSelf)
            {
                bubbles[(int)bubbleCount].SetActive(false);
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
