using System.Collections.Generic;
using MrPink.WeaponsSystem;
using UnityEngine;

namespace MrPink.Health
{
    // TileHealth
    public class TileHealth : BasicHealth
    {
        private Vector3Int tileRoomCoordinates = Vector3Int.zero;
        public List<Collider> colliders;
        private Level _parentRoom;
        private Rigidbody rb;

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
            if (rb) return;

            Health = newHealth;
            Debug.Log("Add Rigidbody");
            
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            foreach (var coll in colliders)
            {
                coll.material = mat;   
            }
            
            rb.isKinematic = false;
            rb.mass = 50;
            transform.localScale = Vector3.one * Random.Range(0.5f, 1f);
        }

        private void DestroyTile(ScoringActionType action = ScoringActionType.NULL)
        {
            if (action != ScoringActionType.NULL)
                ScoringSystem.Instance.RegisterAction(ScoringActionType.TileDestroyed, 1);

            LevelGenerator.Instance.DebrisParticles(transform.position);
            var hit = Physics.OverlapSphere(transform.position, 1, 1 << 6);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].transform == transform)
                    continue;

                LevelGenerator.Instance.TileDamaged(hit[i].transform);
            }
        }

        public override void Kill(bool combo)
        {
            if (IsDead)
                return;
            
            if (combo)
                ScoringSystem.Instance.RegisterAction(ScoringActionType.TileDestroyed);

            Damage(Health);
        }

        public override void Damage(int damage)
        {
            if (IsDead)
                return;

            Health -= damage;

            if (IsAlive)
                LevelGenerator.Instance.TileDamaged(this);
            else
                Death();
        }

        private void Death()
        {
            DestroyTile();
            if (_parentRoom != null)
                LevelGenerator.Instance.TileDestroyed(_parentRoom, tileRoomCoordinates);
            Destroy(gameObject);
        }

        public override CollisionTarget HandleDamageCollision(Vector3 collisionPosition, int damage, ScoringActionType actionOnHit)
        {
            if (IsAlive)
            {
                UnitsManager.Instance.RagdollTileExplosion(collisionPosition, actionOnHit);
                Damage(damage);
            }

            return CollisionTarget.Solid;
        }
    }
}
