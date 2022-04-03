using System;
using System.Collections.Generic;
using _src.Scripts.LevelGenerators;
using MrPink.WeaponsSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink.Health
{
    // TileHealth
    public class TileHealth : BasicHealth
    {
        [Tooltip("Will be added to Levelgen spawnedProps List and will be bumped by shots")]
        public bool prop = false;
        private Vector3Int tileLevelCoordinates = Vector3Int.zero;
        public Vector3Int TileLevelCoordinates => tileLevelCoordinates;
        public List<Collider> colliders;
        private Level parentLevel;
        [SerializeField]
        private Rigidbody rb;
        [HideInInspector]
        public bool floorLevelTile = false;

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
        }

        public Rigidbody Rigidbody()
        {
            return rb;
        }

        public void SetTileRoomCoordinates(Vector3Int coords, Level _parentLevel)
        {
            tileLevelCoordinates = coords;
            parentLevel = _parentLevel;
        }

        public void AddRigidbody(int newHealth, PhysicMaterial mat = null, bool setLayer11 = false, float explosionForce = -1)
        {
            if (rb) 
                return;

            // TODO нарушаем инкапсуляцию
            _health = newHealth;

            if (setLayer11)
                gameObject.layer = 11;
            
            //Debug.Log("Add Rigidbody");
            rb = gameObject.AddComponent<Rigidbody>();
            if (rb == null) return;
            rb.useGravity = true;
            foreach (var coll in colliders)
            {
                coll.material = mat;   
            }
            
            rb.isKinematic = false;
            rb.mass = 5;
            rb.drag = 1;
            rb.angularDrag = 1;
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
            
            var hit = Physics.OverlapSphere(transform.position, 1, 1 << 6);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].transform == transform)
                    continue;

                LevelGenerator.Instance.TileDamaged(hit[i].transform);
            }
        }

        public override void Damage(int damage, DamageSource source)
        {
            if (IsDead)
                return;

            base.Damage(damage, source);

            if (IsAlive)
                LevelGenerator.Instance.TileDamaged(this);
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
                UnitsManager.Instance.RagdollTileExplosion(collisionPosition, actionOnHit);
                Damage(damage, source);
            }

            return CollisionTarget.Solid;
        }
    }
}
