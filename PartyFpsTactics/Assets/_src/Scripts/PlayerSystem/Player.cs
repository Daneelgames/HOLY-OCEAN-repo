using System;
using System.Collections;
using FishNet;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MrPink.Health;
using MrPink.Units;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink.PlayerSystem
{
    public class Player : NetworkBehaviour
    {
        [ReadOnly] [SerializeField] [SyncVar] private bool gameSceneLoaded = false;
        public bool GameSceneLoaded => gameSceneLoaded;

        [SerializeField, Required] private HealthController _health;

        [SerializeField, Required] private PlayerMovement _movement;


        [SerializeField, Required] private CommanderControls _commanderControls;

        [SerializeField, Required] private PlayerWeaponControls _weapon;

        [SerializeField, Required] private PlayerToolsControls toolControls;

        [SerializeField, Required] private PlayerInventory _inventory;

        [SerializeField, Required] private PlayerInteractor _interactor;


        [SerializeField, Required] private PlayerLookAround _lookAround;

        [SerializeField, Required] private PlayerVehicleControls _vehicleControls;
        [SerializeField, Required] private CharacterNeeds _characterNeeds;


        // FIXME дает слишком свободный доступ, к тому же объектов сейчас несколько
        public GameObject GameObject
            => gameObject;

        public Camera MainCamera
            => Game._instance.PlayerCamera;

        public PlayerMovement Movement
            => _movement;

        public HealthController Health
            => _health;

        public CommanderControls CommanderControls
            => _commanderControls;

        public PlayerWeaponControls Weapon
            => _weapon;

        public PlayerToolsControls ToolControls
            => toolControls;

        public PlayerInventory Inventory
            => _inventory;

        public PlayerInteractor Interactor
            => _interactor;

        public Vector3 Position
            => transform.position;


        public PlayerLookAround LookAround
            => _lookAround;

        public PlayerVehicleControls VehicleControls
            => _vehicleControls;

        public CharacterNeeds CharacterNeeds
            => _characterNeeds;

        public override void OnStartClient()
        {
            base.OnStartClient();

            Init();
        }
        
        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            /* Current owner can be found by using base.Owner. prevOwner
            * contains the connection which lost ownership. Value will be
            * -1 if there was no previous owner. */

            SetLocalPlayerInstance();
        }


        private void Init()
        {
            Game._instance.AddPlayer(this);
            _health.SetIsPlayerTrue();
        }

        public void SetLevelType(GameManager.LevelType levelType)
        {
            bool _gameSceneLoaded = levelType == GameManager.LevelType.Game;
            if (base.IsHost)
                gameSceneLoaded = _gameSceneLoaded;
            else
                RpcSetGameSceneLoaded(_gameSceneLoaded);
        }

        [ServerRpc]
        void RpcSetGameSceneLoaded(bool loaded)
        {
            gameSceneLoaded = loaded;
        }
        
        [Client(RequireOwnership = true)]
        void SetLocalPlayerInstance()
        {
            Debug.Log(gameObject.name + " PLAYER PUT HIMSELF AS LOCAL PLAYER");

            /*if (base.IsHost == false)
            {
                SceneLoader.Instance.gameObject.SetActive(false);
            }*/

            Game._instance.SetLocalPlayer(this);
        }

        public void Death(Transform killer)
        {
            Debug.Log("PLAYER DEATH, SHOULD DROP SHIT");
            Game.LocalPlayer.Inventory.DropAll();
            Game.LocalPlayer.Interactor.SetInteractionText(String.Empty);
            Movement.Death(killer);
            LookAround.Death(killer);
            Weapon.Death();
            VehicleControls.Death();
            ScoringSystem.Instance.CooldownToZero();
        }

        [Server]
        public void Respawn()
        {
            Debug.Log("SERVER RESPAWN. base isowner " + base.IsOwner);
           
            /*
             if (base.IsOwner)
            {
                if (respawnCoroutine != null)
                    StopCoroutine(respawnCoroutine);
                respawnCoroutine = StartCoroutine(RespawnPlayerOverTime());
                return;
            }
            */
            
            if (respawnCoroutine != null)
                StopCoroutine(respawnCoroutine);
            respawnCoroutine = StartCoroutine(RespawnPlayerOverTime());
            
            // this is for everyone else
            RpcRespawnOnClient();
        }

        [ObserversRpc(IncludeOwner = false)]
        void RpcRespawnOnClient()
        {
            Debug.Log("RpcRespawnOnClient RESPAWN");
            // should be done locally on each player
            if (respawnCoroutine != null)
                StopCoroutine(respawnCoroutine);
            respawnCoroutine = StartCoroutine(RespawnPlayerOverTime());
        }
        
        private Coroutine respawnCoroutine;

        IEnumerator RespawnPlayerOverTime()
        {
            var spawner = InstanceFinder.NetworkManager.gameObject.GetComponent<PlayerSpawner>(); 
            while (spawner == null)
            {
                Debug.Log("RespawnPlayerOverTime WAIT PlayerSpawner.Instance == null RESPAWN");
                spawner = InstanceFinder.NetworkManager.gameObject.GetComponent<PlayerSpawner>();
                yield return null;
            }
            Debug.Log("RespawnPlayerOverTime RESPAWN");
            yield return StartCoroutine(Game.LocalPlayer.Movement.TeleportToPosition(spawner.Spawns[Random.Range(0,spawner.Spawns.Length)].position + Vector3.up * 0.5f));
            yield return null;
            yield return new WaitForFixedUpdate();
            //Game.LocalPlayer.Resurrect();
            yield return new WaitForFixedUpdate();
            yield return null;
            Shop.Instance.OpenShop(0);
            Game.LocalPlayer.Resurrect(true);
            respawnCoroutine = null;
        }

        // called on client by player who interacted
        // with a dead friend
        // but this method actually runs on this non local player
        // on a machine who fired up the interaction
        public void ResurrectByOtherPlayerInteraction()
        {
            if (base.IsHost)
            {
                ResurrectPlayerOnClientRpc(false);        
            }
            else
            {
                ResurrectPlayerOnServerRpc(false);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void ResurrectPlayerOnServerRpc(bool fullHeal)
        {
            ResurrectPlayerOnClientRpc(fullHeal);
        }
        
        [ObserversRpc(IncludeOwner = true)]
        void ResurrectPlayerOnClientRpc(bool fullHeal)
        {
            Resurrect(fullHeal);
        }
        
        void Resurrect(bool fullHeal)
        {
            Debug.Log("PLAYER RESURRECT RESPAWN");
            //Game.LocalPlayer.Inventory.DropAll();
            Game.LocalPlayer.Interactor.SetInteractionText("");
            Health.Resurrect(fullHeal);
            Movement.Resurrect();
            LookAround.Resurrect();
            Weapon.Resurrect();
            CharacterNeeds.ResetNeeds();
        }

        void OnDestroy()
        {
            Game._instance.RemovePlayer(this);
        }
    }
}