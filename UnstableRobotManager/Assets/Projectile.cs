using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float Speed = 0.5F;
    public float damage = 1;
    public float HitSuccessDistance = 2;
    public Vector2 originPos;
    public Rigidbody2D rb;
    public AgentAI shooter;
    public GameObject hitSparkParticle;
    public GameObject hitWallParticle;
    public GameObject wallHolePrefab;
    
    public HurtableObject targetHurtableObject;
    
    void Update()
    {
        rb.MovePosition((Vector2)rb.position + (Vector2)transform.right * Time.deltaTime * Speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HurtableObject hurtOther = other.GetComponent<HurtableObject>();
        if (hurtOther!= null)
        {
            if (hurtOther.parentAgent == shooter)
                return;
            
            hurtOther.HurtMe(transform.position, originPos, null, damage);
            
            GameObject newSpark = Instantiate(hitSparkParticle, transform.position, Quaternion.identity);
            newSpark.transform.right = transform.right;
        }
        else
        {
            GameObject newSpark = Instantiate(hitWallParticle, transform.position, Quaternion.identity);
            newSpark.transform.right = transform.right;
            
            GameObject newWallHole = Instantiate(wallHolePrefab, transform.position, Quaternion.identity);
            
            Vector2 dirFromWallToAtk = ((Vector2)transform.position - (Vector2)other.transform.position);
            newWallHole.transform.right = dirFromWallToAtk;
            
            var vec = newWallHole.transform.eulerAngles;
            vec.z = Mathf.Round(vec.z / 90) * 90;
            newWallHole.transform.eulerAngles = vec;
        }


        
        Destroy(this.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position,HitSuccessDistance);
    }
}
