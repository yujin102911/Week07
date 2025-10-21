using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public sealed class SpikeController : MonoBehaviour
{
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool oneShot = false;     // �� ���� �۵�
    [SerializeField] private float cooldown = 0f;      // ��ߵ� ��ٿ�(��)

    bool _used;
    float _last;

    void Reset()
    {
        // ������ũ �ݶ��̴��� Ʈ���ŷ�!
        var c = GetComponent<Collider2D>();
        c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_used && oneShot) return;
        if (cooldown > 0f && Time.time - _last < cooldown) return;
        if (!other.CompareTag(targetTag)) return;

        // PlayerRespawn2D_RuntimeSO ã�Ƽ� Die()
        // �켱 attachedRigidbody���� ã��, ������ ���� �������� ��� �˻�
        PlayerRespawn2D_RuntimeSO respawn = null;

        var rb2d = other.attachedRigidbody;
        if (rb2d) respawn = rb2d.GetComponent<PlayerRespawn2D_RuntimeSO>();
        if (!respawn) respawn = other.GetComponentInParent<PlayerRespawn2D_RuntimeSO>();

        if (respawn)
        {
            Debug.Log("Player Trigger");

            respawn.Die();
            _last = Time.time;
            if (oneShot) _used = true;
        }
    }
}
