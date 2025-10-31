using System.Collections.Generic;
using UnityEngine;

public class Carrot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    { 
    
    }
        private void OnTriggerEnter2D(Collider2D collision)
    {
        Transform[] childs = collision.transform.GetComponentsInChildren<Transform>();
        foreach (Transform t in childs)
        {
            if (!t.TryGetComponent<Axe>(out Axe axe)) continue;
            if (axe.falling == true || axe.throwing)
            {
                GameObject carrot = Instantiate(new GameObject(), transform.position,Quaternion.identity);
                carrot.name = "carrots";
                carrot.layer = LayerMask.NameToLayer("Carryable");
                carrot.transform.SetParent(transform);
                carrot.AddComponent<Carrot>();
            }
        }
    }
}
