using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public void OnMove(InputAction.CallbackContext context)
    {
        var move = context.ReadValue<Vector2>();
        Player.Instance.SetDirectionalInput(move);
        InputSnapshot.Move = move;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Player.Instance.OnJumpInputDown();
            InputSnapshot.JumpHeld = true;
            InputSnapshot.JumpDown = true;
        }

        if (context.canceled)
        {
            Player.Instance.OnJumpInputUp();
            InputSnapshot.JumpHeld = false;
            InputSnapshot.JumpUp = true;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (Player.TryInteract() == false) Player.TryPickUp();
            InputSnapshot.Interact = true;
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Player.TryDrop();
            InputSnapshot.Drop = true;
        }
    }
}