using System.Collections;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using UnityEngine;

namespace MrPink
{
    public class LookAtPlayer : MonoBehaviour
    {
        void Update()
        {
            if (Game._instance == null || Game.LocalPlayer == null)
            {
                return;
            }
            transform.LookAt(Game.LocalPlayer.MainCamera.transform.position);
        }
    }
}