using UnityEngine;
using UnityEngine.Android;

public class Mirror : MonoBehaviour
{
    [SerializeField] private Camera mirrorCamera;

    [SerializeField] private Vector2Int textureResolution = new Vector2Int(64, 64);

    private void Start()
    {
        if (mirrorCamera == null)
        {
            Debug.Log($"{gameObject.name}�� ī�޶� �Ҵ���� �ʾҽ��ϴ�.");
            return;
        }
        Renderer mirrorSurface = GetComponent<Renderer>();
        if(mirrorSurface == null) { Debug.Log("Rederer������Ʈ�� �����ϴ�."); return; } 

        RenderTexture renderTexture = new RenderTexture(textureResolution.x, textureResolution.y, 24);

        mirrorCamera.targetTexture = renderTexture;

        mirrorSurface.material.mainTexture = renderTexture;
    }
    private void OnDestroy()
    {
        if (mirrorCamera != null && mirrorCamera.targetTexture != null) mirrorCamera.targetTexture.Release(); 
    }

}
