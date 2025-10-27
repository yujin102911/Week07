using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    [SerializeField] Button startButton;
    [SerializeField] string scene;

    private void Start()
    {
        startButton.onClick.AddListener(OnClickStart);
    }

    void OnClickStart()
    {
        SceneManager.LoadScene(scene);
    }

}
