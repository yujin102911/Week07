using UnityEngine;

public static class InputSnapshot
{
    public static Vector2 Move;
    public static bool JumpHeld;
    public static bool JumpDown;
    public static bool JumpUp;
    public static bool Interact;
    public static bool Drop;

    public static void ConsumeFrameEdges()
    {
        JumpDown = false;
        JumpUp = false;
        Interact = false;
        Drop = false;
    }
}
