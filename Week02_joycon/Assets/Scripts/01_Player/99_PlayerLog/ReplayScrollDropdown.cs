using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Linq;

public class ReplayScrollDropdown : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private InputReplayer replayer;

    [SerializeField] private Button headerButton;
    [SerializeField] private TextMeshProUGUI headerLabel;
    [SerializeField] private GameObject overlay;
    [SerializeField] private GameObject panel;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject itemPrefab;

    [Header("Playback Controls")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseToggleButton;
    [SerializeField] private Button stopButton;
    [SerializeField] private Slider speedSlider;
    [SerializeField] private bool enableSpaceToggle = true;

    private string[] files = Array.Empty<string>();
    private int selectedIndex = -1;
    private bool isPaused = false;

    void Start()
    {
        if (headerButton) headerButton.onClick.AddListener(TogglePanel);
        if (overlay)
        {
            overlay.SetActive(false);
            overlay.GetComponent<Button>()?.onClick.AddListener(ClosePanel);
        }
        if (panel) panel.SetActive(false);

        if (playButton) playButton.onClick.AddListener(PlaySelected);
        if (pauseToggleButton) pauseToggleButton.onClick.AddListener(TogglePauseUI);
        if (stopButton) stopButton.onClick.AddListener(Stop);

        if (speedSlider)
        {
            speedSlider.minValue = 1f;
            speedSlider.maxValue = 10f;
            speedSlider.wholeNumbers = true;
            speedSlider.value = 1f;
            speedSlider.onValueChanged.AddListener(v => replayer.SetPlaybackSpeed(v));
        }

        RefreshList();
        UpdateHeader();
    }

    void Update()
    {
        if (enableSpaceToggle && Input.GetKeyDown(KeyCode.Space))
            TogglePauseUI();
    }

    public void RefreshList()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        files = Directory.Exists(LogPathUtil.Root)
            ? Directory.GetFiles(LogPathUtil.Root, "input*.jsonl", SearchOption.AllDirectories)
                      .OrderByDescending(File.GetLastWriteTimeUtc).ToArray()
            : Array.Empty<string>();

        for (int i = 0; i < files.Length; i++)
        {
            int idx = i;
            var go = Instantiate(itemPrefab, content);
            var view = go.GetComponent<ReplayListItemView>();

            string label = File.GetLastWriteTime(files[i]).ToString("yyMMdd_HH:mm:ss");
            view.Init(
                labelText: label,
                onPlay: () => { SelectIndex(idx); PlaySelected(); ClosePanel(); },
                onDelete: () => DeleteNow(idx),          // ← 즉시 삭제
                selected: idx == selectedIndex
            );
        }

        if (files.Length == 0) selectedIndex = -1;
        else if (selectedIndex < 0 || selectedIndex >= files.Length) selectedIndex = 0;

        UpdateHeader();
    }

    void SelectIndex(int idx)
    {
        if (files.Length == 0) { selectedIndex = -1; UpdateHeader(); return; }
        selectedIndex = Mathf.Clamp(idx, 0, files.Length - 1);

        for (int i = 0; i < content.childCount; i++)
        {
            var v = content.GetChild(i).GetComponent<ReplayListItemView>();
            if (v) v.SetSelected(i == selectedIndex);
        }
        UpdateHeader();
    }

    void UpdateHeader()
    {
        if (!headerLabel) return;
        if (selectedIndex < 0 || files.Length == 0) headerLabel.text = "(no logs)";
        else headerLabel.text = File.GetLastWriteTime(files[selectedIndex]).ToString("yyMMdd_HH:mm:ss");
    }

    public void PlaySelected()
    {
        if (selectedIndex < 0 || files.Length == 0) return;
        replayer.LoadAndPlay(files[selectedIndex]);
        isPaused = false;
        Debug.Log("[ReplayScrollDropdown] Play: " + files[selectedIndex]);
    }

    public void Stop()
    {
        replayer.Stop();
        isPaused = false;
    }

    public void TogglePauseUI()
    {
        isPaused = !isPaused;
        if (isPaused) replayer.Pause(); else replayer.Resume();
    }

    void DeleteNow(int idx)
    {
        try
        {
            replayer.Stop();

            var path = files[idx];
            var sessionDir = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(sessionDir) && Directory.Exists(sessionDir)) Directory.Delete(sessionDir, true);
        }
        catch (Exception e)
        {
            Debug.LogError("[ReplayScrollDropdown] Delete failed: " + e.Message);
        }

        RefreshList();
    }

    public void TogglePanel()
    {
        bool open = !panel.activeSelf;
        panel.SetActive(open);
        if (overlay) overlay.SetActive(open);
    }
    public void ClosePanel()
    {
        panel.SetActive(false);
        if (overlay) overlay.SetActive(false);
    }

    [ContextMenu("Log Save Folder")]
    public void LogSaveFolder() => Debug.Log("[Logs] " + LogPathUtil.Root);
}