using UnityEngine;

public class DirtObject : MonoBehaviour
{
    private static int dirtamount;

    private void OnEnable()
    {
        dirtamount++;
    }

    private void OnDestroy()
    {
        ClearDirt();
    }

    private void ClearDirt()
    {
        dirtamount--;
        if (dirtamount <= 0)
        {
            QuestRuntime.Instance.SetFlag(FlagId.Dust_AllCleared);
            GameLogger.Instance.LogDebug(this, "먼지 퀘스트 완료");
        }
    }
}