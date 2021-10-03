using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkNode : MonoBehaviour
{
    public bool IsAvaible = true;
    public bool IsCivil = true;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if(IsCivil)
            Gizmos.color = new Color(0.29f, 0.87f, 0.27f, 0.41f);
        else
        {
            Gizmos.color = new Color(0.05f, 0.24f, 0.87f, 0.41f);
        }
        
        Gizmos.DrawSphere(transform.position,0.5F);
    }
}
