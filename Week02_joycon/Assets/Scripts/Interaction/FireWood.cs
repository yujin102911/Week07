using System;
using System.Collections.Generic;
using UnityEngine;

public class FireWood : MonoBehaviour
{
    [SerializeField]BoxCollider2D boxCollider2D;
    float boxCollider2Dx;
    [SerializeField] GameObject[]Prefabs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (boxCollider2D==null)
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
        }
        boxCollider2Dx= boxCollider2D.bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name + "닿음");
        Transform[] childs = collision.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in childs)
        {
            if (t.gameObject.CompareTag("Axe"))
            {
                if (t.GetComponent<Axe>().falling == true)//떨어지는 중일 때만
                {
                    for (int i = 0; i < Prefabs.Length; i++)
                    {
                        if (Prefabs[i] != null)
                        {
                            Instantiate(Prefabs[i],
                                new Vector2(transform.position.x - boxCollider2Dx / 2 * Prefabs.Length + boxCollider2Dx * i, transform.position.y), 
                                Quaternion.Euler(0,0,transform.localRotation.z+(-90+30-60*i)));
                        }
                    }
                    Destroy(gameObject);
                }
            }
        }
    }
}
