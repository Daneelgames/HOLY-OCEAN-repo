using System.Collections.Generic;
using _src.Scripts.LevelGenerators;
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
        
        private void Start()
        {
            if (prop)
                LevelGenerator.Instance.AddProp(this);
        }

        private void OnDestroy()
        {
            if (prop)
                LevelGenerator.Instance.RemoveProp(this);
            
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
            Death(DamageSource.Environment ,false, false); 
        }
        private void DestroyTile(DamageSource source, bool deathParticles = true)
        {
            if (deathParticles)
                LevelGenerator.Instance.DebrisParticles(transform.position);
            
            var hit = Physics.OverlapSphere(transform.position, 1, GameManager.Instance.AllSolidsMask);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].transform == transform || hit[i].gameObject.isStatic)
                    continue;

                LevelGenerator.Instance.TileDamagedFeedback(hit[i].transform);
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
                if ( ! rb)
                    LevelGenerator.Instance.TileDamagedFeedback(this);
            }
            else
                Death(source);
        }

        private void Death(DamageSource source, bool sendToLevelgen = true, bool deathParticles = true)
        {
            if (source == DamageSource.Player)
                ScoringSystem.Instance.RegisterAction(ScoringActionType.TileDestroyed, 1);
            
            DestroyTile(source, deathParticles);
            
            if (parentLevel != null && sendToLevelgen)
                LevelGenerator.Instance.TileDestroyed(parentLevel, tileLevelCoordinates);
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
