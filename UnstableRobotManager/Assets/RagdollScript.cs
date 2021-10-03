using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollScript : MonoBehaviour
{

    public List<Rigidbody2D> rb;
    public BoxCollider2D DeathCollider;
    
    float MyFloorY = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        MyFloorY = transform.position.y - 1F;
    }

    
}
