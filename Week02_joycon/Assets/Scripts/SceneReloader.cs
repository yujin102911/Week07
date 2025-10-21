using UnityEngine;
using UnityEngine.InputSystem;       // �� ��ǲ �ý���
using UnityEngine.SceneManagement;   // �� �ε��

public class SceneReloader : MonoBehaviour
{
    // PlayerInput ������Ʈ���� �̺�Ʈ�� ���� �Լ�
    public void OnRestart(InputAction.CallbackContext context)
    {
        // performed ������ ���� ���� (��ư ������ ��)
        if (context.performed)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentScene);
        }
    }
}
