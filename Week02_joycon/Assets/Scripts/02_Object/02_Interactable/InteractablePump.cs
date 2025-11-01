using UnityEngine;

public class InteractablePump : MonoBehaviour
{
    [SerializeField] Controller2D controller2D;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (controller2D == null)
            controller2D = Player.Instance.GetComponent<Controller2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
