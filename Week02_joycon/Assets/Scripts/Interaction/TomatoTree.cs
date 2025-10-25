using UnityEngine;

public class TomatoTree : MonoBehaviour
{
    [SerializeField] TomatoGrow[] tomatos;
    [SerializeField] int growSpeed = 255;
    Vector2 firstScale;
    bool isWatered = false;

    void Start()
    {
        firstScale = tomatos[0].transform.localScale;
    }

    void Update()
    {
        if (isWatered)
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
        isWatered = (collision.CompareTag("Water"));
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        isWatered = (!collision.CompareTag("Water"));
    }
}
