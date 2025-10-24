using UnityEngine;

public class Axe : MonoBehaviour
{
    Controller2D controller2D;
    public bool falling;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (controller2D == null)
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo != null)
            {
                controller2D = pgo.GetComponent<Controller2D>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        falling = controller2D.isFalling?  true: false;
    }
}
