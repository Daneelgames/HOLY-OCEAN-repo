using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _src.Scripts
{
    public class Respawner : NetworkBehaviour
    {
        public float corpseShredderY = -25;

        public static Respawner Instance;
        public bool spawn = false;
        private void Awake()
        {
            Instance = this;
        }


        void Update()
        {
            if (Game._instance == null || Game.LocalPlayer == null)
            {
                return;
            }
            if (Game.LocalPlayer.Position.y < corpseShredderY)
            {
                if (Game.LocalPlayer.Health.health > 0)
                    GameManager.Instance.KillPlayer();
                Game.LocalPlayer.transform.position = new Vector3(Game.LocalPlayer.transform.position.x, corpseShredderY + 5, Game.LocalPlayer.transform.position.z);
                return;
            }
            
            if (UnitsManager.Instance.HcInGame.Count < 1)
                return;
            
            for (int i = UnitsManager.Instance.HcInGame.Count - 1; i >= 0; i--)
            {
                if (i >= UnitsManager.Instance.HcInGame.Count)
                    continue;
            
                var corpse = UnitsManager.Instance.HcInGame[i];
                if (corpse.transform.position.y < corpseShredderY)
                {
                    if (corpse == Game.LocalPlayer.Health) // corpse is local owner
                    {
                        if (Game.LocalPlayer.Health.health > 0)
                            GameManager.Instance.KillPlayer();
                        Game.LocalPlayer.transform.position = new Vector3(Game.LocalPlayer.transform.position.x, corpseShredderY + 5, Game.LocalPlayer.transform.position.z);
                        return;
                    }

                    if (corpse.IsPlayer) // but not local owner
                    {
                        // nothing
                    }
                    else if (base.IsServer) // destroy mob on server
                    {
                        //ServerManager.Despawn(corpse.gameObject, DespawnType.Destroy);
                        Destroy(corpse.gameObject);   
                    }
                }
            }
        }
    }
}
