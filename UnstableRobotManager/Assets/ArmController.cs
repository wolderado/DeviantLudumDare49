using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ArmController : MonoBehaviour
{
    public bool InGuideAnimation = false;
    public LineIK[] AttackArms;
    private Camera cam;
    public Transform DEBUG_ArmAtkPos;
    public float AtkDistance = 10;
    public LayerMask blockLayer;
    public LayerMask hurtLayer;
    public Animator GuideAnim;
    public ParticleSystem[] OpenArmParticles;
    public Transform GuideHolder;
    public Transform CutHole;


    void Start()
    {

        cam = Camera.main;
        CloseArms(true);
    }

    void Update()
    {


        if (PlayerController.instance.Died == false)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 toMouse = mousePos - (Vector2)transform.position;

            float angle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;
            for (int i = 0; i < AttackArms.Length; i++)
            {
                AttackArms[i].transform.localEulerAngles = new Vector3(0, 0, angle);
            }
            
            GuideHolder.transform.localEulerAngles = new Vector3(0, 0, angle);
            
            if (PlayerController.instance.CurrentStability == PlayerController.Stability.Unstable)
            {
                if (PlayerController.instance.WaitPlayer <= 0)
                {
                    if (Input.GetButtonDown("Fire1"))
                    {
                        if (Vector3.Distance(transform.position, mousePos) < AtkDistance)
                        {
                            Tuple<Vector2, GameObject> result = GetTheClosestObstacleOnAttack(mousePos);
                            HurtableObject hurtObject = CheckHit(result.Item1);
                            PierceAttack(result.Item1, result.Item2, hurtObject);
                        }
                    }
                }
            }
        }

        

        if (InGuideAnimation)
        {
            for (int i = 0; i < OpenArmParticles.Length; i++)
            {
                var s = OpenArmParticles[i].shape;
                s.position = OpenArmParticles[i].transform.InverseTransformPoint(transform.position);
                //OpenArmParticles[i].transform.position = transform.position;
            }
        }
    }


    Tuple<Vector2,GameObject> GetTheClosestObstacleOnAttack(Vector2 targetPos)
    {
        Vector2 atkDir = targetPos - (Vector2)transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, atkDir, atkDir.magnitude, blockLayer);
        if (hit.collider != null)
        {
            return Tuple.Create(hit.point,hit.collider.gameObject);
        }

        return Tuple.Create(targetPos,(GameObject)null);
    }

    HurtableObject CheckHit(Vector2 targetPos)
    {
        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.1F,hurtLayer);

        if (hit != null)
        {
            return hit.GetComponent<HurtableObject>();
        }

        return null;
    }


    void PierceAttack(Vector2 atkPos,GameObject hitWall = null,HurtableObject hitObject = null)
    {
        DEBUG_ArmAtkPos.position = atkPos;

        List<LineIK> avaibleAtkArms = new List<LineIK>();
        foreach (LineIK line in AttackArms.Where(p => p.AvaibleForNextAttack))
            avaibleAtkArms.Add(line);

        //Fisher-Yates Shuffle
        int n = avaibleAtkArms.Count;
        for (int i = 0; i < (n - 1); i++)
        {
            int r = Random.Range(0, i);
            (avaibleAtkArms[r], avaibleAtkArms[i]) = (avaibleAtkArms[i], avaibleAtkArms[r]);
        }

        if (avaibleAtkArms.Count > 0)
        {
            LineIK selectedArm = avaibleAtkArms[Random.Range(0, avaibleAtkArms.Count)];
            selectedArm.HurtTarget = hitObject;
            selectedArm.AttackingWall = hitWall;
            selectedArm.Attack(atkPos);
        }
    }


    public void OpenArms()
    {
        for (int i = 0; i < AttackArms.Length; i++)
        {
            AttackArms[i].MakeArmFollowGuide();
        }
        
        GuideAnim.Play("ArmOpen",-1,0);

        
        for (int i = 0; i < OpenArmParticles.Length; i++)
        {
            OpenArmParticles[i].transform.position = transform.position;
            OpenArmParticles[i].Play();
        }

        InGuideAnimation = true;

        StartCoroutine(CreateCutHoles());
        StartCoroutine(OpenArmKillCast());
    }

    public void CloseArms(bool instant = false)
    {
        for (int i = 0; i < AttackArms.Length; i++)
        {
            AttackArms[i].MakeArmFollowGuide();
        }

        float nTime = 0;
        if (instant)
            nTime = 1;
        
        GuideAnim.Play("ArmClose",-1,nTime);
        
        InGuideAnimation = true;
    }
    
    IEnumerator CreateCutHoles()
    {
        int holeCount = Random.Range(6, 12);
        float waitTime = 0.6F / (float)holeCount;
        float rndAngle = 0;
        float maxAngle = 360 / holeCount;
        for (int i = 0; i < holeCount; i++)
        {
            rndAngle += Random.Range(0, maxAngle);
            Instantiate(CutHole, transform.position + Vector3.forward * 2 + (Vector3)(Random.insideUnitCircle * 2F), Quaternion.Euler(0, 0, rndAngle));
            yield return new WaitForSeconds(waitTime);
        }
    }
    
    
    IEnumerator OpenArmKillCast()
    {
        for (int i = 0; i < 20; i++)
        {
            yield return new WaitForSeconds(0.01F);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 3F, hurtLayer);
            for (int j = 0; j < hits.Length; j++)
            {
                if (!hits[j].CompareTag("Player"))
                {
                    HurtableObject hurt = hits[j].GetComponent<HurtableObject>();
                    if (hurt != null)
                    {
                        if (hurt.Health > 0)
                        {
                            if (hurt.parentAgent != null)
                                hurt.parentAgent.KilledBySpinningArms = true;
                            
                            
                            hurt.HurtMe(hurt.transform.position, transform.position, null, 1000);
                        }
                    }
                }
            }
        }
    }

    public void GuideAnimFinished()
    {
        for (int i = 0; i < AttackArms.Length; i++)
        {
            AttackArms[i].RelaseArmFromGuide();
        }

        InGuideAnimation = false;
    }
    

}
