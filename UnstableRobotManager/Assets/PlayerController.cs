using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [Header("Variables")]
    public float WalkSpeed = 5;
    public Rigidbody2D rb;
    public Stability CurrentStability;
    public float MaxStableHealth = 5;
    public float StableHealth;
    public bool Died = false;
    public GameObject FinalDoor;



    public float StabilitySpeedMultiplier = 1;


    [Header("Game")] public List<string> foundKeys;
    
    [Header("References")]
    public LineIK DebugLineIK;
    public Animator WalkAnim;
    public HurtableObject myHurtable;
    public ParticleSystem[] hurtFX;
    public ArmController armScript;
    public SpriteRenderer skin;
    public CanvasGroup dieScreen;
    public CanvasGroup startScreenFade;
    
    private bool NoInput = false;
    private bool MoveToNearestPixel = false;

    private Vector2 externalVelocity;

    public float WaitPlayer = 2F;
    private float endScreenAlpha = -2;
    
    public static PlayerController instance;

    private void Awake()
    {
        instance = this;
        
    }
    
    void Start()
    {
        StableHealth = MaxStableHealth;
        WaitPlayer = 2.3F;

    }
    
    void Update()
    {

        
        if (Died)
        {
            endScreenAlpha += Time.deltaTime;
            dieScreen.alpha = endScreenAlpha;
            
            
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene(Application.loadedLevel);
            }
        }
        else
        {
            startScreenFade.alpha -= Time.deltaTime * 2;
        }
        
        if(foundKeys.Contains("GreenKey") && foundKeys.Contains("RedKey") && foundKeys.Contains("BlueKey"))
            FinalDoor.SetActive(false);
        
        
        float verticalInput = Input.GetAxisRaw("Vertical");
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        Vector2 inputVector = new Vector2(horizontalInput, verticalInput); 
        inputVector.Normalize();

        float speed = WalkSpeed * StabilitySpeedMultiplier;

        if (CurrentStability == Stability.Unstable)
        {
            StableHealth = MaxStableHealth;
            speed += 2;
        }
        
        

        Vector2 moveVector = inputVector * Time.deltaTime * speed;

        if (WaitPlayer > 0)
        {
            WaitPlayer -= Time.deltaTime;
            moveVector = Vector2.zero;
        }
        
        if (Died)
            moveVector = Vector2.zero;
        
        rb.MovePosition((Vector2)rb.position + moveVector + externalVelocity);

        externalVelocity = Vector2.Lerp(externalVelocity, Vector2.zero, Time.deltaTime * 8F);
        
        NoInput = Mathf.Abs(verticalInput) < 0.01F && Mathf.Abs(horizontalInput) < 0.01F;
        if (!NoInput)
        {
            /*DebugLineIK.UpperOffset = new Vector2(Mathf.Sign(inputVector.x), 3F);
            DebugLineIK.ForwardOffset = new Vector2(Mathf.Sign(inputVector.x) * 2F, 1F);*/
        }

        if (WaitPlayer <= 0 && !Died)
        {
            WalkAnim.SetFloat("Walk", inputVector.magnitude);
        }
        else
        {
            WalkAnim.SetFloat("Walk",0F);
        }
        
    }

    public void Hurt(float damage,Vector2 pushForce)
    {
        if (Died)
            return;
        
        PushPlayer(pushForce);
        CameraScript.instance.Shake(damage * 0.5F);
        MasterAI.instance.AlertNearbyAgents();

        if (CurrentStability == Stability.Stable)
        {
            //ActuallyDie
            StableHealth -= damage;
            if (StableHealth <= 0)
            {
                skin.color = new Color(0.73f, 0.73f, 0.73f);
                Died = true;
                rb.isKinematic = true;
                
                WalkAnim.Play("HumanDie");
            }
        }
        else
        {
            //Reduce unstabilitiy
            StabilityController.instance.PlayerHurt(damage);
        }
        
        
        
        for (int i = 0; i < hurtFX.Length; i++)
        {
            hurtFX[i].Play();
            hurtFX[i].transform.position = (Vector2)transform.position + pushForce ;
        }


        hurtFX[0].transform.position = transform.position;
        hurtFX[0].transform.right = -pushForce;
    }
    
    public void PushPlayer(Vector2 force)
    {
        externalVelocity += force;
    }

    public enum Stability
    {
        Stable,
        Unstable
    }
}
