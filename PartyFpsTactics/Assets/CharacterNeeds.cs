using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterNeeds : MonoBehaviour
{
    // 24 hours is 600 seconds is 10 minutes
    public List<Need> needs;
    //
    private bool sleeping = false;
    private bool eating = false;
    private bool drinking = false;
    public HealthController ownHealth;
    public int healthRegenOnNeeds = 3;
    public int healthDrainOnNeeds = 3;

    
    
    private void Start()
    {
        needsCoroutine = StartCoroutine(Needs());
    }

    private Coroutine needsCoroutine;

    IEnumerator Needs()
    {
        // ADD A WAY FOR NPC TO RESTORE THEIR NEEDS
        // THEN UNBREAK IT
        if (ownHealth != Game.Player.Health)
            yield break;
        ///////////////
        
        while (true)
        {
            if (ownHealth.health <= 0)
                yield break;
            
            float needsPool = 0;
            float needsCurrent = 0;
            for (int i = 0; i < needs.Count; i++)
            {
                var need = needs[i];

                needsPool += need.needMaxBase;
                needsCurrent += need.needCurrent;
                
                if (IsRestoringNeed(need.needType))
                    need.needCurrent -= need.needRegenBase;
                else
                    need.needCurrent += need.needCostBase;

                need.needCurrent = Mathf.Clamp(need.needCurrent, 0, need.needMaxBase);
                
                if (need.needCurrent == need.needMaxBase)
                {
                    // need's deprivation event
                    // set unconcious
                    ownHealth.DrainHealth(healthDrainOnNeeds);
                    PlayerUi.Instance.SetNeedColor(i, Color.red);
                }
            }

            if (needsCurrent > needsPool / needs.Count) 
            {
                // drain health
                ownHealth.DrainHealth(healthDrainOnNeeds);
                for (int i = 0; i < needs.Count; i++)
                {
                    PlayerUi.Instance.SetNeedColor(i, Color.red);   
                }
            }
            else
            {
                // regen health
                ownHealth.RegenHealth(healthRegenOnNeeds);
                
                for (int i = 0; i < needs.Count; i++)
                {
                    PlayerUi.Instance.SetNeedColor(i, Color.white);   
                }
            }
            
            yield return new WaitForSeconds(1);
        }
    }

    public void AddToNeed(Need.NeedType needType, float add)
    {
        for (int i = 0; i < needs.Count; i++)
        {
            var need = needs[i];
            if (need.needType == needType)
            {
                need.needCurrent += add;
            }
        }
    }

    public void SetSleeping(bool _sleeping)
    {
        sleeping = _sleeping;
    }

    bool IsRestoringNeed(Need.NeedType need)
    {
        bool restoring = false;
        switch (need)
        {
            case Need.NeedType.Sleep:
                if (sleeping)
                    restoring = true;
                break;
            case Need.NeedType.Food:
                if (eating)
                    restoring = true;
                break;
            case Need.NeedType.Water:
                if (drinking)
                    restoring = true;
                break;
        }

        return restoring;
    }
}

[Serializable]
public class Need
{
    public enum NeedType
    {
        Sleep, Food, Water   
    }

    public NeedType needType;
    
    public float needCurrent = 0;
    public float needMaxBase = 1200; // can not sleep for 2 days
    public float needCostBase = 1;
    public float needRegenBase = 3;
    
    // список интерактблс, регенящих нужду, с разными бонусами к регену
    
    // список модификаторов, меняющих статы персонажа 
}
