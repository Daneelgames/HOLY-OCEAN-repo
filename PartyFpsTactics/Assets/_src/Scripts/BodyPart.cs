using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

namespace MrPink
{
    public class BodyPart : MonoBehaviour
    {
        public HealthController hc;
        public int localHealth = 100;

        private Vector3Int tileRoomCoordinates = Vector3Int.zero;
        public List<Collider> colliders;
        private Level _parentRoom;
        [SerializeField]
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
            _parentRoom = _parentRoom;
        }

        public void AddRigidbody(int newHealth, PhysicMaterial mat = null)
        {
            if (rb) return;

            localHealth = newHealth;
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
            Death(false); 
        }
        
        public void DamageTile(int dmg, ScoringActionType action = ScoringActionType.NULL)
        {
            if (localHealth <= 0)
                return;
        
            localHealth -= dmg;
        
            if (localHealth <= 0)
            {
                if (action != ScoringActionType.NULL)
                {
                    ScoringSystem.Instance.RegisterAction(ScoringActionType.TileDestroyed, 1);
                }
            
                LevelGenerator.Instance.DebrisParticles(transform.position);
                var hit = Physics.OverlapSphere(transform.position, 1, 1 << 6);
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].transform == transform)
                        continue;
                
                    LevelGenerator.Instance.TileDamaged(hit[i].transform);
                }
                Death(); 
                return;
            }
            LevelGenerator.Instance.TileDamaged(this);
        }

        public void Kill(bool combo)
        {
            if (hc)
                hc.Damage(hc.health);
            else if (localHealth > 0)
            {
                if (combo)
                    ScoringSystem.Instance.RegisterAction(ScoringActionType.TileDestroyed);
                DamageTile(localHealth);
            }
        }

        void Death(bool sendToLevelgen = true)
        {
            if (sendToLevelgen && _parentRoom != null)
            {
                LevelGenerator.Instance.TileDestroyed(_parentRoom, tileRoomCoordinates);
            }
            Destroy(gameObject);
        }
    }
}
