using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponScript : MonoBehaviour
{
    public AgentAI connectedAI;
    public bool RotateIn180Degrees = false;
    public float AttackDistance;
    public bool TargetInRange;
    public string AttackAnimCode = "WeaponSwing";
    public bool CanAttack = true;
    public Animator anim;
    public float AttackCooldown = 2;
    public bool RaycastAttack = true;
    public LayerMask RaycastHitLayer;
    public float Damage = 1;
    public bool LooksAtTarget = true;
    public Projectile bulletPrefab;
    public Transform ProjectileSpawnPoint;
    public float PushSelfForce = 1;
    public ParticleSystem shootParticle;
    public ParticleSystem shootParticle2;
    
    Vector3 defaultLocalPosition;
    private float nextAtkTime;
    private float gunSwitchCooldown;
    float ReactionTimer;
    
    
    void Start()
    {
        defaultLocalPosition = transform.localPosition;
    }
    
    void Update()
    {
        if (connectedAI.targetEntity != null)
        {
            Vector2 dirToTarget = connectedAI.targetEntity.transform.position - transform.position;

            if(LooksAtTarget)
                transform.right = dirToTarget.normalized;
            
            
            if (RotateIn180Degrees)
            {
                if (connectedAI.targetEntity.transform.position.x+0.1F > connectedAI.transform.position.x)
                {
                    transform.localPosition = defaultLocalPosition;
                    transform.localScale = Vector3.one;
                }
                else if (connectedAI.targetEntity.transform.position.x-0.1F < connectedAI.transform.position.x)
                {
                    transform.localPosition = new Vector3(-defaultLocalPosition.x,defaultLocalPosition.y,defaultLocalPosition.z);
                    transform.localScale = new Vector3(-1F, 1F, 1F);
                    
                    
                    if(LooksAtTarget)
                        transform.right = -dirToTarget.normalized;
                }
            }

            float distToPlayer = Vector2.Distance(transform.position, connectedAI.targetEntity.transform.position);

            TargetInRange = (distToPlayer < AttackDistance);
            CanAttack = nextAtkTime < Time.time;
            if (TargetInRange && CanAttack)
            {
                if (ReactionTimer <= Time.time)
                {
                    Attack();
                }
            }
        }
        else
        {
            transform.right = Vector2.right;
        }
    }

    public void ResetReactionTimer()
    {
        ReactionTimer = Time.time + Random.Range(0.3F,0.6F);
    }

    void Attack()
    {
        nextAtkTime = Time.time + AttackCooldown;
        anim.Play(AttackAnimCode,-1,0);

    }


    public void RaycastMoment()
    {
        Vector2 dirToTarget = connectedAI.targetEntity.transform.position - transform.position;

        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dirToTarget,AttackDistance+0.1F,RaycastHitLayer);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null)
            {
                HurtableObject hurtObject = hit.collider.GetComponent<HurtableObject>();
                if (hurtObject != null)
                {
                    if (hurtObject.parentAgent != connectedAI)
                    {

                        connectedAI.Push(-dirToTarget.normalized * PushSelfForce);
                            
                        if (hit.collider.CompareTag("Player"))
                        {
                            PlayerController.instance.Hurt(Damage,dirToTarget.normalized * 0.5F);
                        }
                        else
                        {
                            hurtObject.HurtMe(hit.point, transform.position, null,Damage);
                        }
                            
                        return;
                    }
                }
            }
        }
    }


    public void CreateProjectileMoment()
    {
        if (shootParticle != null)
        {
            shootParticle.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
            shootParticle.Play();
        }


        if (shootParticle2 != null)
        {
            shootParticle2.transform.localScale = new Vector3(transform.localScale.x, 1, 1);
            shootParticle2.Play();
        }
        
        

        Vector2 dirToTarget = connectedAI.targetEntity.transform.position - transform.position;
        
        Projectile newBullet = Instantiate(bulletPrefab, ProjectileSpawnPoint.position, ProjectileSpawnPoint.rotation);
        newBullet.damage = Damage;
        newBullet.originPos = connectedAI.transform.position;
        newBullet.shooter = connectedAI;
        newBullet.transform.right = dirToTarget;
                        
        connectedAI.Push(-dirToTarget.normalized * PushSelfForce);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position,AttackDistance);
    }
}
