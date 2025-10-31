using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] Carryable carryable;
    private Controller2D controller2D;
    public bool falling;
    public bool throwing;

    void Start()
    {
        controller2D = Player.Instance.GetComponent<Controller2D>();
    }

    void Update()
    {
        falling = controller2D.isFalling && carryable.GetIsCarried();
        throwing = carryable.throwing;
    }
}