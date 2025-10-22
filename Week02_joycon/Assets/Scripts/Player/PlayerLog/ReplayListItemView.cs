using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ReplayListItemView : MonoBehaviour
{
    public TextMeshProUGUI label;
    public Button playButton;
    public Button deleteButton;
    public Image background;

    Action onPlay, onDelete;
    bool selected;

    public void Init(string labelText, Action onPlay, Action onDelete, bool selected = false)
    {
        this.onPlay = onPlay;
        this.onDelete = onDelete;
        if (label) label.text = labelText;

        if (playButton) playButton.onClick.AddListener(() => this.onPlay?.Invoke());
        if (deleteButton) deleteButton.onClick.AddListener(() => this.onDelete?.Invoke());

        SetSelected(selected);

        // 라벨 영역 클릭도 재생으로 쓰고 싶으면 루트에 Button 달아 연결
        var selfBtn = GetComponent<Button>();
        if (selfBtn) selfBtn.onClick.AddListener(() => this.onPlay?.Invoke());
    }

    public void SetSelected(bool on)
    {
        selected = on;
        if (background)
            background.color = on ? new Color(0.2f, 0.5f, 1f, 0.15f) : new Color(1, 1, 1, 0f);
    }
}