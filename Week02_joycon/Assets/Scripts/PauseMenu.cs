using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    public GameObject pauseMenuUI;
    private bool isPaused = false;

    private void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed) TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused) Pause();
        else Resume();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }
    public void ShowEndingScreen()
    {
        StartCoroutine(EndingFadeSequenceCoroutine());
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    private System.Collections.IEnumerator EndingFadeSequenceCoroutine()
    {
        yield return new WaitForSeconds(3f);

        float elapsedTime = 0f;
        Color color = fadeImage.color;
        fadeImage.gameObject.SetActive(true);

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        color.a = 1f;
        fadeImage.color = color;

        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }
        fadeImage.gameObject.SetActive(false);

        Time.timeScale = 0f;
    }
}