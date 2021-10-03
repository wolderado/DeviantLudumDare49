using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AgentAI : MonoBehaviour
{
    public float WalkSpeed = 2;
    public Node targetNode;
    public Queue<Node> currentPath;
    public AIState CurrentState;
    public float SeeDistance = 10;
    public bool CanSeePlayer = true;
    public SkinGenerator mySkin;
    public Animator anim;
    public AIBehaviour myBehaviour;
    public float ExtraAtkDistanceWalk = 0;
    public bool IsCivil = true;
    
    private Vector2 nodeOffset;
    private Vector2 targetPos;
    private Vector2 escapePos;
    private Vector2 afterNodeTargetPos;
    private bool AsignedLastMove = false;
    private float warningSignAlpha = 1F;
    public bool SawPlayerStable = false;

    public WorkNode myWorkNode;
    public float WorkTime = 0;
    public bool DontEscapeMap = false;
    public bool Unresponsive;
    
    public Job MyJob;
    public SpriteRenderer warningSign;
    public HurtableObject targetEntity;
    public WeaponScript myWeapon;


    private bool Died = false;

    [HideInInspector] public bool KilledBySpinningArms;
    
    private Vector2 externalVelocity;
    

    private float extraDistanceWalkTimer = 0;
    
    public delegate void onPlayerSightedDelegate(Vector2 pos);

    public event onPlayerSightedDelegate OnPlayerSighted;
    
    public delegate void onDeathDelegate();

    public event onDeathDelegate OnDeath;

    void Start()
    {
        currentPath = new Queue<Node>();

        if (CanSeePlayer)
            MasterAI.instance.AllDetectorAgents.Add(this);


        if (myWorkNode != null)
        {
            WorkTime = Random.Range(10F, 20F);
            MoveTo(myWorkNode.transform.position, true);
        }
        else
            CurrentState = AIState.Idle;
    }

    void Update()
    {
        if (CurrentState == AIState.Chase || CurrentState == AIState.Panicked)
        {
            ChaseAI();
        }
        else
        {
            if (CurrentState == AIState.Idle)
            {
                if (WorkTime > 0)
                    CurrentState = AIState.Working;
                
                warningSignAlpha = 1F;
                warningSign.enabled = false;
                anim.SetFloat("Walk", 0F);
            }
            else if (CurrentState == AIState.Working)
            {
                WorkAI();
            }
        }
        
        
        externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, Time.deltaTime * 8F);

        
    }

    void ChaseAI()
    {
        if (Unresponsive)
            return;
        
        anim.SetFloat("Walk",1F);
        float speed = WalkSpeed;
        if (myWeapon != null && myWeapon.TargetInRange)
        {
            extraDistanceWalkTimer -= Time.deltaTime;
            if (extraDistanceWalkTimer <= 0)
            {
                speed = 0;
                anim.SetFloat("Walk", 0F);
            }
        }
        else
            extraDistanceWalkTimer = ExtraAtkDistanceWalk;



        Vector2 dirToTargetPos = targetPos - (Vector2)transform.position;
        Vector2 moveVector = dirToTargetPos.normalized * Time.deltaTime * speed + externalVelocity;
        


        warningSign.enabled = true;
        Color newWarningColor = Color.white;
        if(CurrentState == AIState.Panicked)
            warningSign.color = new Color(1f, 0.97f, 0.31f,warningSignAlpha);
        else
            warningSign.color = new Color(1f, 0.25f, 0.27f,warningSignAlpha);


        warningSignAlpha = Mathf.Lerp(warningSignAlpha, 0.2F, Time.deltaTime);
        
        if (dirToTargetPos.SqrMagnitude() < 1)
        {
            if (targetNode != null)
            {
                if (currentPath.Count > 0)
                {
                    AsignedLastMove = false;
                    MoveToNextNode();
                }
                else
                {
                    if (AsignedLastMove == false)
                    {
                        targetPos = afterNodeTargetPos;
                        AsignedLastMove = true;
                    }
                    else
                    {
                        if (myBehaviour != AIBehaviour.Escape)
                        {
                            bool finalCheck = MasterAI.instance.CanAgentSeePlayer(this);

                            if (finalCheck == false)
                                ReturnToIdle();
                            else
                                PlayerSeen(PlayerController.instance.transform.position);
                        }
                        else
                        {
                            if(!DontEscapeMap)
                                DestroyAgent();
                            else
                            {
                                CurrentState = AIState.Idle;
                            }
                        }
                    }
                }
            }
            else
            {
                if (myBehaviour != AIBehaviour.Escape)
                {
                    bool finalCheck = MasterAI.instance.CanAgentSeePlayer(this);

                    if (finalCheck == false)
                        ReturnToIdle();
                }
            }
        }
        else
        {
            transform.position = (Vector2)transform.position + moveVector;
        }
    }


    void WorkAI()
    {
        if (Unresponsive)
            return;
        
        float speed = WalkSpeed;
        Vector2 dirToTargetPos = targetPos - (Vector2)transform.position;
        Vector2 moveVector = dirToTargetPos.normalized * Time.deltaTime * speed + externalVelocity;
        anim.SetFloat("Walk", 1F);
                
        if (dirToTargetPos.SqrMagnitude() < 1)
        {

            if (targetNode != null)
            {
                if (currentPath.Count > 0)
                {
                    AsignedLastMove = false;
                    MoveToNextNode();
                }
                else
                {
                    if (AsignedLastMove == false)
                    {
                        targetPos = afterNodeTargetPos;
                        AsignedLastMove = true;
                    }
                    else
                    {
                        if (WorkTime > 0)
                        {
                            anim.SetFloat("Walk", 0F);
                            WorkTime -= Time.deltaTime;
                            if (WorkTime <= 0)
                            {
                                MoveTo(MasterAI.instance.GiveMeEscapePoint(transform.position));
                                AsignedLastMove = false;
                                myWorkNode.IsAvaible = true;
                            }
                        }
                        else
                        {
                            DestroyAgent();
                        }
                    }
                }
            }
        }
        else
        {
            transform.position = (Vector2)transform.position + moveVector;
        }
    }

    public void PlayerSeen(Vector2 seenPosition)
    {
        if (myBehaviour != AIBehaviour.Escape)
        {
            //targetNode = null;

            if (CurrentState == AIState.Idle || CurrentState == AIState.Working)
            {
                bool AttackPerm = (PlayerController.instance.CurrentStability == PlayerController.Stability.Unstable);

                if (SawPlayerStable)
                    AttackPerm = true;

                if (AttackPerm)
                {
                    AsignedLastMove = false;
                    myWeapon.ResetReactionTimer();
                    CurrentState = AIState.Chase;
                    MoveTo(seenPosition);
                }
            }
            else if (CurrentState == AIState.Chase)
            {
                MoveDirectlyTo(seenPosition);
            }

            targetEntity = PlayerController.instance.myHurtable;
        }
        else
        {
            if (CurrentState != AIState.Panicked)
            {
                CurrentState = AIState.Panicked;
                MakeAgentPanic();
            }
        }


        OnPlayerSighted?.Invoke(seenPosition);
    }

    public void ReturnToIdle()
    {
        targetEntity = null;
        CurrentState = AIState.Idle;
        
        if(WorkTime > 0)
            MoveTo(myWorkNode.transform.position,true);
        else
        {
            if(!DontEscapeMap)
                MoveTo(MasterAI.instance.GiveMeEscapePoint(transform.position),true);
        }
    }

    void MoveTo(Vector2 pos,bool withoutSight = false)
    {
        currentPath.Clear();
        currentPath = Pathfinder.instance.FindPathTo(pos,transform.position,withoutSight);
        afterNodeTargetPos = pos;

        MoveToNextNode();
    }

    void MoveToNextNode()
    {
        targetNode = currentPath.Dequeue();
        nodeOffset = targetNode.GiveMeRandomOffset();
        targetPos = (Vector2)targetNode.transform.position + nodeOffset;
        
    }
    
    
    void MoveDirectlyTo(Vector2 pos)
    {
        targetPos = pos;
    }

    public void Hurt(HurtableObject bodyPiece,Vector3 damageDir,Vector2 hurtPos,LineIK AttackedArm)
    {
        if (Died)
            return;
        
        if (bodyPiece.Health <= 0)
        {
            if(WorkTime > 0)
                myWorkNode.IsAvaible = true;
            
            Died = true;
            DeathManager.instance.CreateFullBody(this,damageDir, mySkin,hurtPos,bodyPiece,AttackedArm);
        }
    }


    void MakeAgentPanic()
    {
        if (!DontEscapeMap)
        {
            escapePos = MasterAI.instance.GiveMeEscapePoint(transform.position);
            MoveTo(escapePos);
        }
        else
        {
            escapePos = Pathfinder.instance.nodes[Random.Range(0, Pathfinder.instance.nodes.Count)].transform.position;
            MoveTo(escapePos);
        }
    }

    public void Push(Vector2 force)
    {
        externalVelocity += force;
    }
    
    public void DestroyAgent()
    {
        if(CanSeePlayer)
            MasterAI.instance.AllDetectorAgents.Remove(this);

        if (WorkTime > 0)
            myWorkNode.IsAvaible = true;

        if (IsCivil)
            MasterAI.instance.CivilAgents.Remove(this);
        else
        {
            MasterAI.instance.SecurityAgents.Remove(this);
        }
        
        OnDeath?.Invoke();
        
        Destroy(this.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position,SeeDistance);
    }
    


    public enum AIState
    {
        Idle,
        Chase,
        Working,
        Panicked
    }
    
    public enum AIBehaviour
    {
        AtkClose,
        Escape,
        AtkDistant
    }


    public enum Job
    {
        Civil,
        Security,
        VIP
    }
}
