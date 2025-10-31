using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    [SerializeField] private Image fadePanel;
    [SerializeField] private GameObject endingPanel;
    private float fadeDuration = 3.0f;

    private void Start()
    {
        fadePanel.gameObject.SetActive(false);
        endingPanel.SetActive(false);

        QuestManager.OnAllQuestsCompleted += SetEndingUIActive;
    }

    private void SetEndingUIActive()
    {
        StartCoroutine(FadeInAndShowEndingUI());
    }

    private IEnumerator FadeInAndShowEndingUI()
    {
        fadePanel.gameObject.SetActive(true);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            fadePanel.color = new Color(0f, 0f, 0f, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        endingPanel.SetActive(true);
    }
}