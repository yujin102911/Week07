using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //���� ������ �Ѿ�� �������� �ʵ��� ����
        }
        else
        {
            Destroy(gameObject); //�̹� �����ϸ� �����ϵ���
        }
    }
    public void LoadNextScene() //�ε��� �������� ���� �� �ε�
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex; //���� �� ���� �ε��� ����
        Debug.Log($"{currentSceneIndex + 1}�� �ε��� ���� �ε��մϴ�.");
        SceneManager.LoadScene(currentSceneIndex + 1); //���� �� �ε�
    }
    public void LoadSceneByName(string sceneName) //�� �̸����� ���� �� �ε�
    {
        Debug.Log($"{sceneName}�� �ε��մϴ�.");
        SceneManager.LoadScene(sceneName);
    }
    public void RestartScene() //���� �� �����
    {
        Debug.Log("���� ���� ��ε��մϴ�.");
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex); //���� �� �ε����� ��ε�
    }
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("������ �����մϴ�.");
    }

}
