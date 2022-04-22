using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CharacterNeeds : MonoBehaviour
{
    // 24 hours is 600 seconds is 10 minutes
    public List<Need> needs;
    //

    private void Start()
    {
        needsCoroutine = StartCoroutine(Needs());
    }

    private Coroutine needsCoroutine;

    IEnumerator Needs()
    {
        while (true)
        {
            for (int i = 0; i < needs.Count; i++)
            {
                var need = needs[i];
                
                if (IsRestoringNeed(need.needType))
                    need.needCurrent -= need.needRegenBase;
                else
                    need.needCurrent += need.needCostBase;

                need.needCurrent = Mathf.Clamp(need.needCurrent, 0, need.needMaxBase);
                
                if (need.needCurrent == need.needMaxBase)
                {
                    // need's deprivation event
                }
            }
            yield return new WaitForSeconds(1);
        }
    }

    private bool sleeping = false;
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
            case Need.NeedType.Nutrition:
                break;
            case Need.NeedType.Water:
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
        Sleep, Nutrition, Water   
    }

    public NeedType needType;


    public float needCurrent = 0;
    public float needMaxBase = 1200; // can not sleep for 2 days
    public float needCostBase = 1;
    public float needRegenBase = 3;
    
    // список интерактблс, регенящих нужду, с разными бонусами к регену
}
