using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class BallonAxe : MonoBehaviour
{
    [SerializeField] GameObject handle;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform[] childs = collision.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in childs)
        {
            if (t.CompareTag("Axe"))
            {
                Destroy(handle);
                Destroy(transform.parent.gameObject);
                return;
            }
        }
    }
}
