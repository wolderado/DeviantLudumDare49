using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimSpriteChanger : MonoBehaviour
{
    public SpriteRenderer rend;
    public Sprite[] WalkSprites;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeSprite(int sprIndex)
    {
        rend.sprite = WalkSprites[sprIndex];
    }
}
