using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreScript : MonoBehaviour
{

    public bool CoreBeaten = false;

    public CanvasGroup endCredits;
    public GameObject[] DestroyParticles;
    public GameObject spr;
    public ParticleSystem coreParticle;
    private float coreendcredAlpha = -2;
    
    private void Update()
    {
        if (CoreBeaten)
        {
            
            coreendcredAlpha += Time.deltaTime;
            endCredits.alpha = coreendcredAlpha;

            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                    Application.Quit();
            }
            

        }
    }

    public void PlayerHitCore()
    {
        spr.gameObject.SetActive(false);
        PlayerController.instance.WaitPlayer = 1000;

        for (int i = 0; i < DestroyParticles.Length; i++)
        {
            Instantiate(DestroyParticles[i], transform.position, Quaternion.identity);
        }

        coreParticle.Play();

        CoreBeaten = true;

    }
}
