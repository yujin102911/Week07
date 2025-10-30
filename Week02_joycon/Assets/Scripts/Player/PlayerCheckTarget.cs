using UnityEditor;
using UnityEngine;

public class PlayerCheckTarget : MonoBehaviour
{
    private LayerMask carryableMask;
    private Vector2 playerSize;
    ObjectHighlight highlight;//현재 하이라이트 된거
    ObjectHighlight highlighted;//하이라이트 했던거 끄기 용

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        carryableMask = LayerMask.GetMask(PlayerConstant.CarryableMask);
        playerSize = GetComponent<BoxCollider2D>().bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        var area = BuildForwardBox();
        Collider2D[] hits = Physics2D.OverlapBoxAll(area.pos, area.size, 0f, carryableMask);

        Carryable closest = FindClosestCarryable(hits);
        if (closest == null) { highlight = null; }
        else { closest.TryGetComponent(out highlight); }
        Debug.Log(highlight);
        if (highlighted != highlight)//이전 하이라이트랑 다르면
        {
            if (highlighted != null) highlighted.color = Color.black;//이전 하이라이트 끄기
            if (highlight != null) highlight.SwitchColor();//현재 하이라이트 색 바꾸기
            highlighted = highlight;//마지막 하이라이트 갱신
        }

        /*if (closest == null) return false;
        if (closest.GetIsCarried() == true) return false;



        return true;*/
    }
    private (Vector2 pos, Vector2 size) BuildForwardBox()
    {
        var pickUpPos = transform.position;
        pickUpPos.x += PlayerConstant.InteractableRange / 2f * Player.GetFaceDir();
        var pickUpBox = new Vector2(PlayerConstant.InteractableRange, playerSize.y * 1.1f);
        return (pickUpPos, pickUpBox);
    }
    private Carryable FindClosestCarryable(Collider2D[] hits)
    {
        Carryable closest = null;
        float minD = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!hit) continue;
            if (hit.TryGetComponent(out Carryable carryable) == false) continue;
            if (carryable.enabled == false || carryable.GetIsCarried() == true) continue;

            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < minD) { minD = d; closest = carryable; }

        }
        return closest;
    }
}
