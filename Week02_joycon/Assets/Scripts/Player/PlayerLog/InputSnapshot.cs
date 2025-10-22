using UnityEngine;

public static class InputSnapshot
{
    // PlayerInputHandler가 매 프레임 최신값만 갱신
    public static Vector2 Move;       // (-1~1, x=좌우, y=상하) 2D면 y는 주로 점프/사다리 등
    public static bool JumpHeld;
    public static bool JumpDown;      // 이번 틱에 눌림
    public static bool JumpUp;        // 이번 틱에 뗌

    // Recorder가 FixedUpdate 마지막에 리셋
    public static void ConsumeFrameEdges()
    {
        JumpDown = false;
        JumpUp = false;
    }
}
