using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEditor;
using Random = UnityEngine.Random;


public class MasterAI : MonoBehaviour
{
    public LayerMask SightMask;
    public List<AgentAI> AllDetectorAgents;
    public Transform[] EscapePoints;
    public Transform[] VIPPoints;


    public List<WorkNode> CivilWorkNodes;
    public List<WorkNode> SecurityWorkNodes;


    public float DifficultyIncreaseTime = 6;
    
    [Header("World AI")]
    public int MinCivilCount = 0;
    public int MinSecurityCount = 0;
    public AgentAI CivilPrefab;
    public AgentAI StickSecurityPrefab;
    public AgentAI GunmanSecurityPrefab;

    public List<AgentAI> CivilAgents;
    public List<AgentAI> SecurityAgents;
    public AgentAI VIPGREEN;
    public AgentAI VIPBLUE;
    public AgentAI VIPRED;

    public AgentAI CR_VIPGreen;
    public AgentAI CR_VIPRed;
    public AgentAI CR_VIPBlue;
    
    private int playerCheckFrame = 0;
    private PlayerController player;



    private float spawnTimer = 0F;
    private float nextSpawnWave = 0.5F;
    private float DifficultyTimer = 0;
    
    public int CurrentMinCivilCount = 0;
    public int CurrentMinSecurityCount = 0;


    public static MasterAI instance;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        instance = this;
    }

    void Start()
    {
        player = PlayerController.instance;;
        playerCheckFrame = 99;

        CurrentMinCivilCount = 10;
        CurrentMinSecurityCount = 15;
        
        spawnTimer = -5;


        Vector3 GreenVIPPos;
        Vector3 RedVIPPos;
        Vector3 BlueVIPPos;
        
        List<Transform> RndVIPPoints = new List<Transform>(VIPPoints);
        Transform selected = RndVIPPoints[Random.Range(0, RndVIPPoints.Count)];
        GreenVIPPos = selected.position;
        RndVIPPoints.Remove(selected);
        
        selected = RndVIPPoints[Random.Range(0, RndVIPPoints.Count)];
        RedVIPPos = selected.position;
        RndVIPPoints.Remove(selected);
        
        selected = RndVIPPoints[Random.Range(0, RndVIPPoints.Count)];
        BlueVIPPos = selected.position;
        RndVIPPoints.Remove(selected);


        CR_VIPGreen = Instantiate(VIPGREEN, GreenVIPPos,Quaternion.identity);
        CR_VIPRed = Instantiate(VIPRED, RedVIPPos,Quaternion.identity);
        CR_VIPBlue = Instantiate(VIPBLUE, BlueVIPPos,Quaternion.identity);


        for (int i = 0; i < CivilWorkNodes.Count; i++)
            CivilWorkNodes[i].IsAvaible = true;
        
        for (int i = 0; i < SecurityWorkNodes.Count; i++)
            SecurityWorkNodes[i].IsAvaible = true;

        StartCoroutine(CreateStartAgents());
    }

    IEnumerator CreateStartAgents()
    {
        for (int i = 0; i < (MinCivilCount-1)  + (MinSecurityCount-1); i++)
        {
            CreateNewAgent(true);
        }
        
        
        yield return null;
    }

    void Update()
    {

        
        
        DifficultyTimer += Time.deltaTime;
        if (DifficultyTimer > DifficultyIncreaseTime)
        {
            DifficultyTimer = 0;
            
            
            CurrentMinCivilCount++;
            if (CurrentMinCivilCount > MinCivilCount)
                CurrentMinCivilCount = MinCivilCount;
            
            CurrentMinSecurityCount+= Random.Range(1,3);
            if (CurrentMinSecurityCount > MinSecurityCount)
                CurrentMinSecurityCount = MinSecurityCount;

        }
        
        playerCheckFrame++;
        if (playerCheckFrame > 9)
        {
            playerCheckFrame = 0;
            Vector2 dirToPlayer;
            RaycastHit2D hit;

            for (int i = 0; i < AllDetectorAgents.Count; i++)
            {
                dirToPlayer = player.transform.position - AllDetectorAgents[i].transform.position;
                hit = Physics2D.Raycast(AllDetectorAgents[i].transform.position, dirToPlayer, AllDetectorAgents[i].SeeDistance,
                    SightMask);
                if (hit.collider != null)
                {
                    if (hit.collider.tag == "Player" && player.CurrentStability == PlayerController.Stability.Unstable)
                    {
                        AllDetectorAgents[i].PlayerSeen(player.transform.position);
                    }
                }
            }
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer > nextSpawnWave)
        {
            nextSpawnWave = Random.Range(0.3F, 0.7F);
            CreateNewAgent();
        }
    }

    void CreateNewAgent(bool FirstWave = false)
    {
        
        if (CivilAgents.Count < CurrentMinCivilCount)
        {
            int count = Random.Range(1,1);
            for (int i = 0; i < count; i++)
            {
                AgentAI newAI = Instantiate(CivilPrefab, GetRandomEscapePoint(), Quaternion.identity);
                newAI.myWorkNode = GetRandomWorkNode(ref CivilWorkNodes);
                newAI.myWorkNode.IsAvaible = false;
                if(FirstWave)
                    newAI.transform.position = newAI.myWorkNode.transform.position;
                CivilAgents.Add(newAI);
            }
        }



        if (SecurityAgents.Count < CurrentMinSecurityCount)
        {
            AgentAI securityPrefab = StickSecurityPrefab;
            if (Random.value < 0.5F)
                securityPrefab = GunmanSecurityPrefab;

            AgentAI newAI = Instantiate(securityPrefab, GetRandomEscapePoint(), Quaternion.identity);
            newAI.myWorkNode = GetRandomWorkNode(ref SecurityWorkNodes);
            newAI.myWorkNode.IsAvaible = false;
            
            if(FirstWave)
                newAI.transform.position = newAI.myWorkNode.transform.position;
            SecurityAgents.Add(newAI);
        }
    }


    public bool CanAgentSeePlayer(AgentAI agent)
    {
        Vector2 dirToPlayer;
        RaycastHit2D hit;
        dirToPlayer = player.transform.position - agent.transform.position;
        hit = Physics2D.Raycast(agent.transform.position, dirToPlayer, agent.SeeDistance,
            SightMask);
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Player" && player.CurrentStability == PlayerController.Stability.Unstable)
            {
                return true;
            }
        }

        return false;
    }

    public void PlayerTurnedStable()
    {
        for (int i = 0; i < MasterAI.instance.SecurityAgents.Count; i++)
        {
            Vector2 dir = SecurityAgents[i].transform.position - PlayerController.instance.transform.position;
            RaycastHit2D hit = Physics2D.Raycast(PlayerController.instance.transform.position, dir,
                SecurityAgents[i].SeeDistance, LayerMask.NameToLayer("Walls"));
            if (hit.collider == null)
            {
                SecurityAgents[i].SawPlayerStable = true;
            }

        }
    }
    
    
    
    public void AlertNearbyAgents()
    {

        for (int i = 0; i < MasterAI.instance.SecurityAgents.Count; i++)
        {
            if (SecurityAgents[i].IsCivil == false && SecurityAgents[i].CurrentState != AgentAI.AIState.Chase)
            {
                Vector2 dir = SecurityAgents[i].transform.position - PlayerController.instance.transform.position;
                if (dir.magnitude < SecurityAgents[i].SeeDistance)
                {
                    RaycastHit2D hit = Physics2D.Raycast(PlayerController.instance.transform.position, dir,
                        SecurityAgents[i].SeeDistance, LayerMask.NameToLayer("Walls"));
                    if (hit.collider == null)
                    {
                        if(SecurityAgents[i].CurrentState != AgentAI.AIState.Chase)
                            SecurityAgents[i].PlayerSeen(PlayerController.instance.transform.position);

                        if (player.CurrentStability == PlayerController.Stability.Stable)
                            SecurityAgents[i].SawPlayerStable = true;
                    }
                }
            }
        }
    }

    public Vector3 GiveMeEscapePoint( Vector2 myPos)
    {
        int closestIndex = -1;
        float closestDist = Mathf.Infinity;
        for (int i = 0; i < EscapePoints.Length; i++)
        {
            float dist = Vector2.Distance(EscapePoints[i].position, myPos);
            if (dist < closestDist)
            {
                closestIndex = i;
                closestDist = dist;
            }
        }


        return EscapePoints[closestIndex].position;
    }
    
    public Vector3 GetRandomEscapePoint()
    {
        return EscapePoints[Random.Range(0,EscapePoints.Length)].position;
    }
    
    public WorkNode GetRandomWorkNode(ref List<WorkNode> nodes)
    {
        List<WorkNode> avaibleNodes = new List<WorkNode>();
        for (int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i].IsAvaible)
                avaibleNodes.Add(nodes[i]);
        }

        //Debug.Log("AvaibleNodeCount" + avaibleNodes.Count);
        return avaibleNodes[Random.Range(0, avaibleNodes.Count)];
    }
    

}
