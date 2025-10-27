using UnityEngine;

public class BubblesCheck : MonoBehaviour
{
    [SerializeField] private CarryableMimic mimic;
    [SerializeField] GameObject[] bubbles;
    public bool cleanUp = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        int cleanCount = 0;
        foreach (GameObject bubble in bubbles)
        {
            if (!bubble.activeSelf)
            {
                cleanCount++;
            }
        }
        if (cleanCount == bubbles.Length)
        {
            cleanUp = true;
            mimic.CleanUp();
        }
    }
}
