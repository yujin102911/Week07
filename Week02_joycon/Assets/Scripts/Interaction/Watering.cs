using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class Watering : MonoBehaviour
{
    [SerializeField] GameObject activeObject;
    [SerializeField] Carryable carryable;
    [SerializeField] bool treeDetact;
    Vector2 dropOffset;
    void Start()
    {
        if (carryable == null)
            carryable = GetComponent<Carryable>();
    }
    void Update()
    {
        if (carryable.carrying)
        {
            dropOffset = new Vector3(1.5f * -transform.localScale.y, 0, 0);//내 y스케일에 따라 오른쪽이나 왼쪽
            Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)transform.position + dropOffset, new Vector2(0.25f, 0.5f), 0);
            if (hits.Length == 0)
            {
                Debug.Log("No hits detected.");
                activeObject.SetActive(false);
                return;
            }
            else
            {
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Tree"))
                    {
                        treeDetact = true;
                        break;
                    }
                    else
                    {
                        treeDetact = false;
                    }
                }
            }
        }
        if (treeDetact)
        {
            activeObject.SetActive(true);
        }
        else
        {
            activeObject.SetActive(false);
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + dropOffset, new Vector2(0.25f, 0.5f));
    }
}
