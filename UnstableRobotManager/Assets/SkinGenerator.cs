using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinGenerator : MonoBehaviour
{
    public bool GenerateOnStart = true;
    public bool IsHuman = true;
    public Gradient SkinColor;
    public JobClothing[] JobBasedClothings;
    public AgentAI AliveAgent;
    
    [Header("References")]
    public SpriteRenderer[] Skins;
    public SpriteRenderer ClothingRend;

    [Header("Selected Indexes")]
    public Color SelectedSkinColor;
    public Sprite SelectedClothing;

    void Start()
    {
        if (GenerateOnStart)
            GenerateSkin();
    }
    
    void Update()
    {
        
    }


    public void GenerateSkin()
    {
        if (IsHuman)
        {
            float skinc = Random.value;
            SelectedSkinColor = SkinColor.Evaluate(skinc);

            if(AliveAgent != null)
                SelectedClothing = FindClothingSet(AliveAgent.MyJob).GiveMeRandomClothing();
            
            ApplySkin();
        }
    }
    
    
    public void SyncSkin(SkinGenerator other)
    {
        IsHuman = other.IsHuman;
        SelectedSkinColor = other.SelectedSkinColor;
        SelectedClothing = other.SelectedClothing;
        ApplySkin();
    }


    void ApplySkin()
    {
        if (IsHuman)
        {
            for (int i = 0; i < Skins.Length; i++)
            {
                Skins[i].color = SelectedSkinColor;
            }

            ClothingRend.sprite = SelectedClothing;
        }
    }


    JobClothing FindClothingSet(AgentAI.Job targetJob)
    {
        for (int i = 0; i < JobBasedClothings.Length; i++)
        {
            if (JobBasedClothings[i].job == targetJob)
                return JobBasedClothings[i];
        }

        return null;
    }

    [System.Serializable]
    public class JobClothing
    {
        public AgentAI.Job job;
        public Sprite[] clothings;

        public Sprite GiveMeRandomClothing()
        {
            return clothings[Random.Range(0, clothings.Length)];
        }
    }

}
