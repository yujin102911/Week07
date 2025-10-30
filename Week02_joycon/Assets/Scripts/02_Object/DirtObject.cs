using UnityEngine;

public class DirtObject : MonoBehaviour
{
    private static int dirtamount;

    private void Start()
    {
        dirtamount++;
    }

    private void ClearDirt()
    {
        dirtamount--;
        if (dirtamount == 0) QuestRuntime.Instance.SetFlag(FlagId.WipingDust);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Rag rag) == false) return;
        if (rag.TryCleanDirt() == false) return;

        ClearDirt();
    }
}