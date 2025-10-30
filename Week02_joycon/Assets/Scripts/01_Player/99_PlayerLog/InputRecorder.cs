using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputRecorder : MonoBehaviour
{
    [SerializeField] private bool recording = true;
    private string path;
    private int tick;
    private float fixedDt;

    void Start()
    {
        if (recording == false)
        {
            enabled = false;
            return;
        }

        fixedDt = Time.fixedDeltaTime;
        var level = SceneManager.GetActiveScene().name;
        var dir = Path.Combine(LogPathUtil.Root, level, SessionBootstrap.SessionId);

        Directory.CreateDirectory(dir);
        path = LogPathUtil.GetUniquePath(dir, "input");

        File.AppendAllText(
            path,
            $"{{\"meta\":\"input_v1\",\"session\":\"{SessionBootstrap.SessionId}\",\"fixedDt\":{fixedDt:F4}}}\n",
            Encoding.UTF8
        );
    }

    void FixedUpdate()
    {
        if (recording == false || string.IsNullOrEmpty(path)) return;

        var m = InputSnapshot.Move;
        int held = InputSnapshot.JumpHeld ? 1 : 0;
        int dwn = InputSnapshot.JumpDown ? 1 : 0;
        int up = InputSnapshot.JumpUp ? 1 : 0;
        int interact = InputSnapshot.Interact ? 1 : 0;
        int drop = InputSnapshot.Drop ? 1 : 0;

        var line =
            $"{{\"tick\":{tick},\"dt\":{fixedDt:F4},\"mx\":{m.x:F3},\"my\":{m.y:F3}," +
            $"\"jumpHeld\":{held},\"jumpDown\":{dwn},\"jumpUp\":{up},\"interact\":{interact},\"drop\":{drop}}}\n";

        File.AppendAllText(path, line, Encoding.UTF8);

        tick++;
        InputSnapshot.ConsumeFrameEdges();
    }
}
