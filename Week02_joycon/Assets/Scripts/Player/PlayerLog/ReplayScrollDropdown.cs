using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Linq;

public class ReplayScrollDropdown : MonoBehaviour
{
    [Header("Wiring")]
    public InputReplayer replayer;
    public Button headerButton;                // 헤더 버튼(열기/닫기)
    public TextMeshProUGUI headerLabel;        // 현재 선택 라벨
    public GameObject overlay;                 // 바깥 클릭 닫기 (Panel/Image)
    public GameObject panel;                   // 펼침 패널
    public Transform content;                  // ScrollView/Viewport/Content
    public GameObject itemPrefab;              // ReplayListItem 프리팹
    public Slider speedSlider;                 // 1~10 배속 (옵션)

    string[] files = Array.Empty<string>();
    int selectedIndex = -1;
    int pendingDeleteIndex = -1;

    void Start()
    {
        headerButton.onClick.AddListener(TogglePanel);
        if (overlay)
        {
            overlay.SetActive(false);
            overlay.GetComponent<Button>()?.onClick.AddListener(ClosePanel);
        }
        panel.SetActive(false);

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

    public void RefreshList()
    {
        // 기존 항목 제거
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        // 파일 목록 스캔(최신순)
        files = Directory.Exists(LogPathUtil.Root)
            ? Directory.GetFiles(LogPathUtil.Root, "input*.jsonl", SearchOption.AllDirectories)
                      .OrderByDescending(File.GetLastWriteTimeUtc).ToArray()
            : Array.Empty<string>();

        // 항목 생성
        for (int i = 0; i < files.Length; i++)
        {
            int idx = i;
            var go = Instantiate(itemPrefab, content);
            var view = go.GetComponent<ReplayListItemView>();

            string label = File.GetLastWriteTime(files[i]).ToString("yyMMdd_HH:mm:ss");
            view.Init(
                labelText: label,
                onPlay: () => { SelectIndex(idx); PlaySelected(); ClosePanel(); },
                onDelete: () => DeleteNow(idx),
                selected: idx == selectedIndex
            );
        }

        // 선택 인덱스 보정
        if (files.Length == 0) selectedIndex = -1;
        else if (selectedIndex < 0 || selectedIndex >= files.Length) selectedIndex = 0;

        UpdateHeader();
    }

    void SelectIndex(int idx)
    {
        if (files.Length == 0) { selectedIndex = -1; UpdateHeader(); return; }
        selectedIndex = Mathf.Clamp(idx, 0, files.Length - 1);

        // 하이라이트 갱신
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
        Debug.Log("[ReplayScrollDropdown] Play: " + files[selectedIndex]);
    }

    void DeleteNow(int idx)
    {
        try
        {
            replayer.Stop();
            var path = files[idx];
            if (File.Exists(path)) File.Delete(path);

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir) &&
                Directory.GetFiles(dir).Length == 0 &&
                Directory.GetDirectories(dir).Length == 0)
            {
                Directory.Delete(dir);
            }
            Debug.Log("[ReplayScrollDropdown] Deleted: " + path);
        }
        catch (Exception e)
        {
            Debug.LogError("[ReplayScrollDropdown] Delete failed: " + e.Message);
        }

        // 목록 갱신(선택 보정 포함)
        RefreshList();
    }

    // 열기/닫기 (드롭다운 느낌)
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