using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class PlayerVisual : NetworkBehaviour
{
    [SerializeField] private Transform networkVisual;
    
    public override void OnStartClient() { 
        base.OnStartClient();

        if (base.IsOwner)
            networkVisual.gameObject.SetActive(false);
        else
            networkVisual.gameObject.SetActive(true);
    }
}