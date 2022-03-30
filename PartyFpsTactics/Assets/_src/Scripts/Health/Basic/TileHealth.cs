using System;
using System.Collections.Generic;
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
        private Vector3Int tileRoomCoordinates = Vector3Int.zero;
        public List<Collider> colliders;
        private Level _parentRoom;
        [SerializeField]
        private Rigidbody rb;

        private void Start()
        {
            if (prop)
                LevelGenerator.Instance.AddProp(this);
        }

        private void OnDestroy()
        {
            if (prop)
                LevelGenerator.Instance.RemoveProp(this);
        }

        public Rigidbody Rigidbody()
        {
            return rb;
        }

        public void SetTileRoomCoordinates(Vector3Int coords, Level parentRoom)
        {
            tileRoomCoordinates = coords;
            _parentRoom = parentRoom;
        }
        public void SetTileRoomCoordinates(Level parentRoom)
        {
            Vector3Int coords = Vector3Int.zero;
            // find local coords
            // transform.localPosition.x, transform.localPosition.y, transform.localPosition.z
            Debug.Log("Thin wall tile local pos " + transform.localPosition);
            tileRoomCoordinates = coords;
            _parentRoom = parentRoom;
        }

        public void AddRigidbody(int newHealth, PhysicMaterial mat = null)
        {
            if (rb) 
                return;

            // TODO нарушаем инкапсуляцию
            _health = newHealth;
            Debug.Log("Add Rigidbody");
            
            rb = gameObject.AddComponent<Rigidbody>();
            if (rb == null) return;
            rb.useGravity = true;
            foreach (var coll in colliders)
            {
                coll.material = mat;   
            }
            
            rb.isKinematic = false;
            rb.mass = 5;
            transform.localScale = Vector3.one * Random.Range(0.5f, 1f);
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
            
            if (_parentRoom != null && sendToLevelgen)
                LevelGenerator.Instance.TileDestroyed(_parentRoom, tileRoomCoordinates);
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
