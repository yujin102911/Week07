using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerCarrying))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    private Player player;
    private PlayerCarrying playerCarrying;

    private void Awake()
    {
        player = GetComponent<Player>();
        playerCarrying = GetComponent<PlayerCarrying>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        var move = context.ReadValue<Vector2>();
        player.SetDirectionalInput(move);
        InputSnapshot.Move = move;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            player.OnJumpInputDown();
            InputSnapshot.JumpHeld = true;
            InputSnapshot.JumpDown = true;
        }

        if (context.canceled)
        {
            player.OnJumpInputUp();
            InputSnapshot.JumpHeld = false;
            InputSnapshot.JumpUp = true;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerCarrying.TryInteract();
            InputSnapshot.Interact = true;
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerCarrying.TryDrop();
            InputSnapshot.Drop = true;
        }
    }
}