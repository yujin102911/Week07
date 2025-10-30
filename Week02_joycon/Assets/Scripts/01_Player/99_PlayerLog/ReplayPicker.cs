using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System;
using System.Collections.Generic;

public class ReplayPickerTMP : MonoBehaviour
{
    [SerializeField] private InputReplayer replayer;
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private bool enableSpaceToggle;
    private string[] files;
    private bool isPaused = false;

    void Start()
    {
        RefreshList();

        if (speedSlider != null)
        {
            speedSlider.minValue = 1f;
            speedSlider.maxValue = 10f;
            speedSlider.value = 1f;
            speedSlider.wholeNumbers = true;
            speedSlider.onValueChanged.AddListener(SetSpeed);
        }

        if (playButton) playButton.onClick.AddListener(PlaySelected);
        if (stopButton) stopButton.onClick.AddListener(Stop);
        if (pauseButton) pauseButton.onClick.AddListener(TogglePauseUI);
    }

    void Update()
    {
        if (enableSpaceToggle && Input.GetKeyDown(KeyCode.Space)) TogglePauseUI();
    }

    public void RefreshList()
    {
        files = Directory.Exists(LogPathUtil.Root)
            ? Directory.GetFiles(LogPathUtil.Root, "input*.jsonl", SearchOption.AllDirectories)
                      .OrderByDescending(File.GetLastWriteTimeUtc).ToArray()
            : Array.Empty<string>();

        var labels = new List<string>(files.Length);
        foreach (var f in files)
        {
            DateTime t = File.GetLastWriteTime(f);
            labels.Add(t.ToString("yyMMdd_HH:mm:ss"));
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(labels);
    }

    public void PlaySelected()
    {
        if (files == null || files.Length == 0) return;
        int idx = Mathf.Clamp(dropdown.value, 0, files.Length - 1);
        replayer.LoadAndPlay(files[idx]);
        isPaused = false;
        Debug.Log("[ReplayPickerTMP] Playing: " + files[idx]);
    }

    public void Stop()
    {
        replayer.Stop();
        isPaused = false;
    }

    public void SetSpeed(float x)
    {
        replayer.SetPlaybackSpeed(Mathf.Clamp(x, 1f, 10f));
    }

    public void TogglePauseUI()
    {
        isPaused = !isPaused;
        if (isPaused) replayer.Pause();
        else replayer.Resume();
    }

    public void PauseUI()
    {
        isPaused = true;
        replayer.Pause();
    }

    public void ResumeUI()
    {
        isPaused = false;
        replayer.Resume();
    }

    [ContextMenu("Log Save Folder")]
    public void LogSaveFolder()
    {
        Debug.Log("[Logs] " + LogPathUtil.Root);
    }
}
