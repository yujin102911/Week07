using UnityEngine;

public class Axe : MonoBehaviour
{
    [SerializeField] Carryable carryable;
    private Controller2D controller2D;
    public bool falling;

    void Start()
    {
        controller2D = Player.Instance.GetComponent<Controller2D>();
    }

    void Update()
    {
        falling = controller2D.isFalling && carryable.GetIsCarrying();
    }
}