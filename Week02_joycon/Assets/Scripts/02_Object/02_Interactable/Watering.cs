using UnityEngine;

public class Watering : MonoBehaviour
{
    [SerializeField] GameObject activeObject;
    [SerializeField] Carryable carryable;
    [SerializeField] bool treeDetact;
    Vector2 wateringOffset;
    Vector2 wateringSize;
    void Start()
    {
        if (carryable == null)
            carryable = GetComponent<Carryable>();
    }
    void Update()
    {
        if (carryable.GetIsCarried() == true)
        {
            wateringOffset = new Vector3(1.5f * transform.localScale.y, 0, 0);//내 y스케일에 따라 오른쪽이나 왼쪽
            wateringSize = new Vector2(transform.localScale.x , Mathf.Abs(transform.localScale.y));
            Collider2D[] hits = Physics2D.OverlapBoxAll((Vector2)transform.position + wateringOffset, wateringSize, 0);
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
        else
        {
            treeDetact = false;
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
        Gizmos.DrawWireCube((Vector2)transform.position + wateringOffset, wateringSize);
    }
}
