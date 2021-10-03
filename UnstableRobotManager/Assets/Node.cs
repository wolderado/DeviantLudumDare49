using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Node : MonoBehaviour
{
    public Node[] Neighbours;
    public float WalkRadius = 4F;
    
    
    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (Pathfinder.instance != null)
        {
            if (Pathfinder.instance.VisualizeNodes == false)
                return;
        }
        
        Gizmos.color = new Color(1f, 0.67f, 0.96f);
        if(Neighbours.Contains(this))
            Gizmos.color = new Color(1f, 0.03f, 0f);
        Gizmos.DrawSphere(transform.position,0.2F);
        Gizmos.color = new Color(1f, 0.67f, 0.96f);
        
        for (int i = 0; i < Neighbours.Length; i++)
        {
            Gizmos.color = new Color(1f, 0.67f, 0.96f);
            if(Neighbours[i].Neighbours.Contains(this) == false)
                Gizmos.color = new Color(1f, 0.24f, 0.18f);
            Gizmos.DrawLine(transform.position,Neighbours[i].transform.position);
        }
        
        
        Gizmos.color = new Color(1f, 0.65f, 0.93f, 0.4f);
        Gizmos.DrawSphere(transform.position,WalkRadius);
    }

    public Vector2 GiveMeRandomOffset()
    {
        return (Random.insideUnitCircle * WalkRadius);
    }
}
