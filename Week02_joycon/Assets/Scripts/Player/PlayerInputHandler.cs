using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        var move = ctx.ReadValue<Vector2>();
        player.SetDirectionalInput(move);
        InputSnapshot.Move = move;
    }
    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            player.OnJumpInputDown();
            InputSnapshot.JumpHeld = true;
            InputSnapshot.JumpDown = true;
        }

        if (ctx.canceled)
        {
            player.OnJumpInputUp();
            InputSnapshot.JumpHeld = false;
            InputSnapshot.JumpUp = true;
        }
    }
}