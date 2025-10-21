using Unity.Burst;
using UnityEngine;

public enum InteractionKind : byte
{
    None,
    Press,          // Ű ������ �� ���
    Hold,           // ���� �ð� Ȧ��
    EnterArea,      // Ʈ���� ������(��ĳ�� ���� Ʈ���Ű� Raise)
    UseItem         // Ư�� ������ ���� ���� ��
}

[DisallowMultipleComponent]
public sealed class Interactable2D : MonoBehaviour
{
    [Header("ID / Type")]
    [SerializeField] string id = "Box_A";
    [SerializeField] InteractionKind kind = InteractionKind.Press;

    [Header("Hold Option")]
    [SerializeField, Min(0f)] float requiredHoldSeconds = 0.8f; // 0�̸� ����
    [SerializeField] bool cancelOnMove = false;


    [Header("Stay Option")]
    [SerializeField, Min(0f)] float requiredStaySeconds = 0.8f; // 0�̸� ����



    [Header("prompt")]
    [SerializeField] string promptText = "��ȣ�ۿ�";
    [SerializeField] Transform promptAnchor;            // ������ this
    [SerializeField] Vector3 promptOffset = new Vector3(0, 1f, 0);

    [Header("Preconditions (all satisfied if any)\r\n")]
    [SerializeField] string[] requiredEventFlags;
    [SerializeField] string requiredItemId;             // �� ���ڿ��̸� ����

    public string Id => id;
    public InteractionKind Kind => kind;
    public float RequiredHoldSeconds => requiredHoldSeconds;
    public float RequiredStaySeconds => requiredStaySeconds;
    public bool CancelOnMove => cancelOnMove;

    public bool ActiveToDestory = false;

    public Transform PromptAnchor => promptAnchor ? promptAnchor : transform;
    public Vector3 PromptOffset => promptOffset;
    public string PromptText => promptText;



    public bool HasRequiredFlags(System.Func<string, bool> flagChecker)
    {
        if (requiredEventFlags == null || requiredEventFlags.Length == 0) return true;
        if (flagChecker == null) return false;
        for (int i = 0; i < requiredEventFlags.Length; ++i)
            if (!flagChecker(requiredEventFlags[i])) return false;
        return true;
    }
    public bool CheckItem(System.Func<string, bool> hasItem)
        => string.IsNullOrEmpty(requiredItemId) || (hasItem != null && hasItem(requiredItemId));




    /*   float ComputeScore(Interactable2D it, Vector2 pos, Vector2 facing)
       {
           Vector2 to = (Vector2)it.transform.position - pos;
           float d2 = to.sqrMagnitude + 0.0001f;
           float score = d2 * it.PriorityBias;

           // ���� ����(����)
           if (it.MinFacingDot > -0.999f)
           {
               float dot = Vector2.Dot(facing.normalized, to.normalized);
               if (dot < it.MinFacingDot) return float.PositiveInfinity;
               // �����ϼ��� ����ġ��
               score *= Mathf.Lerp(2f, 0.5f, (dot + 1f) * 0.5f);
           }

           // �þ�(����)
           if (it.LOSBlockMask != 0 && !HasLOS(pos, it.transform.position, it.LOSBlockMask))
               score *= 10f; // �г�Ƽ

           // ����Ʈ ���ü�(������ ����ġ��)
           if (QuestRuntime.IsRelevantTarget(it.Id)) score *= 0.5f;

           return score;
       }

       bool HasLOS(Vector2 from, Vector2 to, LayerMask block)
       {
           Vector2 dir = to - from; float dist = dir.magnitude;
           return dist <= 0.001f || !Physics2D.Raycast(from, dir / dist, dist, block);
       }

     [SerializeField] float hysteresis = 1.15f; // 15% �� �������� ������ ����
   Interactable2D _stickyTarget; float _stickyScore;

   Interactable2D ApplyHysteresis(Interactable2D best, float bestScore)
   {
       if (_stickyTarget && _stickyTarget == best) { _stickyScore = bestScore; return best; }
       if (_stickyTarget && _stickyScore * hysteresis < bestScore) return _stickyTarget;
       _stickyTarget = best; _stickyScore = bestScore; return best;
   }


     */
}
