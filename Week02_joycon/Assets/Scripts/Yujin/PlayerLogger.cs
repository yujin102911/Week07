using System.Collections;
using System.IO;
using UnityEngine;

public class PlayerLogger : MonoBehaviour
{
    [SerializeField] private float logInterval = 3f;
    private string positionLogPath;

    private void Start()
    {
        string exeDir = Path.GetDirectoryName(Application.dataPath); //���� ���� ����
        string logDir = Path.Combine(exeDir, "log");
        Directory.CreateDirectory(logDir);

        string fileName = $"PlayerPositions_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        positionLogPath = Path.Combine(logDir, fileName);
        File.AppendAllText(positionLogPath, "Time,X,Y,Z\n"); // ��� �߰�

        StartCoroutine(LogPlayerPositionRoutine());
    }
    IEnumerator LogPlayerPositionRoutine()
    {
        while (true)
        {
            Vector3 pos = transform.position;
            string line = $"{System.DateTime.Now:HH:mm:ss.fff},{pos.x},{pos.y},{pos.z}\n";
            File.AppendAllText(positionLogPath, line);

            // ���� GameLogger���� ���� ��� �ʹٸ� �� �߰�
            if (GameLogger.Instance != null)
                GameLogger.Instance.LogDebug(this, $"Player position: {pos}");

            yield return new WaitForSeconds(logInterval);
        }
    }

}
