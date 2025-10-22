// ReplayPicker.cs
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using TMPro; // 꼭 필요

public class ReplayPicker : MonoBehaviour
{
    public InputReplayer replayer;  // 더미 Player에 붙은 InputReplayer    
    public TMP_Dropdown dropdown;      // UGUI Dropdown
    public Slider speedSlider;      // 1~10 배속 슬라이더(옵션)

    string[] files;

    void Start()
    {
        files = Directory.Exists(LogPathUtil.Root)
            ? Directory.GetFiles(LogPathUtil.Root, "input*.jsonl", SearchOption.AllDirectories)
                      .OrderByDescending(File.GetLastWriteTimeUtc).ToArray()
            : new string[0];

        dropdown.ClearOptions();
        dropdown.AddOptions(files.Select(Path.GetFileName).ToList());

        if (speedSlider != null)
        {
            speedSlider.minValue = 1f;
            speedSlider.maxValue = 10f;
            speedSlider.value = 1f;
            speedSlider.wholeNumbers = true;
            speedSlider.onValueChanged.AddListener(SetSpeed);
        }
    }

    public void PlaySelected()
    {
        if (files == null || files.Length == 0) return;
        var idx = Mathf.Clamp(dropdown.value, 0, files.Length - 1);
        replayer.LoadAndPlay(files[idx]);
    }

    public void Stop() => replayer.Stop();

    public void SetSpeed(float x)
    {
        replayer.playbackSpeed = Mathf.Clamp(x, 1f, 10f);
        if (replayer.enabled) Time.timeScale = replayer.playbackSpeed;
    }

    // 최신 로그 자동 재생용(옵션)
    [ContextMenu("Play Latest")]
    void PlayLatest()
    {
        var latest = InputReplayer.FindLatestInputLog();
        if (latest != null) replayer.LoadAndPlay(latest);
    }
}