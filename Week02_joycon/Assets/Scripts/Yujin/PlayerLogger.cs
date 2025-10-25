using System.Collections;
using System.IO;
using UnityEngine;

public class PlayerLogger : MonoBehaviour
{
    [SerializeField] private float logInterval = 3f;

    [Header("Optional Custom File Name")]
    [SerializeField] private string customFileName = "";

    private string positionLogPath;
    private PlayerCarrying playerCarrying;

    private void Start()
    {
        playerCarrying = GetComponent<PlayerCarrying>(); 

        string exeDir = Path.GetDirectoryName(Application.dataPath); //별도 파일 생성
        string logDir = Path.Combine(exeDir, "playerlog");
        Directory.CreateDirectory(logDir);

        string filePrefix = string.IsNullOrWhiteSpace(customFileName) ? "PlayerPositions" : customFileName.Trim();
        string fileName = $"{filePrefix}_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv";
        positionLogPath = Path.Combine(logDir, fileName);
        File.AppendAllText(positionLogPath, "Time,X,Y,Z,ItemId\n"); // 헤더 추가

        StartCoroutine(LogPlayerPositionRoutine());
    }
    IEnumerator LogPlayerPositionRoutine()
    {
        while (true)
        {
            Vector3 pos = transform.position;

            string itemId = "null";
            if (playerCarrying != null && playerCarrying.carriedObjects.Count > 0)
            {
                var firstItem = playerCarrying.carriedObjects[0];
                if (firstItem != null)
                {
                    var carryable = firstItem.GetComponent<Carryable>();
                    if (carryable != null && !string.IsNullOrEmpty(carryable.Id))
                        itemId = carryable.Id;
                }
            }

            string line = $"{System.DateTime.Now:HH:mm:ss.fff},{pos.x},{pos.y},{pos.z},{itemId}\n";
            File.AppendAllText(positionLogPath, line);

            //if (GameLogger.Instance != null)
                //GameLogger.Instance.LogDebug(this, $"Player position: {pos}");

            yield return new WaitForSeconds(logInterval);
        }
    }

}
