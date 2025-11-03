using System.Xml.Schema;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapToggle : MonoBehaviour
{
    public GameObject minimapPanel;
    public GameObject realMinimapPanel; //화면에 항시 있는 동그란 미니맵
    public InputActionReference toggleActionReference;

    private void OnEnable()
    {
        toggleActionReference.action.Enable();
        toggleActionReference.action.performed += OnToggleMinimap;
    }
    private void OnDisable()
    {
        toggleActionReference.action.performed -= OnToggleMinimap;
        toggleActionReference.action.Disable();
    }

    private void OnToggleMinimap(InputAction.CallbackContext context)
    {
        if (QuestUI.questId == 1000) return;

        if (minimapPanel != null)
        {
            minimapPanel.SetActive(!minimapPanel.activeSelf);
        }
        if (realMinimapPanel != null)
        {
            realMinimapPanel.SetActive(!realMinimapPanel.activeSelf);
        }
    }

}
