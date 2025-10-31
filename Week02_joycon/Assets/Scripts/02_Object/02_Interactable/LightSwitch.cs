using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] private Light2D targetLight;
    private bool isLightOn = false;

    private void Awake()
    {
        if (targetLight != null)
        {
            targetLight.enabled = false;
        }
    }
    public bool TryInteract()
    {
        if (isLightOn || targetLight == null) return false;
        targetLight.enabled = true;
        isLightOn = true;
        return true;
    }
}
