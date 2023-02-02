using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using FishNet.Object;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink
{
    public class PartyController : NetworkBehaviour
    {
        public static PartyController Instance;
        [SerializeField] [ReadOnly] private int alivePlayers = 0;
        public override void OnStartClient()
        {
            base.OnStartClient();
            Instance = this;

            StartCoroutine(GetPlayers());
        }

        IEnumerator GetPlayers()
        {
            while (true)
            {
                while (alivePlayers < 1)
                {
                    // wait until first player connects
                    yield return null;
                }

                while (alivePlayers > 0)
                {
                    // wait while there's at least one player alive
                    yield return null;
                }

                Game._instance.RespawnAllPlayers();
                yield return null;
            }
        }

        public void PlayerDied()
        {
            alivePlayers--;   
        }
        public void PlayerResurrected()
        {
            alivePlayers++;
        }
    }
}