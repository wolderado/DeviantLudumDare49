using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StabilityController : MonoBehaviour
{
    public Animator anim;

    public float StableDuration = 15;
    public float UnstableDuration = 25;

    public Image stableFiller;
    public Image unstableFiller;
    public AnimationCurve StableWalkSpeed;
    public AnimationCurve StableFOVCurve;
    public AnimationCurve UnstableFOVCurve;

    private PlayerController player;
    public ArmController arm;


    public float StableTimer = 0;
    public float UnstableTimer = 0;

    public CameraScript cam;


    public static StabilityController instance;

    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        player = PlayerController.instance;

        StableTimer = 2;
        UnstableTimer = UnstableDuration;
        
    }

    void Update()
    {
        if (player.Died)
            return;
   
        
        

        float ratio;
        
        if (player.CurrentStability == PlayerController.Stability.Stable)
        {
            StableTimer -= Time.deltaTime;
            if (StableTimer <= 0)
            {
                UnstableTimer = UnstableDuration;
                anim.Play("BecomeUnstable");
            }
            

            ratio = StableTimer / StableDuration;
            stableFiller.fillAmount = ratio;
            cam.StabilityFOVChange = StableFOVCurve.Evaluate(1F - ratio);

            player.StabilitySpeedMultiplier = StableWalkSpeed.Evaluate(ratio);
        }
        else
        {
            UnstableTimer -= Time.deltaTime;
            if (UnstableTimer <= 0)
            {
                StableTimer = StableDuration;
                anim.Play("BecomeStable");
            }
            
            ratio =  UnstableTimer / UnstableDuration;
            unstableFiller.fillAmount = ratio;
            cam.StabilityFOVChange = UnstableFOVCurve.Evaluate(1F - ratio);
            
            player.StabilitySpeedMultiplier = StableWalkSpeed.Evaluate(ratio);
        }
    }

    public void PlayerHurt(float damage)
    {
        UnstableTimer -= damage * 5F;
        if (UnstableTimer <= 0)
            UnstableTimer = 0;
    }

    public void ChangeToUnstableMoment()
    {
        player.CurrentStability = PlayerController.Stability.Unstable;
        arm.OpenArms();
    }
    
    public void ChangeToStableMoment()
    {
        player.CurrentStability = PlayerController.Stability.Stable;
        arm.CloseArms();
        MasterAI.instance.PlayerTurnedStable();
        
    }
}
