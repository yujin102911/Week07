using UnityEngine;
using System.Collections; // IEnumerator를 사용하기 위해 필요합니다.

public class FadeOutOnTrigger : MonoBehaviour
{
    [Header("Fade Out Settings")]
    public float fadeOutDuration = 1.0f; // 완전히 사라지는 데 걸리는 시간 (초)
    public bool destroyOnFadeOut = true; // 페이드아웃 후 오브젝트를 파괴할지 여부

    private SpriteRenderer spriteRenderer;
    private Collider2D triggerCollider; // 중복 트리거 방지용

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            //Debug.LogError("FadeOutOnTrigger: SpriteRenderer 컴포넌트를 찾을 수 없습니다! 2D 오브젝트가 아닌가요?", this);
            // 3D 오브젝트라면 MeshRenderer 등으로 변경해야 합니다.
        }

        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider == null)
        {
            //Debug.LogError("FadeOutOnTrigger: Collider2D 컴포넌트를 찾을 수 없습니다! Is Trigger가 체크되어 있나요?", this);
        }
    }

    /// <summary>
    /// 이 오브젝트의 Collider2D(Is Trigger 체크됨)에
    /// 다른 Collider2D가 들어왔을 때 호출됩니다.
    /// </summary>
    /// <param name="other">나에게 닿은 대상의 Collider2D</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag != "Player")
        {
            return;
        }

        // 이미 페이드아웃 중이거나, SpriteRenderer가 없으면 아무것도 하지 않습니다.
        if (spriteRenderer == null || !gameObject.activeSelf) return;

        // 중복 트리거 방지를 위해 콜라이더를 비활성화합니다.
        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        // 페이드아웃 코루틴 시작
        StartCoroutine(FadeOutRoutine());

        // (선택 사항) 로그 출력
        //Debug.Log(gameObject.name + "가 " + other.name + "와(과) 닿아서 페이드아웃을 시작합니다.");
    }

    private IEnumerator FadeOutRoutine()
    {
        float timer = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // 알파 값을 0으로

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeOutDuration;
            spriteRenderer.color = Color.Lerp(startColor, endColor, progress); // 색상 보간
            yield return null; // 다음 프레임까지 대기
        }

        // 완전히 투명해진 후 최종 색상 설정 (혹시 모를 잔여값 처리)
        spriteRenderer.color = endColor;

        // 페이드아웃 후 오브젝트 처리
        if (destroyOnFadeOut)
        {
            Destroy(gameObject); // 오브젝트 파괴
            //Debug.Log(gameObject.name + "가 페이드아웃 후 파괴되었습니다.");
        }
        else
        {
            gameObject.SetActive(false); // 비활성화
            //Debug.Log(gameObject.name + "가 페이드아웃 후 비활성화되었습니다.");
        }
    }
}