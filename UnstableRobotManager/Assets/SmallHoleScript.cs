using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallHoleScript : MonoBehaviour
{
    public float DestroyTime;
    public SpriteRenderer spr;

    private float MaxTime;
    private Color sprCol;
    private float defaultAlpha;
    public float AlphaVariation = 0.1F;
    
    void Start()
    {

        MaxTime = DestroyTime;
        sprCol = spr.color;
        defaultAlpha = sprCol.a + Random.Range(-AlphaVariation, AlphaVariation);
    }
    
    void Update()
    {
        DestroyTime -= Time.deltaTime;
        sprCol.a = (DestroyTime / MaxTime) * defaultAlpha;
        spr.color = sprCol;
        
        if(DestroyTime <= 0)
            Destroy(this.gameObject);
        
    }
}
