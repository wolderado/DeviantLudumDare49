using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtableObject : MonoBehaviour
{
    public float Health = 1;
    public AgentAI parentAgent;
    public string PieceName;

    public PlayerController parentPlayer;

    public CoreScript targetCore;
    

    public delegate void onHurtDelegate(Vector2 pos, Vector2 origin, LineIK atkArm);

    public event onHurtDelegate OnHurt;
    

    public void HurtMe(Vector2 damagePos,Vector2 originPos,LineIK armAttackObject,float Damage)
    {
        Health--;
        
        Vector2 dmgDir = originPos - (Vector2)transform.position;
        
        if (parentAgent != null)
        {
            parentAgent.Hurt(this,dmgDir,damagePos,armAttackObject);
        }
        
        if (parentPlayer != null)
        {
            parentPlayer.Hurt(1, -dmgDir.normalized * 0.5F);
        }

        if (targetCore != null)
        {
            targetCore.PlayerHitCore();
        }

        
        
        OnHurt?.Invoke(damagePos,originPos,armAttackObject);
    }
}
