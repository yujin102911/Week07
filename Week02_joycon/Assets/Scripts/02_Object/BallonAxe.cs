using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class BallonAxe : MonoBehaviour
{
    [SerializeField] GameObject handle;
    [SerializeField] ParticleSystem popEffect;
    [SerializeField] bool destroy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var main = popEffect.main;

        // 2. main 모듈의 startColor 속성을 변경합니다.
        main.startColor = GetComponent<SpriteRenderer>().color;
    }

    // Update is called once per frame
    void Update()
    {
        if (destroy)
        {
            // 1. 파티클이 존재하는지 확인합니다.
            if (popEffect != null)
            {
                // 2. 파티클을 부모 객체로부터 분리합니다.
                // 이렇게 하면 부모가 파괴되어도 파티클은 씬에 남아있게 됩니다.
                popEffect.transform.SetParent(null);

                // 3. 파티클을 재생합니다.
                popEffect.Play();

                // 4. (매우 중요!) 유니티 에디터에서 popEffect 파티클 시스템의
                // Main 모듈을 선택하고 'Stop Action' 속성을 'Destroy'로 설정하세요.
                // 
                // 이렇게 해야 파티클 재생이 끝난 후 자동으로 파괴되어 메모리 누수를 막습니다.
            }

            // 5. 이제 나머지 오브젝트들을 파괴합니다.
            Destroy(handle);
            Destroy(transform.parent.gameObject);

            // 6. 이 스크립트 컴포넌트도 즉시 비활성화하여 
            // Update가 중복 실행되는 것을 막습니다. (선택 사항이지만 권장)
            this.enabled = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform[] childs = collision.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in childs)
        {
            if (t.CompareTag("Axe"))
            {
                //Instantiate(popEffect, transform.position, Quaternion.identity);
                destroy =true;
                return;
            }
        }
    }
    //private void OnDestroy()
    //{
    //    popEffect.Play();
    //}
}
