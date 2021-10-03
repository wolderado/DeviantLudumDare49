using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BodyScript : MonoBehaviour
{
    public SkinGenerator bodySkin;
    public Rigidbody2D masterRb;
    public SpriteRenderer entryHole;

    public Gradient EntryHoleColor;
    private float entryHoleColorChangeTimer = 0F;
    public ParticleSystem EntryHoleDrip;
    public ParticleSystem CarriedDrip;
    
    public SpriteRenderer[] skinRends;
    public SpriteRenderer bloodPool;
    public SpriteRenderer clothing;
    
    private Color skinColor;
    private Color deadSkinColor;
    private float SkinColorChangeTimer = 0;
    public Transform BodyHolder;

    public DeadBodyPiece[] myPieces;

    public bool OnFloor = true;

    private float dropLevel = -1;

    private float ShakeTimer = 0;
    private bool OnArm = false;


    public void Init(SkinGenerator refSkin, bool isOnFloor)
    {
        CarriedDrip.Stop();
        bodySkin.SyncSkin(refSkin);

        OnFloor = isOnFloor;


        skinColor = refSkin.SelectedSkinColor;
        deadSkinColor = Color.Lerp(skinColor, new Color(0.4f, 0.39f, 0.36f), 0.7F);


        if (OnFloor == false)
        {
            OnArm = true;
            bloodPool.enabled = false;
            
            var em = EntryHoleDrip.emission;
            em.rateOverTime = 0;
            CarriedDrip.Play();
            for (int i = 0; i < skinRends.Length; i++)
            {
                skinRends[i].sortingOrder += 10;
            }

            clothing.sortingOrder += 10;
            entryHole.sortingOrder += 10;


            if (Random.value < 0.25F)
            {
                for (int i = 0; i < Random.Range(1,4); i++)
                {
                    myPieces[i].DetachPiece(Random.insideUnitCircle);
                }
            }



            BodyHolder.localPosition = new Vector2(0.02F, 0F);
        }
    }

    public void ProcessLimpDetach(HurtableObject hurtPiece,Vector2 pushDir)
    {
        if (hurtPiece.PieceName == "Torso")
        {
            masterRb.AddForce(pushDir.normalized * Random.Range(2F, 10F), ForceMode2D.Impulse);
        }
        
        for (int i = 0; i < myPieces.Length; i++)
        {
            myPieces[i].ProcessHit(hurtPiece,pushDir);


        }

    }
    
    public void SetupHole()
    {
        entryHole.enabled = true;
        entryHoleColorChangeTimer = Random.value;
    }

    private void Update()
    {
        if (entryHole.enabled)
        {
            if (entryHoleColorChangeTimer < 10F)
            {
                entryHoleColorChangeTimer += Time.deltaTime;
                entryHole.color = EntryHoleColor.Evaluate(entryHoleColorChangeTimer / 10F);

                if (OnFloor)
                {
                    var em = EntryHoleDrip.emission;
                    em.rateOverTime = 5 * 1F - (entryHoleColorChangeTimer / 10F);
                }
            }
        }


        if (SkinColorChangeTimer < 10F)
        {
            SkinColorChangeTimer += Time.deltaTime;
            for (int i = 0; i < skinRends.Length; i++)
            {
                skinRends[i].color = Color.Lerp(skinColor, deadSkinColor, SkinColorChangeTimer / 10F);
            }

            if (OnFloor)
            {
                Color bc = bloodPool.color;
                bc.a = (SkinColorChangeTimer / (10F));
                bloodPool.color = bc;
            }
        }
        
        if (OnArm)
        {
            ShakeTimer += Time.deltaTime;
            if (ShakeTimer > 0.02F)
            {
                ShakeTimer = 0;
                Vector2 pos = BodyHolder.localPosition;
                pos.x *= -1;
                BodyHolder.localPosition = pos;
            }
        }

        if (OnFloor == false)
        {
            if (transform.position.y > dropLevel && !OnArm)
            {
                BodyHolder.localPosition = Vector2.zero;
                Vector3 newPos = transform.position;
                newPos.y -= Time.deltaTime * 8F;
                if (newPos.y < dropLevel)
                {
                    newPos.y = dropLevel;
                    OnFloor = true;
                }
                

                transform.position = newPos;
            }
        }
    }

    public void DropBodyFromArm(float DropLevel)
    {
        OnArm = false;
        transform.parent = null;
        dropLevel = DropLevel;
        //CarriedDrip.Stop();

        float angle = 90;
        if (Random.value < 0.5F)
            angle *= -1;

        transform.eulerAngles = new Vector3(0, 0, angle);
        
        
        for (int i = 0; i < skinRends.Length; i++)
        {
            skinRends[i].sortingOrder -= 10;
        }

        clothing.sortingOrder -= 10;
        entryHole.sortingOrder -= 10;

    }
}
