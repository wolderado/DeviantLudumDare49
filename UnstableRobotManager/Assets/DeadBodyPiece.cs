using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;
using Random = UnityEngine.Random;

public class DeadBodyPiece : MonoBehaviour
{
    public bool CanBeDetachable = true;
    public bool Detached = false;
    public string PieceName = "Torso";
    public Rigidbody2D rb;

    public float DetachUpForce = 10;
    public float DetachDirectForce = 30;

    public HurtableObject hurtPiece;
    public ParticleSystem DetachParticle;
    public ParticleSystem smallSplatterPrefab;


    public Rigidbody2D masterRB;
    
    // Start is called before the first frame update
    void Start()
    {
        hurtPiece.OnHurt += BreakBodyPiece;
    }

    private void OnDisable()
    {
        hurtPiece.OnHurt -= BreakBodyPiece;
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    public void ProcessHit(HurtableObject originalPiece,Vector2 pushDir)
    {
        if (PieceName == originalPiece.PieceName)
        {

            DetachPiece(pushDir);
        }
    }

    public void DetachPiece(Vector2 pushDir)
    {
        Vector2 force = (pushDir.normalized * Random.Range(0F, DetachDirectForce)) + (Vector2.up * Random.Range(-DetachUpForce, DetachUpForce));
        
        if (Detached == false && CanBeDetachable)
        {
            DetachParticle.Play();
            Detached = true;
            transform.parent = null;

            rb.isKinematic = false;

            rb.AddForce(force, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-0.25F, 0.25F), ForceMode2D.Impulse);
            
            if(masterRB != null)
                masterRB.AddForce(-force * 0.5F,ForceMode2D.Impulse);
        }
        
        
        Instantiate(smallSplatterPrefab, transform.position, UnityEngine.Quaternion.identity);
    }
    

    void BreakBodyPiece(Vector2 pos, Vector2 origin, LineIK atkArm)
    {
        Vector2 dmgDir = origin - (Vector2)transform.position;
        ProcessHit(hurtPiece, dmgDir);
    }
}
