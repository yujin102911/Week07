using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestContentUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questContentText;
    [SerializeField] private Image questCompleteLine;
    private bool isCompleted;

    void Awake()
    {
        isCompleted = false;

        questCompleteLine.type = Image.Type.Filled;
        questCompleteLine.fillMethod = Image.FillMethod.Horizontal;
        questCompleteLine.fillOrigin = 0;
        questCompleteLine.fillAmount = 0f;

        var rt = questCompleteLine.rectTransform;
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);

        questCompleteLine.gameObject.SetActive(false);
    }

    public void SetContent(string content, bool completed)
    {
        questContentText.text = content;
        questContentText.ForceMeshUpdate();

        var rt = questCompleteLine.rectTransform;
        rt.sizeDelta = new Vector2(questContentText.preferredWidth, rt.sizeDelta.y);

        if (!completed)
        {
            isCompleted = false;
            questCompleteLine.gameObject.SetActive(false);
            questCompleteLine.fillAmount = 0f;
            StopAllCoroutines();
            return;
        }

        if (isCompleted) return;
        isCompleted = true;
        questCompleteLine.gameObject.SetActive(true);
        questCompleteLine.fillAmount = 0f;

        StopAllCoroutines();
        StartCoroutine(AnimateFill());
    }

    private IEnumerator AnimateFill()
    {
        float t = 0f;
        yield return new WaitForSeconds(0.5f);
        while (t < 0.6f)
        {
            questCompleteLine.fillAmount = Mathf.SmoothStep(0f, 1f, t / 0.6f);
            t += Time.deltaTime;
            yield return null;
        }
        questCompleteLine.fillAmount = 1f;
    }
}