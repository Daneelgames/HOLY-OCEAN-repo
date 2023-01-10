using System.Collections;
using System.Collections.Generic;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private bool followLocalPlayer = false;
    [HideIf("followLocalPlayer")]
    public Transform target;
    void Update()
    {
        if (followLocalPlayer)
        {
            if (Game._instance && Game.LocalPlayer)
                target = Game.LocalPlayer.transform;
        }
        
        if (!target)
            return;
        
        transform.position = target.position;
    }
}
