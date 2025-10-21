using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [SerializeField] private int targetFPS = 60;

    void Awake()
    {
        // VSync ���� (VSync�� ���� ������ Application.targetFrameRate�� ���õ�)
        QualitySettings.vSyncCount = 0;

        // ��ǥ ������ ����
        Application.targetFrameRate = targetFPS;
    }
}
