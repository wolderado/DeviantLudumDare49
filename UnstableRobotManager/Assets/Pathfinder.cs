using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class Pathfinder : MonoBehaviour
{
    public List<Node> nodes;
    public bool VisualizeNodes = true;
    public LayerMask SightMask;
    
    public static Pathfinder instance;

    private void Awake()
    {
        instance = this;
    }
    
    public void OnEnable()
    {
        instance = this;
    }

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }


    public Queue<Node> FindPathTo(Vector2 myPos,Vector2 targetPosition,bool WithoutSight = false)
    {
        //Depth First Search
        Node closestNodeToMyPos = GetClosestNodeFromPositionWithSight(myPos);
        Node closestNodeToTargetPos = GetClosestNodeFromPositionWithSight(targetPosition);
        
        if (WithoutSight)
        {
            closestNodeToMyPos = GetClosestNodeFromPosition(myPos);
            closestNodeToTargetPos = GetClosestNodeFromPosition(targetPosition);
        }
        
        

        Queue<Node> frontier = new Queue<Node>();
        frontier.Enqueue(closestNodeToMyPos);
        Dictionary<Node,Node> camefrom = new Dictionary<Node,Node>();
        camefrom[closestNodeToMyPos] = null;
        
        Node current ;

        while (frontier.Count > 0)
        {
            current = frontier.Dequeue();

            if (current == closestNodeToTargetPos)
                break;

            foreach (Node next in current.Neighbours)
            {
                if (camefrom.ContainsKey(next) == false)
                {
                    frontier.Enqueue(next);
                    camefrom[next] = current;
                }
            }
        }


        List<Node> debugPrint = new List<Node>();
        Queue<Node> finalPath = new Queue<Node>();
        current = closestNodeToTargetPos;
        int overflow = 100;
        while (current != closestNodeToMyPos)
        {
            finalPath.Enqueue(current);
            debugPrint.Add(current);
            current = camefrom[current];

            overflow--;
            if (overflow < 0)
            {
                Debug.LogError("Overflow while calculating path!");
                break;
            }
        }

        finalPath.Enqueue(closestNodeToMyPos);
        //finalPath.Reverse();
        
        debugPrint.Add(closestNodeToMyPos);
        //debugPrint.Reverse();

        /*for (int i = 0; i < debugPrint.Count; i++)
        {
            Debug.Log($"Path[{i}]:{debugPrint[i].name}");
        }*/
        
        return finalPath;
    }
    
    Node GetClosestNodeFromPositionWithSight(Vector2 pos)
    {
        Node closestNode = null;
        float dist;
        float closestDist = Mathf.Infinity;
        for (int i = 0; i < nodes.Count; i++)
        {
            dist = Vector2.SqrMagnitude((Vector2)nodes[i].transform.position - pos);
            RaycastHit2D hit = Physics2D.Raycast(pos, (Vector2)nodes[i].transform.position - pos,dist,SightMask);

            if (hit.collider == null)
            {
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestNode = nodes[i];
                }
            }
        }

        return closestNode;
    }
    

    Node GetClosestNodeFromPosition(Vector2 pos)
    {
        Node closestNode = null;
        float dist;
        float closestDist = Mathf.Infinity;
        for (int i = 0; i < nodes.Count; i++)
        {
            dist = Vector2.SqrMagnitude((Vector2)nodes[i].transform.position - pos);

            if (dist < closestDist)
            {
                closestDist = dist;
                closestNode = nodes[i];
            }
        }

        return closestNode;
    }

#if UNITY_EDITOR
    [MenuItem("CustomMethods/Find All Pathfinding Nodes")]
#endif
    public static void FindAllNodes()
    {
        instance.nodes.Clear();

        GameObject[] AllNodes = GameObject.FindGameObjectsWithTag("Node");
        foreach (GameObject n in AllNodes)
        {
            instance.nodes.Add(n.GetComponent<Node>());
        }
        
    }
}
