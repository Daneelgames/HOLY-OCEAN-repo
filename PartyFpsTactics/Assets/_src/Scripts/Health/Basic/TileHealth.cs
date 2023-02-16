using System.Collections.Generic;
using _src.Scripts.LevelGenerators;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MrPink.Health
{
    // TileHealth
    public class TileHealth : BasicHealth
    {
        public bool ImmuneToDamage = false;
        [Tooltip("Will be added to Levelgen spawnedProps List and will be bumped by shots")]
        public bool prop = false;
        private Vector3Int tileLevelCoordinates = Vector3Int.zero;
        public Vector3Int TileLevelCoordinates => tileLevelCoordinates;
        public List<Collider> colliders;
        private Level parentLevel;
        public Level ParentLevel => parentLevel;
        
        [SerializeField, ChildGameObjectsOnly]
        private Rigidbody rb;
        
        [HideInInspector]
        public bool floorLevelTile = false;
        [HideInInspector]
        public bool ceilingLevelTile = false;

        public TileHealth supporterTile;
        public TileHealth supportedTile;
        public List<TileHealth> objectsToDestroyOnClash = new List<TileHealth>();

        public TileAttack tileAttack;
        public Rigidbody Rigidbody 
            => rb;




        [ContextMenu("GetColliders")]
        public void GetColliders()
        {
            
            colliders.Clear();
            var _colliders = GetComponents<Collider>();
            foreach (var collider1 in _colliders)
            {
                colliders.Add(collider1);
            }
        }
        
        void Start()
        {   
            if (prop)
                IslandSpawner.Instance.GetClosestTileBuilding(transform.position).AddProp(this);
        }


        private void OnDestroy()
        {
            if (prop)
                IslandSpawner.Instance.GetClosestTileBuilding(transform.position)?.RemoveProp(this);
            
            if (parentLevel)
                parentLevel.allTiles.Remove(this);

            if (supportedTile)
                supportedTile.supporterTile = null;
            if (supporterTile)
                supporterTile.supportedTile = null;

            DestroyDependantObjects();
        }

        void DestroyDependantObjects()
        {
            for (int i = 0; i < objectsToDestroyOnClash.Count; i++)
            {
                var o = objectsToDestroyOnClash[i];
                if (o)
                    o.Kill(DamageSource.Environment);
            }
            objectsToDestroyOnClash.Clear();
        }

        
        public void SetTileRoomCoordinates(Vector3Int coords, Level _parentLevel)
        {
            tileLevelCoordinates = coords;
            parentLevel = _parentLevel;
        }

        public void ActivateRigidbody(int newHealth, PhysicMaterial mat = null, bool setLayer11 = false, float explosionForce = -1, bool addTileAttack = false)
        {
            if (rb && ! rb.isKinematic)  // Такое у предметов 
                return;

            DestroyDependantObjects();
            gameObject.tag = GameManager.Instance.portableObjectTag;
            
            // TODO нарушаем инкапсуляцию
            _health = newHealth;

            if (setLayer11)
                gameObject.layer = 11;
            
            
            foreach (var coll in colliders)
                coll.material = mat;

            if (supportedTile)
                supportedTile.supporterTile = null;
            if (supporterTile)
                supporterTile.supportedTile = null;

            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();

            if (addTileAttack)
            {
                var _tileAttack = gameObject.AddComponent<TileAttack>();
                _tileAttack.rb = rb;
                tileAttack = _tileAttack;
                tileAttack.dangerous = true;
            }
            
            SetRigidbodyState(ref rb, false, true, 5, 1, 1);
            transform.localScale = Vector3.one * Random.Range(0.5f, 1f);
            if (explosionForce > 0)
                rb.AddExplosionForce(explosionForce, rb.transform.position, 10);
        }
        
		public void DestroyTileFromGenerator()
        {
            // no death effects
            Death(DamageSource.Environment ,false, false, false); 
        }
        private void DestroyTileParticlesAndShake(DamageSource source, bool deathParticles = true)
        {
            if (deathParticles)
                IslandSpawner.Instance.GetClosestTileBuilding(transform.position).DebrisParticles(transform.position);
            
            if (source != DamageSource.Player)
                return;

            var closestBuilding = IslandSpawner.Instance.GetClosestTileBuilding(transform.position);
            
            var hit = Physics.OverlapSphere(transform.position, 1, GameManager.Instance.AllSolidsMask, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].transform == transform || hit[i].gameObject.isStatic)
                    continue;

                closestBuilding.TileDamagedFeedback(hit[i].transform);
            }
        }

        public override void Damage(int damage, DamageSource source)
        {
            if (ImmuneToDamage)
                return;
            
            if (IsDead)
                return;

            base.Damage(damage, source);

            if (IsAlive)
            {
                if (!rb)
                    IslandSpawner.Instance.GetClosestTileBuilding(transform.position).TileDamagedFeedback(this);
            }
            else
                Death(source);
        }

        public void Death(DamageSource source, bool sendToLevelgen = true, bool deathParticles = true, bool rpcSync = true)
        {
            if (source == DamageSource.Player)
                ScoringSystem.Instance.RegisterAction(ScoringActionType.TileDestroyed, 1);
            
            if (deathParticles)
                DestroyTileParticlesAndShake(DamageSource.Environment, true);
            
            if (sendToLevelgen && parentLevel != null)
                IslandSpawner.Instance.GetClosestTileBuilding(transform.position).TileDestroyed(parentLevel, this);
            
            // sync tile destruction by ehhh position?
            if (rpcSync)
                GameVoxelModifier.Instance.TileDestroyedInWorld(transform.position);

            if (gameObject.layer == 6) // if SOLIDS - it counts in navmesh.
            {
                // spawn nav obstacle cuz we dont update navmeshes anymore
                GameManager.Instance.SpawnTileNavObstacle(transform);
            }
            
            Destroy(gameObject);
        }

        
        public override CollisionTarget HandleDamageCollision(Vector3 collisionPosition, DamageSource source, int damage, ScoringActionType actionOnHit)
        {
            if (IsAlive)
            {
                // UnitsManager.Instance.RagdollTileExplosion(collisionPosition, actionOnHit);
                Damage(damage, source);
            }

            return CollisionTarget.Solid;
        }

        private static void SetRigidbodyState(
            ref Rigidbody rigidbody,
            bool isKinematic,
            bool useGravity,
            float mass,
            float drag,
            float angularDrag
        )
        {
            if (rigidbody == null)
                return;
            
            rigidbody.isKinematic = isKinematic;
            rigidbody.useGravity = useGravity;
            rigidbody.mass = mass;
            rigidbody.drag = drag;
            rigidbody.angularDrag = angularDrag;
        }

#if UNITY_EDITOR

        [ContextMenu("Добавить RB блока")]
        private void LinkBodyParts()
        {
            if (rb != null)
            {
                Debug.LogWarning("Уже есть");
                return;
            }

            rb = gameObject.AddComponent<Rigidbody>();
            SetRigidbodyState(ref rb, true, false, 0, 0, 0);

            EditorUtility.SetDirty(rb);
            AssetDatabase.SaveAssets();
            
            Debug.Log("Добавлен RB блока");
        }
        
#endif
    }
}
