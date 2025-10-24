using UnityEngine;

public class MimicBubble : MonoBehaviour
{
    private float timerMax = 2.0f;
    private float timer = 0.0f;

    private void Update()
    {
        if (timer < 0) gameObject.SetActive(false);
        timer -= Time.deltaTime;
    }

    public void SetOn()
    {
        if (gameObject.activeSelf == false) gameObject.SetActive(true);
        timer = timerMax;
    }
}