using UnityEngine;

public class TomatoTree : MonoBehaviour
{
    [SerializeField] TomatoGrow[] tomatos;
    [SerializeField] int growSpeed = 255;
    Vector2 firstScale;
    [SerializeField] bool Watering = false;

    void Start()
    {
        firstScale = tomatos[0].transform.localScale;
    }

    void Update()
    {
        if (Watering)
        {
            foreach(var tomato in tomatos)
            {
                if (tomato!=null)
                tomato.growCurrent += growSpeed * Time.deltaTime;
            }
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger Entered with: " + collision.gameObject.name);
        if (collision.transform.CompareTag("Water"))
            Watering = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Trigger Exit with: " + collision.gameObject.name);

        if (collision.CompareTag("Water"))
            Watering = false;
    }
}
