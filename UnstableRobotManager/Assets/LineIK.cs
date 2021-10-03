using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class LineIK : MonoBehaviour
{
    [Header("Variables")]
    public int BoneCount = 5;
    public float LineDist = 3;
    public float AttackDuration = 0.2F;
    public float HoldDuration = 0.5F;
    public float RecoverDuration = 1.0F;
    public float FollowThroughDist = 0.2F;


    
    [Header("Runtime")]
    public bool AvaibleForNextAttack = true;
    public bool SmoothArm;
    public bool FollowGuide = false;
    public BodyScript StuckedBody;
    
    [Header("References")]
    public LineRenderer Line;
    public Transform UpperTr;
    public Transform ForwardTr;
    public GameObject holePrefab;
    public GameObject wallHolePrefab;
    public GameObject atkParticlePrefab;
    
    
    float boneLength;
    private float AtkTimer = -1;
    private Vector2 atkPos;
    Vector2 defaultForwardOffset;
    Vector2 defaultUpperOffset;
    private bool LimitedByBones = true;
    private Vector2 attackLocalForwardPos;
    private Vector2 attackLocalUpperPos;
    private Vector2 beforeAttackUpperPos;
    private Vector2 beforeAttackForwardPos;
    private float defaultBoneLength;
    private Vector2 rndUpperPointAtkOffset;
    private bool checkHitMoment;
    private int stuckedBoneIndex;
    private float stuckedBodyDropTimer;
    
    
    public Transform GuideForward;
    public Transform GuideUp;

    private Vector3 defaultForwardPos;
    private Vector3 defaultUpperPos;
    
    [HideInInspector]
    public GameObject AttackingWall ;
    
    [HideInInspector]
    public HurtableObject HurtTarget;


    private float defaultZ;
    
    // Start is called before the first frame update
    void Start()
    {
        defaultForwardOffset = ForwardTr.localPosition;
        defaultUpperOffset = UpperTr.localPosition;

        defaultZ = transform.position.z;
        
        SetupLine();
    }

    // Update is called once per frame
    void Update()
    {
        if (AtkTimer >= 0)
        {
            AtkTimer += Time.deltaTime;

            Vector2 dirToAtk = (atkPos - (Vector2)transform.position);
            Vector2 upperMiddlePos = (dirToAtk)/2F;
            upperMiddlePos = atkPos + rndUpperPointAtkOffset;

            if (AtkTimer <= AttackDuration)
            {
                SmoothArm = false;
                LimitedByBones = false;
                ForwardTr.transform.position =
                    Vector2.Lerp(beforeAttackForwardPos, atkPos, AtkTimer / AttackDuration);
                
                    attackLocalForwardPos = ForwardTr.localPosition;

                

                
                UpperTr.transform.position =
                    Vector2.Lerp(beforeAttackUpperPos, upperMiddlePos, AtkTimer / AttackDuration);

                attackLocalUpperPos = UpperTr.localPosition;
                boneLength = defaultBoneLength * 2;
                checkHitMoment = true;
            }
            else
            {
                if (checkHitMoment)
                {
                    if (HurtTarget != null)
                    {
                        HurtTarget.HurtMe(atkPos,transform.position,this,1);
                        HurtTarget = null;
                    }
                    
                    PlayHitFX();
                    checkHitMoment = false;
                }
                
                float withoutAtkTimer = AtkTimer - AttackDuration;
                if (withoutAtkTimer < HoldDuration)
                {
                    SmoothArm = false;
                    LimitedByBones = false;

                    ForwardTr.transform.position= Vector2.Lerp(atkPos, atkPos + dirToAtk * FollowThroughDist, withoutAtkTimer / HoldDuration);
                }
                else
                {
                    SmoothArm = true;
                    float recoverRatio = (withoutAtkTimer - HoldDuration) / RecoverDuration;
                    boneLength = Mathf.Lerp(boneLength, defaultBoneLength, recoverRatio);
                    LimitedByBones = true;
                    ForwardTr.transform.localPosition =
                        Vector2.Lerp(attackLocalForwardPos, defaultForwardOffset, recoverRatio);
                    UpperTr.transform.localPosition =
                        Vector2.Lerp(attackLocalUpperPos, defaultUpperOffset, recoverRatio);
                }
            }

            if (AtkTimer > RecoverDuration+0.5F)
            {
                AtkTimer = -1;
                LimitedByBones = true;
                SmoothArm = true;
                AvaibleForNextAttack = true;
            }
        }

        
        Line.SetPosition(0,transform.position);
        
        Vector2 rootPos = transform.position;
        Vector2 upperPoint = UpperTr.position;
        Vector2 forwardPoint = ForwardTr.position;

        if (FollowGuide)
        {
            forwardPoint = GuideForward.position;
            upperPoint = GuideUp.position;
            SmoothArm = false;
            LimitedByBones = false;
        }
        
        
        for (int i = 1; i < BoneCount; i++)
        {
            float ratio = (float)i / (float)BoneCount;
            Vector2 myPos = (Vector2)Line.GetPosition(i);
            Vector2 prevPos = Line.GetPosition(i - 1);


            Vector3 finalPos = myPos;
            /*if (BoneActive)
            {
                Vector2 dir = myPos - prevPos;
                dir = dir.normalized;
                Vector2 bonePos = prevPos + (dir * boneLength);
                finalPos = Vector2.MoveTowards(myPos,bonePos , Time.deltaTime * 16F);
            }
            */


            Vector2 bezierPos = ApplyBezierCurve(rootPos, upperPoint, forwardPoint, ratio);
            Vector2 toBezier = Vector2.Lerp(myPos, bezierPos, Time.deltaTime * 8F * Mathf.Clamp((1F-ratio),0.5F,1F) );

            if (!SmoothArm)
                toBezier = bezierPos;

            if (LimitedByBones)
            {
                Vector2 dir = (toBezier - prevPos).normalized;
                toBezier = prevPos + (dir * boneLength);
            }

            finalPos = toBezier;




            if(StuckedBody == null)
                finalPos.z = defaultZ;
            else
            {
                finalPos.z = defaultZ + 1 * ratio;
            }
            
            Line.SetPosition(i, finalPos);
        }


        if (StuckedBody != null)
        {
            StuckedBody.transform.position = Line.GetPosition(stuckedBoneIndex);

            stuckedBodyDropTimer -= Time.deltaTime;
            if (stuckedBodyDropTimer <= 0)
                DropStuckedBody();
        }
    }

    void SetupLine()
    {
        Line.positionCount = BoneCount;
        Vector2 linePos = transform.position; 
        boneLength = LineDist / BoneCount;
        defaultBoneLength = boneLength;
        for (int i = 0; i < BoneCount; i++)
        {
            linePos.y += boneLength;
            Line.SetPosition(i,linePos);
        }
    }


    Vector2 ApplyBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2,float t)
    {
        Vector2 firstLine = Vector2.Lerp(p0, p1, t);
        Vector2 secondLine = Vector2.Lerp(p1, p2, t);
        return Vector2.Lerp(firstLine, secondLine, t);
    }

    void PlayHitFX()
    {
        Vector2 dirToAtk = (atkPos - (Vector2)transform.position);
        CameraScript.instance.Shake(Random.Range(0.3F,0.4F));

        if (AttackingWall != null)
        {
            GameObject newWallHole = Instantiate(wallHolePrefab, atkPos, Quaternion.identity);
            
            Vector2 dirFromWallToAtk = (atkPos - (Vector2)AttackingWall.transform.position);
            newWallHole.transform.right = dirFromWallToAtk;
            
            var vec = newWallHole.transform.eulerAngles;
            vec.z = Mathf.Round(vec.z / 90) * 90;
            newWallHole.transform.eulerAngles = vec;

            AttackingWall = null;
        }
        else
            Instantiate(holePrefab, atkPos, Quaternion.identity);
        
        
        
        GameObject newParticle = Instantiate(atkParticlePrefab, atkPos, Quaternion.identity);
        newParticle.transform.Translate(0,0,-2);

        newParticle.transform.right = dirToAtk;
        
        PlayerController.instance.PushPlayer(-dirToAtk.normalized * 0.02F);
    }

    public void Attack(Vector3 pos)
    {
        AvaibleForNextAttack = false;
        beforeAttackUpperPos = UpperTr.position;
        beforeAttackForwardPos = ForwardTr.position;
        SmoothArm = false;
        AtkTimer = 0F;
        atkPos = pos;
        rndUpperPointAtkOffset = new Vector2(Random.Range(-4F, 2), Random.Range(-2F, 2F));

        DropStuckedBody(true);
    }


    void DropStuckedBody(bool byAttack = false)
    {
        if (StuckedBody != null)
        {
            StuckedBody.transform.Translate(0, 0, 1);
            StuckedBody.DropBodyFromArm(PlayerController.instance.transform.position.y - 1F);


            if (byAttack)
            {
                Vector2 atkDir = atkPos - (Vector2)transform.position;
                StuckedBody.masterRb.AddForce(atkDir * Random.value * 10, ForceMode2D.Impulse);
            }
            
            StuckedBody = null;
        }
    }


    public void MakeBodyStuckToThisArm(BodyScript body)
    {
        StuckedBody = body;

        stuckedBoneIndex = Random.Range(BoneCount - 10, BoneCount - 1);
        stuckedBodyDropTimer = Random.Range(1F, 3F);
    }


    public void MakeArmFollowGuide()
    {
        SmoothArm = false;
        LimitedByBones = false;
        FollowGuide = true;
    }
    
    public void RelaseArmFromGuide()
    {
        SmoothArm = true;
        LimitedByBones = true;
        FollowGuide = false;
    }
}
