using System.Collections;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(Player.MainCamera.transform.position);
    }
}