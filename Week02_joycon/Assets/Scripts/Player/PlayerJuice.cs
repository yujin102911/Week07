using UnityEngine;
using System.Collections;

public class PlayerJuice : MonoBehaviour
{
    //������&��Ʈ��ġ�� ������ ���� ��������Ʈ ������Ʈ�� Transform
    [SerializeField] private Transform spriteTransform;

    [SerializeField] private Vector3 jumpSqueeze = new Vector3(0.8f, 1.2f, 1f);
    [SerializeField] private Vector3 landSqueeze = new Vector3(1.2f, 0.8f, 1f);
    [SerializeField] private float squeezeDuration = 0.1f;

    [SerializeField] private float landEffectCooldown = 0.2f;
    private float lastLandTIme;

    private Controller2D controller;

    private bool wasOnGround; //���� �����ӿ� ���� �־�����
    private bool isSqueezing = false; //���� ������ ȿ���� ��� ������
    private Vector3 originalScale;

    private void Start()
    {
        controller = GetComponent<Controller2D>();

        if(spriteTransform == null)
        {
            Transform foundSprite = transform.Find("Sprite");
            if (foundSprite != null) { spriteTransform = foundSprite; }
            else { this.enabled = false; }
            
        }
        originalScale = spriteTransform.localScale;
        wasOnGround = controller.collisions.below;
    }

    private void Update()
    {
        bool landedThisFrame = !wasOnGround && controller.collisions.below;

        if (landedThisFrame && Time.time - lastLandTIme > landEffectCooldown) 
        {
            lastLandTIme = Time.time;
            PlayLandEffects();
        }//���� ȿ��
        wasOnGround = controller.collisions.below;

    }

    public void PlayJumpEffects() //Player.cs�� ȣ��
    {
        StartSqueeze(jumpSqueeze.x, jumpSqueeze.y, squeezeDuration);

        //���� ��ƼŬ, ���� ����Ʈ �߰� ����
    }

    private void PlayLandEffects()
    {
        StartSqueeze(landSqueeze.x, landSqueeze.y, squeezeDuration);

        //���� ��ƼŬ, ���� ����Ʈ �߰� ����
    }

    private void StartSqueeze(float xSqueeze, float ySqueeze, float duration)
    {
        if(!isSqueezing) { StartCoroutine(SqueezeCoroutine(xSqueeze, ySqueeze, duration)); }
    }

    private IEnumerator SqueezeCoroutine(float xSqueeze, float ySqueeze,float duration)
    {
        isSqueezing = true;

        Vector3 squeezeScale = new Vector3(xSqueeze, ySqueeze, originalScale.z);
        spriteTransform.localScale = squeezeScale;

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float percent = timer / duration;
            spriteTransform.localScale = Vector3.Lerp(squeezeScale, originalScale, percent);
            yield return null;
        }
        spriteTransform.localScale = originalScale;

        isSqueezing = false;
    }


}
