using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeathManager : MonoBehaviour
{
    public BodyScript FullDeadBodyPrefab;
    public GameObject BloodRadiusFX;
    public GameObject BloodSplatFX;
    
    
    public static DeathManager instance;

    private void Awake()
    {
        instance = this;
    }
    
    public void CreateFullBody(AgentAI agent,Vector2 killerDir, SkinGenerator skin,Vector2 hurtPosition,HurtableObject damagedPiece,LineIK AttackedArm)
    {
        BodyScript body = Instantiate(FullDeadBodyPrefab, agent.transform.position - Vector3.up * 0.5F, Quaternion.identity, transform);


        bool VerticalDeath = false;

        if (damagedPiece.PieceName == "Torso")
        {
            body.entryHole.transform.position = hurtPosition;
            body.SetupHole();

            GameObject splatFX= Instantiate(BloodSplatFX, body.transform.position, Quaternion.identity);
            splatFX.transform.right = killerDir;

            if (AttackedArm != null)
            {
                if (Random.value < 0.25F)
                {
                    MakeBodyStuckToArm(body,AttackedArm,hurtPosition);
                    VerticalDeath = true;
                }
            }
        }

        if (VerticalDeath == false)
        {
            body.transform.Translate(-killerDir.normalized * 0.5F);
            body.transform.right = killerDir;

            var vec = body.transform.eulerAngles;
            vec.z = (Mathf.Round((vec.z / 180) * 180) + 90F);
            body.transform.eulerAngles = vec;

            float extraForce = 1;
            if (agent.KilledBySpinningArms)
                extraForce = 2;

            body.masterRb.AddForce(-killerDir.normalized * Random.Range(0F, 30F) * extraForce, ForceMode2D.Impulse);
            body.masterRb.AddTorque(Random.Range(-0.25F, 0.25F), ForceMode2D.Impulse);
            
            body.transform.Translate(0,0,1);
        }
        else
        {
            body.transform.rotation = agent.transform.rotation;
            
        }


        Instantiate(BloodRadiusFX, body.transform.position, Quaternion.identity);

        body.ProcessLimpDetach(damagedPiece,killerDir);
        body.Init(skin, !VerticalDeath);
        agent.DestroyAgent();
    }


    void MakeBodyStuckToArm(BodyScript body,LineIK arm, Vector2 hurtPosition)
    {
        body.transform.position = hurtPosition;
        body.transform.parent = arm.transform;
        body.transform.Rotate(0,0,Random.Range(-30,30));
        arm.MakeBodyStuckToThisArm(body);
    }
}
