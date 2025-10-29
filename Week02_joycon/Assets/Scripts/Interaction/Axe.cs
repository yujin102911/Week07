using UnityEngine;

public class Axe : MonoBehaviour
{
    Controller2D controller2D;
    public bool falling;
    [SerializeField] Carryable carryable;

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

    void Update()
    {
        falling = controller2D.isFalling && carryable.carrying;
    }
}