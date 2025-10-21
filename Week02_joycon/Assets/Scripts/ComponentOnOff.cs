using UnityEngine;

public class ComponentOnOff : MonoBehaviour
{
    public MonoBehaviour targetScript;
    public bool conditionMet = false;

    void Update()
    {
        if (conditionMet && !targetScript.enabled)
        {
            targetScript.enabled = true; // ������ �����ϸ� ��ũ��Ʈ ����
        }

    }
}
