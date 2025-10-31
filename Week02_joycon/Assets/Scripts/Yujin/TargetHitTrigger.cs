using UnityEngine;

public class TargetHIt : MonoBehaviour
{
    [Header("Settings")]
    public int hitsRequired = 3;
    public string targetLayerName = "carryable";
    public SpriteRenderer targetSpriteRenderer;
    public Sprite newSprite;
    public ParticleSystem particleEffect;

    private int currentHitCount = 0;
    private bool isTriggered = false;
    private int carryableLayer;

    private void Start()
    {
        carryableLayer = LayerMask.NameToLayer(targetLayerName);

        if (targetSpriteRenderer == null)
        {
            GameLogger.Instance.LogError(this, $"Target Sprite Renderer가 연결되지 않았습니다!: { this.gameObject}");
        }
        if (newSprite == null)
        {
            GameLogger.Instance.LogError(this, $"New Sprite가 연결되지 않았습니다!: {this.gameObject}");
        }
        if (particleEffect == null)
        {
            GameLogger.Instance.LogError(this, $"Particle Effect가 연결되지 않았습니다!: {this.gameObject}");
        }
        if (carryableLayer == -1)
        {
            GameLogger.Instance.LogError(this, "'" + targetLayerName + "' 레이어가 존재하지 않습니다. Project Settings > Tags and Layers에서 먼저 생성해주세요.: {this.gameObject}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //이미 이벤트가 발생했으면 무시
        if (isTriggered) return;

        //부딪힌 오브젝트의 레이어가 carryable이 맞는지 확인
        if (other.gameObject.layer == carryableLayer)
        {
            Carryable carryableScript = other.GetComponent<Carryable>();

            if (carryableScript != null && carryableScript.GetIsCarried() == false)
            {
                currentHitCount++; // 카운트 증가
                GameLogger.Instance.LogDebug(this, "파타냐 한대 맞았아요");

                //카운트가 목표 횟수에 도달했는지 확인
                if (currentHitCount >= hitsRequired)
                {
                    TriggerActions(); // 이벤트 실행
                }
            }
        }
    }

    private void TriggerActions()
    {
        //재실행 방지
        isTriggered = true;

        //스프라이트 변경
        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = newSprite;
        }

        //파티클 재생
        if (particleEffect != null)
        {
            particleEffect.Play();
        }

        GameLogger.Instance.LogDebug(this, "목표 달성! 스프라이트 변경 및 파티클 재생 완료.");
    }

}
