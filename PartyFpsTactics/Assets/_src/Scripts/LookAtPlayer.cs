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
            transform.LookAt(Game.Player.MainCamera.transform.position);
        }
    }
}