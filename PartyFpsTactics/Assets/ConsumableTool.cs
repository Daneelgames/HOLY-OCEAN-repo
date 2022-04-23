using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Unity.VisualScripting;
using UnityEngine;

public class ConsumableTool : MonoBehaviour
{
    public float addSleepNeed = -500;
    public float addFoodNeed = -500;
    public float addWaterNeed = -500;
    public void Consume(HealthController hc)
    {
        var needs = hc.gameObject.GetComponent<CharacterNeeds>();
        if (needs)
        {
            needs.AddToNeed(Need.NeedType.Sleep,addSleepNeed);
            needs.AddToNeed(Need.NeedType.Food,addFoodNeed);
            needs.AddToNeed(Need.NeedType.Water,addWaterNeed);
        }
    }
}
