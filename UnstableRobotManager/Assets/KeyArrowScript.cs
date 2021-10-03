using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyArrowScript : MonoBehaviour
{
    public byte keyID = 0;
    
    
    void Update()
    {
        Vector2 targetPos = Vector2.zero;
        
        if (keyID == 0)
        {
            if (MasterAI.instance.CR_VIPGreen != null)
                targetPos = MasterAI.instance.CR_VIPGreen.transform.position;
            else
            {
                Destroy(this.gameObject);
            }
        }
        
        if (keyID == 1)
        {
            if (MasterAI.instance.CR_VIPRed != null)
                targetPos = MasterAI.instance.CR_VIPRed.transform.position;
            else
            {
                Destroy(this.gameObject);
            }
        }
        
        if (keyID == 2)
        {
            if (MasterAI.instance.CR_VIPBlue != null)
                targetPos = MasterAI.instance.CR_VIPBlue.transform.position;
            else
            {
                Destroy(this.gameObject);
            }
        }
        
        Vector2 toMouse = targetPos - (Vector2)transform.position;
        transform.up = toMouse;
    }
}
