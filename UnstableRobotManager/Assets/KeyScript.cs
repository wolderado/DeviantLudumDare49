using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class KeyScript : MonoBehaviour
{
    public AgentAI connectedAgent;
    public bool Detached = false;
    public string KeyID;

    private float DropY = 0;
    
    
    
    void Start()
    {
        connectedAgent.OnDeath += DropKey;
    }

    private void Update()
    {
        if (Detached)
        {
            if (transform.position.y > DropY)
                transform.position += Vector3.down * Time.deltaTime * 4F;

            if (Vector2.Distance((Vector2)transform.position, PlayerController.instance.transform.position) < 2F)
            {
                GameObject.Find("FoundTxt").GetComponent<TextMeshProUGUI>().text += KeyID + "\n ";
                PlayerController.instance.foundKeys.Add(KeyID);
                Destroy(this.gameObject);
            }
        }
    }

    public void DropKey()
    {
        transform.parent = null;
        DropY = connectedAgent.transform.position.y;
        Detached = true;
    }

}
